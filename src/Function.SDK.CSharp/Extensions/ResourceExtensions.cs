using Apiextensions.Fn.Proto.V1;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using k8s;

namespace Function.SDK.CSharp;

/// <summary>
/// Resource extensions contains utilities for working with Resources.
/// </summary>
public static class ResourceExtensions
{
    /// <summary>
    /// Converts the Resource to a Kubernetes object of the specified type.
    /// </summary>
    /// <param name="resource">The Resource to convert.</param>
    /// <typeparam name="T">The type of the Kubernetes object.</typeparam>
    /// <returns>The Kubernetes object of the specified type.</returns>
    public static T GetKubeResource<T>(this Resource resource)
    {
        var json = JsonFormatter.Default.Format(resource.Resource_);

        return KubernetesJson.Deserialize<T>(json);
    }

    /// <summary>
    /// Gets the Resource condition by name
    /// </summary>
    /// <param name="resource">The Resource to get the condition from.</param>
    /// <param name="conditionType">The type of the condition to get.</param>
    /// <returns>The condition as a Struct, or null if not found.</returns>
    public static Struct? GetCondition(this Resource resource, string conditionType)
    {
        if (!resource.Resource_.Fields.TryGetValue("status", out var status)
            || status.KindCase != Value.KindOneofCase.StructValue
            || !status.StructValue.Fields.TryGetValue("conditions", out var conditions)
            || conditions.KindCase != Value.KindOneofCase.ListValue)
        {
            return null;
        }

        foreach (var condition in conditions.ListValue.Values)
        {
            if (condition.KindCase == Value.KindOneofCase.StructValue
                && condition.StructValue.Fields.TryGetValue("type", out var type)
                && type.KindCase == Value.KindOneofCase.StringValue
                && type.StringValue == conditionType)
            {
                return condition.StructValue;
            }
        }

        return null;
    }
}
