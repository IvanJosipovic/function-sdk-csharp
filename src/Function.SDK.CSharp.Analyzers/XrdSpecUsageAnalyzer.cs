using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Function.SDK.CSharp.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class XrdSpecUsageAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "FSDK001";

    private const string GeneratedModelsNamespace = "Function.SDK.CSharp.SourceGenerator.Models";
    private const string CompositeResourceMethodName = "GetObservedCompositeResource";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "XRD spec field is not mapped",
        "XRD spec field '{0}' is not referenced by the function",
        "Function.SDK.CSharp",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: new[] { WellKnownDiagnosticTags.CompilationEnd });

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(startContext =>
        {
            var resourceUses = new ConcurrentBag<CompositeResourceUse>();
            var referencedProperties = new ConcurrentBag<IPropertySymbol>();

            startContext.RegisterSyntaxNodeAction(
                syntaxContext => AnalyzeInvocation(syntaxContext, resourceUses),
                SyntaxKind.InvocationExpression);

            startContext.RegisterOperationAction(
                operationContext =>
                {
                    var propertyReference = operationContext.Operation switch
                    {
                        IPropertyReferenceOperation property => property,
                        IPropertySubpatternOperation { Member: IPropertyReferenceOperation property } => property,
                        _ => null
                    };

                    if (propertyReference is not null &&
                        IsGeneratedModelType(propertyReference.Property.ContainingType))
                    {
                        referencedProperties.Add(propertyReference.Property.OriginalDefinition);
                    }
                },
                OperationKind.PropertyReference,
                OperationKind.PropertySubpattern);

            startContext.RegisterCompilationEndAction(
                endContext => AnalyzeCompilation(endContext, resourceUses, referencedProperties));
        });
    }

    private static void AnalyzeInvocation(
        SyntaxNodeAnalysisContext context,
        ConcurrentBag<CompositeResourceUse> resourceUses)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        if (context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol method ||
            method.Name != CompositeResourceMethodName ||
            method.TypeArguments.Length != 1 ||
            method.TypeArguments[0] is not INamedTypeSymbol resourceType ||
            !IsGeneratedModelType(resourceType))
        {
            return;
        }

        var genericName = invocation.Expression
            .DescendantNodesAndSelf()
            .OfType<GenericNameSyntax>()
            .FirstOrDefault(name => name.Identifier.ValueText == method.Name);

        var location = genericName?.TypeArgumentList.Arguments.FirstOrDefault()?.GetLocation()
            ?? invocation.GetLocation();

        resourceUses.Add(new CompositeResourceUse(resourceType, location));
    }

    private static void AnalyzeCompilation(
        CompilationAnalysisContext context,
        ConcurrentBag<CompositeResourceUse> resourceUses,
        ConcurrentBag<IPropertySymbol> referencedProperties)
    {
        var usesByType = new Dictionary<INamedTypeSymbol, Location>(SymbolEqualityComparer.Default);
        foreach (var use in resourceUses)
        {
            if (!usesByType.ContainsKey(use.ResourceType))
            {
                usesByType.Add(use.ResourceType, use.Location);
            }
        }

        if (usesByType.Count == 0)
        {
            return;
        }

        var referencedSet = new HashSet<IPropertySymbol>(
            referencedProperties,
            SymbolEqualityComparer.Default);

        foreach (var use in usesByType)
        {
            var specProperty = use.Key.GetMembers()
                .OfType<IPropertySymbol>()
                .FirstOrDefault(property => property.Name == "Spec");

            if (specProperty?.Type is not INamedTypeSymbol specType)
            {
                continue;
            }

            var fields = new List<SpecField>();
            CollectSpecFields(
                specType,
                string.Empty,
                new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default),
                fields);

            foreach (var field in fields)
            {
                if (!referencedSet.Contains(field.Property.OriginalDefinition))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, use.Value, field.Path));
                }
            }
        }
    }

    private static void CollectSpecFields(
        INamedTypeSymbol type,
        string parentPath,
        HashSet<INamedTypeSymbol> visitedTypes,
        List<SpecField> fields)
    {
        if (!visitedTypes.Add(type))
        {
            return;
        }

        try
        {
            foreach (var property in GetSpecProperties(type))
            {
                var path = string.IsNullOrEmpty(parentPath)
                    ? property.Name
                    : $"{parentPath}.{property.Name}";

                var nestedType = GetGeneratedModelType(property.Type, out var collectionPathSuffix);
                if (nestedType is not null &&
                    !visitedTypes.Contains(nestedType) &&
                    GetSpecProperties(nestedType).Any())
                {
                    CollectSpecFields(nestedType, path + collectionPathSuffix, visitedTypes, fields);
                    continue;
                }

                fields.Add(new SpecField(property, path));
            }
        }
        finally
        {
            visitedTypes.Remove(type);
        }
    }

    private static INamedTypeSymbol? GetGeneratedModelType(
        ITypeSymbol type,
        out string collectionPathSuffix)
    {
        var visitedTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        collectionPathSuffix = string.Empty;

        while (visitedTypes.Add(type))
        {
            if (type is IArrayTypeSymbol arrayType)
            {
                type = arrayType.ElementType;
                collectionPathSuffix += "[]";
                continue;
            }

            if (type is not INamedTypeSymbol namedType)
            {
                break;
            }

            if (namedType.TypeKind == TypeKind.Class && IsGeneratedModelType(namedType))
            {
                return namedType;
            }

            var dictionaryType = namedType.AllInterfaces
                .Append(namedType)
                .FirstOrDefault(IsGenericDictionary);
            if (dictionaryType is not null)
            {
                type = dictionaryType.TypeArguments[1];
                collectionPathSuffix += "{}";
                continue;
            }

            var enumerableType = namedType.AllInterfaces
                .Append(namedType)
                .FirstOrDefault(IsGenericEnumerable);
            if (enumerableType is not null)
            {
                type = enumerableType.TypeArguments[0];
                collectionPathSuffix += "[]";
                continue;
            }

            break;
        }

        collectionPathSuffix = string.Empty;
        return null;
    }

    private static bool IsGenericDictionary(INamedTypeSymbol type)
    {
        var definition = type.OriginalDefinition;
        return definition.TypeArguments.Length == 2 &&
            definition.ContainingNamespace.ToDisplayString() == "System.Collections.Generic" &&
            (definition.MetadataName == "IDictionary`2" ||
             definition.MetadataName == "IReadOnlyDictionary`2");
    }

    private static bool IsGenericEnumerable(INamedTypeSymbol type)
    {
        var definition = type.OriginalDefinition;
        return definition.TypeArguments.Length == 1 &&
            definition.ContainingNamespace.ToDisplayString() == "System.Collections.Generic" &&
            definition.MetadataName == "IEnumerable`1";
    }

    private static IEnumerable<IPropertySymbol> GetSpecProperties(INamedTypeSymbol type)
    {
        return type.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(property =>
                property.DeclaredAccessibility == Accessibility.Public &&
                !property.IsStatic &&
                !property.IsIndexer &&
                // The generator injects Crossplane control fields that aren't part of the user's XRD.
                !property.Name.Equals("Crossplane", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsGeneratedModelType(INamedTypeSymbol type)
    {
        var namespaceName = type.ContainingNamespace.ToDisplayString();
        return namespaceName.StartsWith(GeneratedModelsNamespace + ".", StringComparison.Ordinal);
    }

    private sealed class CompositeResourceUse
    {
        public CompositeResourceUse(INamedTypeSymbol resourceType, Location location)
        {
            ResourceType = resourceType;
            Location = location;
        }

        public INamedTypeSymbol ResourceType { get; }

        public Location Location { get; }
    }

    private sealed class SpecField
    {
        public SpecField(IPropertySymbol property, string path)
        {
            Property = property;
            Path = path;
        }

        public IPropertySymbol Property { get; }

        public string Path { get; }
    }
}
