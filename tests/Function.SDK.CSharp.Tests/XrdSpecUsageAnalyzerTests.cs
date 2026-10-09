using System;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Function.SDK.CSharp.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Function.SDK.CSharp.Tests;

public class XrdSpecUsageAnalyzerTests
{
    private const string ModelSource = """
        namespace Function.SDK.CSharp.SourceGenerator.Models.platform.example.com
        {
            public sealed class V1alpha1XStorageBucket
            {
                public V1alpha1XStorageBucketSpec Spec { get; set; } = new();
            }

            public sealed class V1alpha1XStorageBucketSpec
            {
                public V1alpha1XStorageBucketSpecParameters Parameters { get; set; } = new();
                public V1alpha1XStorageBucketSpecCrossplane Crossplane { get; set; } = new();
            }

            public sealed class V1alpha1XStorageBucketSpecParameters
            {
                public string Acl { get; set; } = "";
                public string Location { get; set; } = "";
                public bool Versioning { get; set; }
                public bool Public { get; set; }
                public System.Collections.Generic.List<V1alpha1XStorageBucketSpecParameterItem> Items { get; set; } = new();
                public V1alpha1XStorageBucketSpecParameterItem[] ArrayItems { get; set; } = System.Array.Empty<V1alpha1XStorageBucketSpecParameterItem>();
                public System.Collections.Generic.Dictionary<string, V1alpha1XStorageBucketSpecParameterItem> ItemMap { get; set; } = new();
            }

            public sealed class V1alpha1XStorageBucketSpecParameterItem
            {
                public string Name { get; set; } = "";
                public bool Enabled { get; set; }
            }

            public sealed class V1alpha1XStorageBucketSpecCrossplane
            {
                public string CompositionRef { get; set; } = "";
            }
        }

        namespace Function.SDK.CSharp
        {
            public sealed class Request
            {
            }

            public static class RequestExtensions
            {
                public static T GetObservedCompositeResource<T>(this Request request)
                    where T : new() => new();
            }
        }
        """;

    [Fact]
    public async Task ReportsUnreferencedSpecField()
    {
        var diagnostics = await Analyze(CreateSource(includeVersioning: false));

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(XrdSpecUsageAnalyzer.DiagnosticId, diagnostic.Id);
        Assert.Contains("Parameters.Versioning", diagnostic.GetMessage(CultureInfo.InvariantCulture));
    }

    [Fact]
    public async Task DoesNotReportWhenEverySpecFieldIsReferenced()
    {
        var diagnostics = await Analyze(CreateSource(includeVersioning: true));

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task DoesNotReportWhenSpecFieldIsReferencedInPropertyPattern()
    {
        var diagnostics = await Analyze(
            CreateSource(includeVersioning: true, useVersioningPropertyPattern: true));

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task ReportsUnreferencedPropertiesInsideCollections()
    {
        var diagnostics = await Analyze(
            CreateSource(includeVersioning: true, includeCollectionElementProperties: false));

        var messages = diagnostics
            .Select(diagnostic => diagnostic.GetMessage(CultureInfo.InvariantCulture))
            .ToArray();

        Assert.Equal(6, messages.Length);
        Assert.Contains(messages, message => message.Contains("Parameters.Items[].Name", StringComparison.Ordinal));
        Assert.Contains(messages, message => message.Contains("Parameters.Items[].Enabled", StringComparison.Ordinal));
        Assert.Contains(messages, message => message.Contains("Parameters.ArrayItems[].Name", StringComparison.Ordinal));
        Assert.Contains(messages, message => message.Contains("Parameters.ArrayItems[].Enabled", StringComparison.Ordinal));
        Assert.Contains(messages, message => message.Contains("Parameters.ItemMap{}.Name", StringComparison.Ordinal));
        Assert.Contains(messages, message => message.Contains("Parameters.ItemMap{}.Enabled", StringComparison.Ordinal));
    }

    private static string CreateSource(
        bool includeVersioning,
        bool useVersioningPropertyPattern = false,
        bool includeCollectionElementProperties = true)
    {
        var versioning = includeVersioning
            ? useVersioningPropertyPattern
                ? "_ = xr.Spec.Parameters is { Versioning: true };"
                : "_ = xr.Spec.Parameters.Versioning;"
            : string.Empty;
        var collectionItemProperties = includeCollectionElementProperties
            ? "_ = item.Name; _ = item.Enabled;"
            : "_ = item.ToString();";

        return $$"""
            {{ModelSource}}

            namespace Sample
            {
                using Function.SDK.CSharp;
                using Function.SDK.CSharp.SourceGenerator.Models.platform.example.com;

                public sealed class RunFunctionService
                {
                    public void Run(Request request)
                    {
                        var xr = request.GetObservedCompositeResource<V1alpha1XStorageBucket>();
                        _ = xr.Spec.Parameters.Acl;
                        _ = xr.Spec.Parameters.Location;
                        {{versioning}}
                        _ = xr.Spec.Parameters.Public;
                        foreach (var item in xr.Spec.Parameters.Items)
                        {
                            {{collectionItemProperties}}
                        }
                        foreach (var item in xr.Spec.Parameters.ArrayItems)
                        {
                            {{collectionItemProperties}}
                        }
                        foreach (var item in xr.Spec.Parameters.ItemMap.Values)
                        {
                            {{collectionItemProperties}}
                        }
                    }
                }
            }
            """;
    }

    private static async Task<ImmutableArray<Diagnostic>> Analyze(string source)
    {
        var trustedAssemblies = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;
        var references = trustedAssemblies
            .Split(Path.PathSeparator)
            .Where(path => !Path.GetFileName(path).StartsWith("Function.SDK.CSharp", StringComparison.OrdinalIgnoreCase))
            .Select(path => MetadataReference.CreateFromFile(path));

        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            new CSharpParseOptions(LanguageVersion.Latest));

        var compilation = CSharpCompilation.Create(
            "XrdSpecUsageAnalyzerTest",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        Assert.DoesNotContain(
            compilation.GetDiagnostics(),
            diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var invocation = syntaxTree.GetRoot().DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Single(candidate =>
                semanticModel.GetSymbolInfo(candidate).Symbol is IMethodSymbol candidateMethod &&
                candidateMethod.Name == "GetObservedCompositeResource");
        var method = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        Assert.NotNull(method);
        Assert.Equal("GetObservedCompositeResource", method.Name);

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new XrdSpecUsageAnalyzer());
        return await compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync().ConfigureAwait(false);
    }
}
