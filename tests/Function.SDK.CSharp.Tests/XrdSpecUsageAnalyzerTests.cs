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

    private static string CreateSource(bool includeVersioning, bool useVersioningPropertyPattern = false)
    {
        var versioning = includeVersioning
            ? useVersioningPropertyPattern
                ? "_ = xr.Spec.Parameters is { Versioning: true };"
                : "_ = xr.Spec.Parameters.Versioning;"
            : string.Empty;

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

        var invocation = syntaxTree.GetRoot().DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Single();
        var method = compilation.GetSemanticModel(syntaxTree).GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        Assert.NotNull(method);
        Assert.Equal("GetObservedCompositeResource", method.Name);

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new XrdSpecUsageAnalyzer());
        return await compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync().ConfigureAwait(false);
    }
}
