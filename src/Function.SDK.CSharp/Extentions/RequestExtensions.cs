using Apiextensions.Fn.Proto.V1;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using k8s;

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
        };

        return resp;
    }

    /// <summary>
    /// Get Observed Composite Resource from the supplied request.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="request"></param>
    /// <returns></returns>
    public static T GetObservedCompositeResource<T>(this RunFunctionRequest request)
    {
        string json = JsonFormatter.Default.Format(request.Observed.Composite.Resource_);

        return KubernetesJson.Deserialize<T>(json);
    }

    /// <summary>
    /// Get Observed Composed Resources from the supplied request.
    /// </summary>
    /// <param name="request">The RunFunctionRequest.</param>
    /// <returns>A dictionary mapping resource names to ObservedComposed objects.</returns>
    /// <exception cref="Exception">Throws if conversion using Resource.AsObject fails.</exception>
    public static IDictionary<string, Resource> GetObservedComposedResources(this RunFunctionRequest request)
    {
        return request.Observed.Resources.ToDictionary();
    }

    /// <summary>
    /// Get Desired Composed Resources from the supplied request.
    /// </summary>
    /// <param name="request">The RunFunctionRequest.</param>
    /// <returns>A dictionary mapping resource names to ObservedComposed objects.</returns>
    /// <exception cref="Exception">Throws if conversion using Resource.AsObject fails.</exception>
    public static IDictionary<string, Resource> GetDesiredComposedResources(this RunFunctionRequest request)
    {
        return request.Desired.Resources.ToDictionary();
    }

    /// <summary>
    /// Get Desired Resource from the supplied request.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="request"></param>
    /// <returns></returns>
    public static T? GetDesiredResource<T>(this RunFunctionRequest request, string key)
    {
        if (request.Desired.Resources.TryGetValue(key, out var resource))
        {
            string json = JsonFormatter.Default.Format(resource.Resource_);

            return KubernetesJson.Deserialize<T>(json);
        }

        return default(T);
    }

    /// <summary>
    /// Get Observed Resource from the supplied request.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="request"></param>
    /// <returns></returns>
    public static T? GetObservedResource<T>(this RunFunctionRequest request, string key)
    {
        if (request.Observed.Resources.TryGetValue(key, out var resource))
        {
            string json = JsonFormatter.Default.Format(resource.Resource_);

            return KubernetesJson.Deserialize<T>(json);
        }

        return default;
    }
}
