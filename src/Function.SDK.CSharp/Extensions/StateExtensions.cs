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
    /// Adds Resource or merges with an existing one. Sets the ApiVersion and Kind if not set on the object.
    /// If the resource with the same key already exists, the existing resource will be merged with the new one using protobuf merge semantics.
    /// </summary>
    /// <param name="state">The state to update.</param>
    /// <param name="key">The key of the resource.</param>
    /// <param name="obj">The Kubernetes object to add or update.</param>
    public static void AddOrUpdate(this State state, string key, IKubernetesObject obj)
    {
        if (string.IsNullOrEmpty(obj.ApiVersion) || string.IsNullOrEmpty(obj.Kind))
        {
            obj.Initialize();
        }

        var kubeObj = Struct.Parser.ParseJson(KubernetesJson.Serialize(obj));

        if (state.Resources.TryGetValue(key, out Resource? value))
        {
            value.Resource_.MergeFrom(kubeObj);
        }
        else
        {
            state.Resources[key] = new()
            {
                Resource_ = kubeObj
            };
        }
    }
}
