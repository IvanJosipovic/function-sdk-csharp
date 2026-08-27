using k8s;

namespace Function.SDK.CSharp;

internal static class KubernetesResourceIdentity
{
    internal static (string ApiVersion, string Kind) Get<T>()
        where T : IKubernetesObject
    {
        return Get(typeof(T));
    }

    internal static (string ApiVersion, string Kind) Get(Type resourceType)
    {
        var metadata = resourceType.GetKubernetesTypeMetadata();
        var apiVersion = string.IsNullOrEmpty(metadata.Group)
            ? metadata.ApiVersion
            : $"{metadata.Group}/{metadata.ApiVersion}";

        return (apiVersion, metadata.Kind);
    }

    internal static string CreateKey<T>(string key)
        where T : IKubernetesObject
    {
        var identity = Get<T>();

        return CreateKey(identity.ApiVersion, identity.Kind, key);
    }

    internal static string CreateKey(IKubernetesObject resource, string key)
    {
        return CreateKey(resource.ApiVersion, resource.Kind, key);
    }

    private static string CreateKey(string apiVersion, string kind, string key)
    {
        return $"{apiVersion}/{kind}/{key}";
    }
}
