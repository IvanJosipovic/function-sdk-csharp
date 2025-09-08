using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Function.SDK.CSharp.SourceGenerator;

#nullable enable
/// <summary>Metadata specifies the desired metadata for the defined composite resource and claim CRD's.</summary>
public partial class V2CompositeResourceDefinitionSpecMetadata
{
    /// <summary>Annotations is an unstructured key value map stored with a resource that may be set by external tools to store and retrieve arbitrary metadata. They are not queryable and should be preserved when modifying objects. More info: https://kubernetes.io/docs/concepts/overview/working-with-objects/annotations</summary>
    [JsonPropertyName("annotations")]
    public IDictionary<string, string>? Annotations { get; set; }

    /// <summary>Map of string keys and values that can be used to organize and categorize (scope and select) objects. May match selectors of replication controllers More info: https://kubernetes.io/docs/concepts/overview/working-with-objects/labels and services. These labels are added to the composite resource and claim CRD's in addition to any labels defined by `CompositionResourceDefinition` `metadata.labels`.</summary>
    [JsonPropertyName("labels")]
    public IDictionary<string, string>? Labels { get; set; }
}
#nullable disable

#nullable enable
/// <summary>Names specifies the resource and kind names of the defined composite resource.</summary>
public partial class V2CompositeResourceDefinitionSpecNames
{
    /// <summary>categories is a list of grouped resources this custom resource belongs to (e.g. 'all'). This is published in API discovery documents, and used by clients to support invocations like `kubectl get all`.</summary>
    [JsonPropertyName("categories")]
    public IList<string>? Categories { get; set; }

    /// <summary>kind is the serialized kind of the resource. It is normally CamelCase and singular. Custom resource instances will use this value as the `kind` attribute in API calls.</summary>
    [JsonPropertyName("kind")]
    public string Kind { get; set; }

    /// <summary>listKind is the serialized kind of the list for this resource. Defaults to "`kind`List".</summary>
    [JsonPropertyName("listKind")]
    public string? ListKind { get; set; }

    /// <summary>plural is the plural name of the resource to serve. The custom resources are served under `/apis/&lt;group&gt;/&lt;version&gt;/.../&lt;plural&gt;`. Must match the name of the CustomResourceDefinition (in the form `&lt;names.plural&gt;.&lt;group&gt;`). Must be all lowercase.</summary>
    [JsonPropertyName("plural")]
    public string Plural { get; set; }

    /// <summary>shortNames are short names for the resource, exposed in API discovery documents, and used by clients to support invocations like `kubectl get &lt;shortname&gt;`. It must be all lowercase.</summary>
    [JsonPropertyName("shortNames")]
    public IList<string>? ShortNames { get; set; }

    /// <summary>singular is the singular name of the resource. It must be all lowercase. Defaults to lowercased `kind`.</summary>
    [JsonPropertyName("singular")]
    public string? Singular { get; set; }
}
#nullable disable

#nullable enable
/// <summary>Schema describes the schema used for validation, pruning, and defaulting of this version of the defined composite resource. Fields required by all composite resources will be injected into this schema automatically, and will override equivalently named fields in this schema. Omitting this schema results in a schema that contains only the fields required by all composite resources.</summary>
public partial class V2CompositeResourceDefinitionSpecVersionsSchema
{
    /// <summary>OpenAPIV3Schema is the OpenAPI v3 schema to use for validation and pruning.</summary>
    [JsonPropertyName("openAPIV3Schema")]
    public JsonNode? OpenAPIV3Schema { get; set; }
}
#nullable disable

#nullable enable
/// <summary>CompositeResourceDefinitionVersion describes a version of an XR.</summary>
public partial class V2CompositeResourceDefinitionSpecVersions
{
    /// <summary>The deprecated field specifies that this version is deprecated and should not be used.</summary>
    [JsonPropertyName("deprecated")]
    public bool? Deprecated { get; set; }

    /// <summary>DeprecationWarning specifies the message that should be shown to the user when using this version.</summary>
    [JsonPropertyName("deprecationWarning")]
    public string? DeprecationWarning { get; set; }

    /// <summary>Name of this version, e.g. “v1”, “v2beta1”, etc. Composite resources are served under this version at `/apis/&lt;group&gt;/&lt;version&gt;/...` if `served` is true.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>Referenceable specifies that this version may be referenced by a Composition in order to configure which resources an XR may be composed of. Exactly one version must be marked as referenceable; all Compositions must target only the referenceable version. The referenceable version must be served. It's mapped to the CRD's `spec.versions[*].storage` field.</summary>
    [JsonPropertyName("referenceable")]
    public bool Referenceable { get; set; }

    /// <summary>Schema describes the schema used for validation, pruning, and defaulting of this version of the defined composite resource. Fields required by all composite resources will be injected into this schema automatically, and will override equivalently named fields in this schema. Omitting this schema results in a schema that contains only the fields required by all composite resources.</summary>
    [JsonPropertyName("schema")]
    public V2CompositeResourceDefinitionSpecVersionsSchema? Schema { get; set; }

    /// <summary>Served specifies that this version should be served via REST APIs.</summary>
    [JsonPropertyName("served")]
    public bool Served { get; set; }
}
#nullable disable

#nullable enable
/// <summary>CompositeResourceDefinitionSpec specifies the desired state of the definition.</summary>
public partial class V2CompositeResourceDefinitionSpec
{
    /// <summary>Group specifies the API group of the defined composite resource. Composite resources are served under `/apis/&lt;group&gt;/...`. Must match the name of the XRD (in the form `&lt;names.plural&gt;.&lt;group&gt;`).</summary>
    [JsonPropertyName("group")]
    public string Group { get; set; }

    /// <summary>Metadata specifies the desired metadata for the defined composite resource and claim CRD's.</summary>
    [JsonPropertyName("metadata")]
    public V2CompositeResourceDefinitionSpecMetadata? Metadata { get; set; }

    /// <summary>Names specifies the resource and kind names of the defined composite resource.</summary>
    [JsonPropertyName("names")]
    public V2CompositeResourceDefinitionSpecNames Names { get; set; }

    /// <summary>Versions is the list of all API versions of the defined composite resource. Version names are used to compute the order in which served versions are listed in API discovery. If the version string is "kube-like", it will sort above non "kube-like" version strings, which are ordered lexicographically. "Kube-like" versions start with a "v", then are followed by a number (the major version), then optionally the string "alpha" or "beta" and another number (the minor version). These are sorted first by GA &gt; beta &gt; alpha (where GA is a version with no suffix such as beta or alpha), and then by comparing major version, then minor version. An example sorted list of versions: v10, v2, v1, v11beta2, v10beta3, v3beta1, v12alpha1, v11alpha2, foo1, foo10.</summary>
    [JsonPropertyName("versions")]
    public IList<V2CompositeResourceDefinitionSpecVersions> Versions { get; set; }
}
#nullable disable

#nullable enable
public class V1ObjectMeta
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}
#nullable disable

#nullable enable
/// <summary>A CompositeResourceDefinition defines the schema for a new custom Kubernetes API.  Read the Crossplane documentation for [more information about CustomResourceDefinitions](https://docs.crossplane.io/latest/concepts/composite-resource-definitions).</summary>
public partial class V2CompositeResourceDefinition
{
    public const string KubeApiVersion = "v2";
    public const string KubeKind = "CompositeResourceDefinition";
    public const string KubeGroup = "apiextensions.crossplane.io";
    public const string KubePluralName = "compositeresourcedefinitions";
    /// <summary>APIVersion defines the versioned schema of this representation of an object. Servers should convert recognized schemas to the latest internal value, and may reject unrecognized values. More info: https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#resources</summary>
    [JsonPropertyName("apiVersion")]
    public string ApiVersion { get; set; } = "apiextensions.crossplane.io/v2";

    /// <summary>Kind is a string value representing the REST resource this object represents. Servers may infer this from the endpoint the client submits requests to. Cannot be updated. In CamelCase. More info: https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#types-kinds</summary>
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "CompositeResourceDefinition";

    /// <summary>Standard object's metadata. More info: https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#metadata</summary>
    [JsonPropertyName("metadata")]
    public V1ObjectMeta Metadata { get; set; }

    /// <summary>CompositeResourceDefinitionSpec specifies the desired state of the definition.</summary>
    [JsonPropertyName("spec")]
    public V2CompositeResourceDefinitionSpec? Spec { get; set; }
}
#nullable disable
