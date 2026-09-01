using Apiextensions.Fn.Proto.V1;
using Google.Protobuf.WellKnownTypes;
using k8s;

namespace Function.SDK.CSharp;

/// <summary>
/// State extensions contains utilities for working with State objects.
/// </summary>
public static class StateExtensions
{
    /// <summary>
    /// Adds a resource or replaces the manifest of an existing one. Sets the ApiVersion and Kind if not set on the object.
    /// Replacing only the manifest preserves outer protocol state such as readiness and connection details.
    /// </summary>
    /// <param name="state">The state to update.</param>
    /// <param name="key">The key of the resource.</param>
    /// <param name="obj">The Kubernetes object to add or update.</param>
    public static void AddOrUpdate(this State state, string key, IKubernetesObject obj)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(obj);

        if (string.IsNullOrEmpty(obj.ApiVersion) || string.IsNullOrEmpty(obj.Kind))
        {
            obj.Initialize();
        }

        var serialized = Struct.Parser.ParseJson(KubernetesJson.Serialize(obj));

        if (state.Resources.TryGetValue(key, out Resource? existing))
        {
            existing.Resource_ = serialized;
        }
        else
        {
            state.Resources[key] = new()
            {
                Resource_ = serialized
            };
        }
    }
}
