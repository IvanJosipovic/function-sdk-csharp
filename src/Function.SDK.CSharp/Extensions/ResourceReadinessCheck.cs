using Apiextensions.Fn.Proto.V1;
using k8s;

namespace Function.SDK.CSharp;

/// <summary>
/// A typed custom readiness check for a Kubernetes resource.
/// </summary>
public sealed class ResourceReadinessCheck
{
    private readonly Func<Resource, bool> evaluate;

    private ResourceReadinessCheck(string apiVersion, string kind, Func<Resource, bool> evaluate)
    {
        ApiVersion = apiVersion;
        Kind = kind;
        this.evaluate = evaluate;
    }

    internal string ApiVersion { get; }

    internal string Kind { get; }

    /// <summary>
    /// Creates a custom readiness check for Kubernetes resources of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The Kubernetes resource type to validate.</typeparam>
    /// <param name="predicates">Predicates that must all return true for the resource to be ready.</param>
    /// <returns>A typed resource readiness check.</returns>
    public static ResourceReadinessCheck For<T>(params Func<T, bool>[] predicates)
        where T : IKubernetesObject
    {
        ArgumentNullException.ThrowIfNull(predicates);
        if (predicates.Length == 0)
        {
            throw new ArgumentException("At least one custom readiness predicate is required.", nameof(predicates));
        }

        if (predicates.Any(static predicate => predicate == null))
        {
            throw new ArgumentException("Custom readiness predicates cannot contain null.", nameof(predicates));
        }

        var identity = KubernetesResourceIdentity.Get<T>();
        return new(identity.ApiVersion, identity.Kind, Evaluate);

        bool Evaluate(Resource resource)
        {
            var observedResource = resource.GetKubeResource<T>();
            return predicates.All(predicate => predicate(observedResource));
        }
    }

    internal bool Evaluate(Resource resource) => evaluate(resource);
}
