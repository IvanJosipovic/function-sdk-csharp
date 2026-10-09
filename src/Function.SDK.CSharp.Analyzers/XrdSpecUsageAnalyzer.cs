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
            var referencedProperties = new ConcurrentBag<ReferencedProperty>();

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
                        referencedProperties.Add(
                            new ReferencedProperty(
                                propertyReference,
                                GetReferenceContext(operationContext.Operation)));
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

        var rootVariable = invocation.Ancestors()
            .OfType<VariableDeclaratorSyntax>()
            .FirstOrDefault(variable => variable.Initializer?.Value.Span.Contains(invocation.Span) == true);
        var rootSymbol = rootVariable is null
            ? null
            : context.SemanticModel.GetDeclaredSymbol(rootVariable, context.CancellationToken) as ILocalSymbol;

        resourceUses.Add(new CompositeResourceUse(resourceType, location, rootSymbol));
    }

    private static void AnalyzeCompilation(
        CompilationAnalysisContext context,
        ConcurrentBag<CompositeResourceUse> resourceUses,
        ConcurrentBag<ReferencedProperty> referencedProperties)
    {
        var usesByType = new Dictionary<INamedTypeSymbol, Location>(SymbolEqualityComparer.Default);
        var rootResources = new Dictionary<ISymbol, INamedTypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var use in resourceUses)
        {
            if (!usesByType.ContainsKey(use.ResourceType))
            {
                usesByType.Add(use.ResourceType, use.Location);
            }

            if (use.RootSymbol is not null)
            {
                rootResources[use.RootSymbol] = use.ResourceType;
            }
        }

        if (usesByType.Count == 0)
        {
            return;
        }

        var referencedPaths = new Dictionary<INamedTypeSymbol, HashSet<string>>(SymbolEqualityComparer.Default);
        var unqualifiedReferences = new HashSet<IPropertySymbol>(SymbolEqualityComparer.Default);
        foreach (var reference in referencedProperties)
        {
            var path = reference.Context is IPropertySubpatternOperation propertySubpattern
                ? GetPropertySubpatternPath(propertySubpattern, rootResources, context.Compilation)
                : GetResourcePath(
                    reference.PropertyReference,
                    reference.Context,
                    rootResources,
                    context.Compilation);

            if (path is null)
            {
                unqualifiedReferences.Add(reference.PropertyReference.Property.OriginalDefinition);
                continue;
            }

            if (!referencedPaths.TryGetValue(path.ResourceType, out var paths))
            {
                paths = new HashSet<string>(StringComparer.Ordinal);
                referencedPaths.Add(path.ResourceType, paths);
            }

            paths.Add(path.Path);
        }

        var fieldsByType = new Dictionary<INamedTypeSymbol, List<SpecField>>(SymbolEqualityComparer.Default);
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

            fieldsByType.Add(use.Key, fields);
        }

        var collectionPathsByProperty = new Dictionary<IPropertySymbol, HashSet<string>>(
            SymbolEqualityComparer.Default);
        foreach (var use in fieldsByType)
        {
            foreach (var field in use.Value.Where(field => IsCollectionElementPath(field.Path)))
            {
                if (!collectionPathsByProperty.TryGetValue(field.Property.OriginalDefinition, out var paths))
                {
                    paths = new HashSet<string>(StringComparer.Ordinal);
                    collectionPathsByProperty.Add(field.Property.OriginalDefinition, paths);
                }

                paths.Add($"{use.Key.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}|{field.Path}");
            }
        }

        foreach (var use in usesByType)
        {
            if (!fieldsByType.TryGetValue(use.Key, out var fields))
            {
                continue;
            }

            referencedPaths.TryGetValue(use.Key, out var paths);
            foreach (var field in fields)
            {
                var isReferencedByPath = paths?.Contains(field.Path) == true;
                var hasUniqueCollectionPath =
                    !IsCollectionElementPath(field.Path) ||
                    !collectionPathsByProperty.TryGetValue(field.Property.OriginalDefinition, out var fieldPaths) ||
                    fieldPaths.Count == 1;
                var isReferencedBySymbol =
                    hasUniqueCollectionPath &&
                    unqualifiedReferences.Contains(field.Property.OriginalDefinition);

                if (!isReferencedByPath && !isReferencedBySymbol)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, use.Value, field.Path));
                }
            }
        }
    }

    private static IOperation GetReferenceContext(IOperation operation)
    {
        for (var current = operation; current is not null; current = current.Parent)
        {
            if (current is IPropertySubpatternOperation)
            {
                return current;
            }
        }

        return operation;
    }

    private static ResourcePropertyPath? GetResourcePath(
        IOperation? operation,
        IOperation referenceContext,
        IReadOnlyDictionary<ISymbol, INamedTypeSymbol> rootResources,
        Compilation compilation,
        HashSet<ISymbol>? visitedLocals = null)
    {
        while (operation is IConversionOperation conversion)
        {
            operation = conversion.Operand;
        }

        switch (operation)
        {
            case IParenthesizedOperation parenthesized:
                return GetResourcePath(
                    parenthesized.Operand,
                    referenceContext,
                    rootResources,
                    compilation,
                    visitedLocals);

            case IPropertyReferenceOperation propertyReference:
            {
                var instancePath = GetResourcePath(
                    propertyReference.Instance,
                    referenceContext,
                    rootResources,
                    compilation,
                    visitedLocals);
                if (instancePath is null)
                {
                    return null;
                }

                if (propertyReference.Property.Name == "Spec" &&
                    SymbolEqualityComparer.Default.Equals(
                        propertyReference.Property.ContainingType,
                        instancePath.ResourceType))
                {
                    return instancePath;
                }

                if (propertyReference.Property.IsIndexer &&
                    propertyReference.Instance?.Type is ITypeSymbol instanceType)
                {
                    var indexedModel = GetGeneratedModelType(instanceType, out var indexerSuffix);
                    if (indexedModel is not null && indexerSuffix.Length > 0)
                    {
                        return instancePath.AppendSuffix(indexerSuffix);
                    }
                }

                return IsGeneratedModelType(propertyReference.Property.ContainingType)
                    ? instancePath.AppendProperty(propertyReference.Property.Name)
                    : instancePath;
            }

            case IArrayElementReferenceOperation arrayElement:
            {
                var arrayPath = GetResourcePath(
                    arrayElement.ArrayReference,
                    referenceContext,
                    rootResources,
                    compilation,
                    visitedLocals);
                if (arrayPath is null)
                {
                    return null;
                }

                var elementModel = GetGeneratedModelType(arrayElement.ArrayReference.Type!, out var arraySuffix);
                return elementModel is not null
                    ? arrayPath.AppendSuffix(arraySuffix)
                    : arrayPath;
            }

            case ILocalReferenceOperation localReference:
            {
                if (rootResources.TryGetValue(localReference.Local, out var resourceType))
                {
                    return new ResourcePropertyPath(resourceType, string.Empty);
                }

                var collectionElementPath = GetForeachElementPath(
                    localReference.Local,
                    referenceContext,
                    rootResources,
                    compilation);
                if (collectionElementPath is not null)
                {
                    return collectionElementPath;
                }

                visitedLocals ??= new HashSet<ISymbol>(SymbolEqualityComparer.Default);
                if (!visitedLocals.Add(localReference.Local))
                {
                    return null;
                }

                try
                {
                    var initializer = GetLocalInitializer(localReference.Local, compilation);
                    return initializer is null
                        ? null
                        : GetResourcePath(
                            initializer,
                            referenceContext,
                            rootResources,
                            compilation,
                            visitedLocals);
                }
                finally
                {
                    visitedLocals.Remove(localReference.Local);
                }
            }

            case IInvocationOperation invocation
                when invocation.TargetMethod.Name == CompositeResourceMethodName &&
                    invocation.TargetMethod.TypeArguments.Length == 1 &&
                    invocation.TargetMethod.TypeArguments[0] is INamedTypeSymbol resourceType &&
                    IsGeneratedModelType(resourceType):
                return new ResourcePropertyPath(resourceType, string.Empty);
        }

        return null;
    }

    private static ResourcePropertyPath? GetForeachElementPath(
        ILocalSymbol local,
        IOperation referenceContext,
        IReadOnlyDictionary<ISymbol, INamedTypeSymbol> rootResources,
        Compilation compilation)
    {
        for (var current = referenceContext; current is not null; current = current.Parent)
        {
            if (current is not IForEachLoopOperation foreachLoop ||
                foreachLoop.Syntax is not ForEachStatementSyntax foreachStatement)
            {
                continue;
            }

            var semanticModel = compilation.GetSemanticModel(foreachStatement.SyntaxTree);
            if (SymbolEqualityComparer.Default.Equals(
                    semanticModel.GetDeclaredSymbol(foreachStatement),
                    local))
            {
                return GetCollectionElementPath(
                    foreachLoop.Collection,
                    referenceContext,
                    rootResources,
                    compilation);
            }
        }

        return null;
    }

    private static ResourcePropertyPath? GetCollectionElementPath(
        IOperation collection,
        IOperation referenceContext,
        IReadOnlyDictionary<ISymbol, INamedTypeSymbol> rootResources,
        Compilation compilation)
    {
        ResourcePropertyPath? bestPath = null;
        foreach (var operation in DescendantsAndSelf(collection))
        {
            if (operation is not IPropertyReferenceOperation propertyReference ||
                !IsGeneratedModelType(propertyReference.Property.ContainingType))
            {
                continue;
            }

            var elementModel = GetGeneratedModelType(propertyReference.Property.Type, out var suffix);
            if (elementModel is null || suffix.Length == 0)
            {
                continue;
            }

            var collectionPath = GetResourcePath(
                propertyReference,
                referenceContext,
                rootResources,
                compilation);
            if (collectionPath is null)
            {
                continue;
            }

            var candidatePath = collectionPath.AppendSuffix(suffix);
            if (bestPath is null || candidatePath.Path.Length > bestPath.Path.Length)
            {
                bestPath = candidatePath;
            }
        }

        if (bestPath is not null)
        {
            return bestPath;
        }

        if (collection is ILocalReferenceOperation localReference)
        {
            var initializer = GetLocalInitializer(localReference.Local, compilation);
            if (initializer is not null)
            {
                return GetCollectionElementPath(
                    initializer,
                    referenceContext,
                    rootResources,
                    compilation);
            }
        }

        if (collection.Type is not null)
        {
            var elementModel = GetGeneratedModelType(collection.Type, out var suffix);
            if (elementModel is not null && suffix.Length > 0)
            {
                var collectionPath = GetResourcePath(
                    collection,
                    referenceContext,
                    rootResources,
                    compilation);
                return collectionPath?.AppendSuffix(suffix);
            }
        }

        return null;
    }

    private static IOperation? GetLocalInitializer(ILocalSymbol local, Compilation compilation)
    {
        var variable = local.DeclaringSyntaxReferences
            .Select(reference => reference.GetSyntax())
            .OfType<VariableDeclaratorSyntax>()
            .FirstOrDefault();
        if (variable?.Initializer is null)
        {
            return null;
        }

        return compilation.GetSemanticModel(variable.SyntaxTree).GetOperation(variable.Initializer.Value);
    }

    private static IEnumerable<IOperation> DescendantsAndSelf(IOperation operation)
    {
        yield return operation;
        foreach (var child in operation.ChildOperations)
        {
            foreach (var descendant in DescendantsAndSelf(child))
            {
                yield return descendant;
            }
        }
    }

    private static ResourcePropertyPath? GetPropertySubpatternPath(
        IPropertySubpatternOperation propertySubpattern,
        IReadOnlyDictionary<ISymbol, INamedTypeSymbol> rootResources,
        Compilation compilation)
    {
        var subpatterns = new List<IPropertySubpatternOperation>();
        IIsPatternOperation? isPattern = null;
        for (var current = (IOperation?)propertySubpattern; current is not null; current = current.Parent)
        {
            if (current is IPropertySubpatternOperation subpattern)
            {
                subpatterns.Add(subpattern);
            }

            if (current is IIsPatternOperation pattern)
            {
                isPattern = pattern;
                break;
            }
        }

        if (isPattern is null)
        {
            return null;
        }

        subpatterns.Reverse();
        var path = GetResourcePath(isPattern.Value, propertySubpattern, rootResources, compilation);
        if (path is null)
        {
            return null;
        }

        foreach (var subpattern in subpatterns)
        {
            if (subpattern.Member is not IPropertyReferenceOperation member)
            {
                return null;
            }

            if (IsGeneratedModelType(member.Property.ContainingType))
            {
                path = path.AppendProperty(member.Property.Name);
            }

            if (subpattern.Pattern.Syntax is ListPatternSyntax)
            {
                var elementModel = GetGeneratedModelType(member.Property.Type, out var suffix);
                if (elementModel is not null)
                {
                    path = path.AppendSuffix(suffix);
                }
            }
        }

        return path;
    }

    private static bool IsCollectionElementPath(string path)
    {
        return path.Contains("[]", StringComparison.Ordinal) ||
            path.Contains("{}", StringComparison.Ordinal);
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
        public CompositeResourceUse(
            INamedTypeSymbol resourceType,
            Location location,
            ILocalSymbol? rootSymbol)
        {
            ResourceType = resourceType;
            Location = location;
            RootSymbol = rootSymbol;
        }

        public INamedTypeSymbol ResourceType { get; }

        public Location Location { get; }

        public ILocalSymbol? RootSymbol { get; }
    }

    private sealed class ReferencedProperty
    {
        public ReferencedProperty(
            IPropertyReferenceOperation propertyReference,
            IOperation context)
        {
            PropertyReference = propertyReference;
            Context = context;
        }

        public IPropertyReferenceOperation PropertyReference { get; }

        public IOperation Context { get; }
    }

    private sealed class ResourcePropertyPath
    {
        public ResourcePropertyPath(INamedTypeSymbol resourceType, string path)
        {
            ResourceType = resourceType;
            Path = path;
        }

        public INamedTypeSymbol ResourceType { get; }

        public string Path { get; }

        public ResourcePropertyPath AppendProperty(string propertyName)
        {
            var path = string.IsNullOrEmpty(Path)
                ? propertyName
                : $"{Path}.{propertyName}";
            return new ResourcePropertyPath(ResourceType, path);
        }

        public ResourcePropertyPath AppendSuffix(string suffix)
        {
            return new ResourcePropertyPath(ResourceType, Path + suffix);
        }
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
