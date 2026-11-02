using System.Collections;
using System.Text.Json;
using Cel;
using k8s;
using YamlDotNet.RepresentationModel;

namespace Function.SDK.CSharp.Sample.Tests;

internal sealed class XrdCelValidation
{
    private readonly IReadOnlyDictionary<string, IReadOnlyList<CompiledCelRule>> _rulesByApiVersion;

    public XrdCelValidation(string xrdPath)
    {
        var yaml = new YamlStream();
        using (var reader = File.OpenText(xrdPath))
        {
            yaml.Load(reader);
        }

        if (yaml.Documents.Count == 0 || yaml.Documents[0].RootNode is not YamlMappingNode root)
        {
            throw new InvalidDataException("The XRD YAML must contain a mapping document.");
        }

        var spec = GetRequiredMapping(root, "spec");
        var group = GetRequiredScalar(spec, "group");
        var versions = GetRequiredSequence(spec, "versions");
        var rulesByApiVersion = new Dictionary<string, IReadOnlyList<CompiledCelRule>>();
        var celEnvironment = new CelEnvironment(null, null);

        foreach (var versionNode in versions.Children)
        {
            if (versionNode is not YamlMappingNode version)
            {
                throw new InvalidDataException("Each XRD version must be a YAML mapping.");
            }

            var versionName = GetRequiredScalar(version, "name");
            var schema = GetRequiredMapping(
                GetRequiredMapping(version, "schema"),
                "openAPIV3Schema");
            var discoveredRules = new List<DiscoveredCelRule>();
            CollectSchemaRules(schema, [], discoveredRules);

            var compiledRules = discoveredRules
                .Select(rule => new CompiledCelRule(
                    rule.ScopePath,
                    rule.Rule,
                    rule.Message,
                    celEnvironment.Compile(rule.Rule)))
                .ToArray();

            rulesByApiVersion.Add($"{group}/{versionName}", compiledRules);
        }

        _rulesByApiVersion = rulesByApiVersion;
    }

    public int RuleCount => _rulesByApiVersion.Values.Sum(rules => rules.Count);

    public IReadOnlyList<XrdCelValidationFailure> Validate(IKubernetesObject xr)
    {
        using var json = JsonDocument.Parse(KubernetesJson.Serialize(xr));
        if (ConvertJsonValue(json.RootElement) is not Dictionary<string, object?> instance ||
            !instance.TryGetValue("apiVersion", out var apiVersionValue) ||
            apiVersionValue is not string apiVersion ||
            !_rulesByApiVersion.TryGetValue(apiVersion, out var rules))
        {
            throw new InvalidDataException("The XR must serialize to an object with an apiVersion in the XRD.");
        }

        var failures = new List<XrdCelValidationFailure>();
        foreach (var rule in rules)
        {
            foreach (var scopedValue in ResolveScopeValues(instance, rule.ScopePath))
            {
                var result = rule.Program.Invoke(new Dictionary<string, object?>
                {
                    ["self"] = scopedValue.Value
                });

                if (result is not bool isValid)
                {
                    throw new InvalidDataException($"CEL rule '{rule.Rule}' did not return a boolean.");
                }

                if (!isValid)
                {
                    failures.Add(new XrdCelValidationFailure(
                        rule.Rule,
                        rule.Message ?? $"CEL validation failed: {rule.Rule}",
                        scopedValue.InstancePath));
                }
            }
        }

        return failures;
    }

    private static object? ConvertJsonValue(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                return element.EnumerateObject().ToDictionary(
                    property => property.Name,
                    property => ConvertJsonValue(property.Value));
            case JsonValueKind.Array:
                return element.EnumerateArray().Select(ConvertJsonValue).ToArray();
            case JsonValueKind.String:
                return element.GetString();
            case JsonValueKind.Number:
                return element.TryGetInt64(out var integer) ? integer : element.GetDouble();
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return null;
            default:
                throw new InvalidDataException($"Unsupported JSON value kind '{element.ValueKind}'.");
        }
    }

    private static void CollectSchemaRules(
        YamlNode schemaNode,
        IReadOnlyList<SchemaPathSegment> scopePath,
        ICollection<DiscoveredCelRule> rules)
    {
        if (schemaNode is not YamlMappingNode schema)
        {
            throw new InvalidDataException("An XRD schema node must be a YAML mapping.");
        }

        var validationsNode = GetChild(schema, "x-kubernetes-validations");
        if (validationsNode is not null)
        {
            if (validationsNode is not YamlSequenceNode validations)
            {
                throw new InvalidDataException("x-kubernetes-validations must be a YAML sequence.");
            }

            foreach (var validationNode in validations.Children)
            {
                if (validationNode is not YamlMappingNode validation)
                {
                    throw new InvalidDataException("Each XRD CEL validation must be a YAML mapping.");
                }

                var rule = GetRequiredScalar(validation, "rule");
                rules.Add(new DiscoveredCelRule(
                    scopePath,
                    rule,
                    GetScalar(validation, "message")));
            }
        }

        if (GetChild(schema, "properties") is YamlMappingNode properties)
        {
            foreach (var property in properties.Children)
            {
                if (property.Key is not YamlScalarNode { Value: string propertyName })
                {
                    throw new InvalidDataException("XRD schema property names must be strings.");
                }

                CollectSchemaRules(
                    property.Value,
                    scopePath.Append(SchemaPathSegment.Property(propertyName)).ToArray(),
                    rules);
            }
        }

        if (GetChild(schema, "items") is YamlMappingNode items)
        {
            CollectSchemaRules(
                items,
                scopePath.Append(SchemaPathSegment.ArrayItem()).ToArray(),
                rules);
        }

        if (GetChild(schema, "additionalProperties") is YamlMappingNode additionalProperties)
        {
            CollectSchemaRules(
                additionalProperties,
                scopePath.Append(SchemaPathSegment.MapValue()).ToArray(),
                rules);
        }
    }

    private static IEnumerable<ScopedValue> ResolveScopeValues(
        object? value,
        IReadOnlyList<SchemaPathSegment> scopePath)
    {
        return ResolveScopeValues(value, scopePath, 0, "$");
    }

    private static IEnumerable<ScopedValue> ResolveScopeValues(
        object? value,
        IReadOnlyList<SchemaPathSegment> scopePath,
        int segmentIndex,
        string instancePath)
    {
        if (value is null)
        {
            yield break;
        }

        if (segmentIndex == scopePath.Count)
        {
            yield return new ScopedValue(value, instancePath);
            yield break;
        }

        var segment = scopePath[segmentIndex];
        switch (segment.Kind)
        {
            case SchemaPathSegmentKind.Property:
                if (value is IDictionary<string, object?> properties &&
                    properties.TryGetValue(segment.Name!, out var propertyValue))
                {
                    foreach (var scopedValue in ResolveScopeValues(
                        propertyValue,
                        scopePath,
                        segmentIndex + 1,
                        $"{instancePath}.{segment.Name}"))
                    {
                        yield return scopedValue;
                    }
                }

                break;
            case SchemaPathSegmentKind.ArrayItem:
                if (value is IEnumerable items && value is not string &&
                    value is not IDictionary<string, object?>)
                {
                    var index = 0;
                    foreach (var item in items)
                    {
                        foreach (var scopedValue in ResolveScopeValues(
                            item,
                            scopePath,
                            segmentIndex + 1,
                            $"{instancePath}[{index}]"))
                        {
                            yield return scopedValue;
                        }

                        index++;
                    }
                }

                break;
            case SchemaPathSegmentKind.MapValue:
                if (value is IDictionary<string, object?> map)
                {
                    foreach (var entry in map)
                    {
                        foreach (var scopedValue in ResolveScopeValues(
                            entry.Value,
                            scopePath,
                            segmentIndex + 1,
                            $"{instancePath}['{entry.Key}']"))
                        {
                            yield return scopedValue;
                        }
                    }
                }

                break;
        }
    }

    private static YamlMappingNode GetRequiredMapping(YamlMappingNode mapping, string key)
    {
        return GetChild(mapping, key) as YamlMappingNode
            ?? throw new InvalidDataException($"The XRD is missing mapping '{key}'.");
    }

    private static YamlSequenceNode GetRequiredSequence(YamlMappingNode mapping, string key)
    {
        return GetChild(mapping, key) as YamlSequenceNode
            ?? throw new InvalidDataException($"The XRD is missing sequence '{key}'.");
    }

    private static string GetRequiredScalar(YamlMappingNode mapping, string key)
    {
        return GetScalar(mapping, key) is { Length: > 0 } value
            ? value
            : throw new InvalidDataException($"The XRD is missing scalar '{key}'.");
    }

    private static string? GetScalar(YamlMappingNode mapping, string key)
    {
        return GetChild(mapping, key) is YamlScalarNode scalar ? scalar.Value : null;
    }

    private static YamlNode? GetChild(YamlMappingNode mapping, string key)
    {
        return mapping.Children.FirstOrDefault(entry =>
            entry.Key is YamlScalarNode scalar && scalar.Value == key).Value;
    }

    private enum SchemaPathSegmentKind
    {
        Property,
        ArrayItem,
        MapValue
    }

    private sealed record SchemaPathSegment(SchemaPathSegmentKind Kind, string? Name = null)
    {
        public static SchemaPathSegment Property(string name) => new(SchemaPathSegmentKind.Property, name);
        public static SchemaPathSegment ArrayItem() => new(SchemaPathSegmentKind.ArrayItem);
        public static SchemaPathSegment MapValue() => new(SchemaPathSegmentKind.MapValue);
    }

    private sealed record DiscoveredCelRule(
        IReadOnlyList<SchemaPathSegment> ScopePath,
        string Rule,
        string? Message);

    private sealed record CompiledCelRule(
        IReadOnlyList<SchemaPathSegment> ScopePath,
        string Rule,
        string? Message,
        CelProgramDelegate Program);

    private sealed record ScopedValue(object Value, string InstancePath);
}

internal sealed record XrdCelValidationFailure(string Rule, string Message, string InstancePath);
