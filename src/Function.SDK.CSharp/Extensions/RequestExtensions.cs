using Apiextensions.Fn.Proto.V1;
using Google.Protobuf.WellKnownTypes;
using k8s;
using k8s.Models;

namespace Function.SDK.CSharp;

/// <summary>
/// Package response contains utilities for working with RunFunctionRequests.
/// </summary>
public static partial class RequestExtensions
{
    /// <summary>
    /// The default TTL for which a RunFunctionResponse may be cached.
    /// </summary>
    public static readonly Duration DefaultTTL = Duration.FromTimeSpan(TimeSpan.FromMinutes(1));

    /// <summary>
    /// To bootstraps a response to the supplied request. It automatically copies the desired state from the request.
    /// </summary>
    /// <param name="request">The request to respond to.</param>
    /// <returns>A response to the supplied request.</returns>
    public static RunFunctionResponse To(this RunFunctionRequest request)
    {
        return request.To(DefaultTTL);
    }

    /// <summary>
    /// To bootstraps a response to the supplied request. It automatically copies the desired state from the request.
    /// </summary>
    /// <param name="request">The request to respond to.</param>
    /// <param name="ttl">How long Crossplane may optionally cache the response.</param>
    /// <returns>A response to the supplied request.</returns>
    public static RunFunctionResponse To(this RunFunctionRequest request, Duration ttl)
    {
        var resp = new RunFunctionResponse()
        {
            Meta = new ResponseMeta()
            {
                Tag = request.Meta?.Tag ?? "",
                Ttl = ttl
            },
            Desired = request.Desired,
            Context = request.Context,
            Requirements = new()
        };

        return resp;
    }

    /// <summary>
    /// Get Observed Composite Resource from the supplied request.
    /// </summary>
    /// <typeparam name="T">The type of the Kubernetes object.</typeparam>
    /// <param name="request">The RunFunctionRequest.</param>
    /// <returns>The Kubernetes object of the specified type.</returns>
    public static T? GetObservedCompositeResource<T>(this RunFunctionRequest request)
    {
        return request.Observed.Composite.GetKubeResource<T>();
    }

    /// <summary>
    /// Get Observed Resources from the supplied request.
    /// </summary>
    /// <param name="request">The RunFunctionRequest.</param>
    /// <returns>A dictionary mapping resource names to Observed objects.</returns>
    /// <exception cref="Exception">Throws if conversion using Resource.AsObject fails.</exception>
    public static IDictionary<string, Resource> GetObservedResources(this RunFunctionRequest request)
    {
        return request.Observed.Resources.ToDictionary();
    }

    /// <summary>
    /// Get Desired Resources from the supplied request.
    /// </summary>
    /// <param name="request">The RunFunctionRequest.</param>
    /// <returns>A dictionary mapping resource names to Desired objects.</returns>
    /// <exception cref="Exception">Throws if conversion using Resource.AsObject fails.</exception>
    public static IDictionary<string, Resource> GetDesiredResources(this RunFunctionRequest request)
    {
        return request.Desired.Resources.ToDictionary();
    }

    /// <summary>
    /// Get Desired Resource from the supplied request.
    /// </summary>
    /// <typeparam name="T">The type of the Kubernetes object.</typeparam>
    /// <param name="request">The RunFunctionRequest.</param>
    /// <param name="key">The key of the resource.</param>
    /// <returns>The Kubernetes object of the specified type, or null if not found.</returns>
    public static T? GetDesiredResource<T>(this RunFunctionRequest request, string key)
    {
        if (request.Desired.Resources.TryGetValue(key, out var resource))
        {
            return resource.GetKubeResource<T>();
        }

        return default;
    }

    /// <summary>
    /// Get Observed Resource from the supplied request.
    /// </summary>
    /// <typeparam name="T">The type of the Kubernetes object.</typeparam>
    /// <param name="request">The RunFunctionRequest.</param>
    /// <param name="key">The key of the resource.</param>
    /// <returns>The Kubernetes object of the specified type, or null if not found.</returns>
    public static T? GetObservedResource<T>(this RunFunctionRequest request, string key)
        where T : IKubernetesObject<V1ObjectMeta>
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var resourceKey = KubernetesResourceIdentity.CreateKey<T>(key);
        if (request.Observed.Resources.TryGetValue(resourceKey, out var resource))
        {
            return resource.GetKubeResource<T>();
        }

        return default;
    }

    /// <summary>
    /// Gets all observed resources of the requested Kubernetes type.
    /// </summary>
    /// <typeparam name="T">The Kubernetes resource type.</typeparam>
    /// <param name="request">The request containing observed resources.</param>
    /// <returns>Observed resources whose API version and kind match the requested type.</returns>
    public static IEnumerable<T> GetObservedResources<T>(this RunFunctionRequest request)
        where T : IKubernetesObject<V1ObjectMeta>
    {
        ArgumentNullException.ThrowIfNull(request);

        var identity = KubernetesResourceIdentity.Get<T>();
        foreach (var resource in request.Observed.Resources.Values)
        {
            if (!resource.Resource_.Fields.TryGetValue("apiVersion", out var apiVersion)
                || !resource.Resource_.Fields.TryGetValue("kind", out var kind)
                || !string.Equals(apiVersion.StringValue, identity.ApiVersion, StringComparison.Ordinal)
                || !string.Equals(kind.StringValue, identity.Kind, StringComparison.Ordinal))
            {
                continue;
            }

            yield return resource.GetKubeResource<T>();
        }
    }

    /// <summary>
    /// Get a Required Resource from the supplied request.
    /// </summary>
    /// <param name="request">The RunFunctionRequest.</param>
    /// <param name="key">The Resource Key</param>
    /// <returns>A Required resource</returns>
    public static List<T>? GetRequiredResource<T>(this RunFunctionRequest request, string key)
    {
        if (request.RequiredResources.TryGetValue(key, out var resource))
        {
            var list = new List<T>();

            foreach (var item in resource.Items)
            {
                var obj = item.GetKubeResource<T>();
                list.Add(obj);
            }

            return list;
        }

        return default;
    }
}
