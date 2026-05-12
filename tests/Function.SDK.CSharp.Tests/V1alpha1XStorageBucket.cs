#nullable enable
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using k8s;
using k8s.Models;

namespace Function.SDK.CSharp.SourceGenerator.Models.platform.example.com;
/// <summary>StorageBucket is the Schema for the StorageBucket API.</summary>
[global::System.CodeDom.Compiler.GeneratedCode("KubernetesCRDModelGen", "1.5.1+3712eeae712149ba9ec495b842c026dd9f33b093")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
[KubernetesEntity(Group = KubeGroup, Kind = KubeKind, ApiVersion = KubeApiVersion, PluralName = KubePluralName)]
public partial class V1alpha1XStorageBucketList : IKubernetesObject<V1ListMeta>, IItems<V1alpha1XStorageBucket>
{
    public const string KubeApiVersion = "v1alpha1";
    public const string KubeKind = "XStorageBucketList";
    public const string KubeGroup = "platform.example.com";
    public const string KubePluralName = "xstoragebuckets";
    /// <summary>APIVersion defines the versioned schema of this representation of an object. Servers should convert recognized schemas to the latest internal value, and may reject unrecognized values. More info: https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#resources</summary>
    [JsonPropertyName("apiVersion")]
    public string ApiVersion { get; set; } = "platform.example.com/v1alpha1";

    /// <summary>Kind is a string value representing the REST resource this object represents. Servers may infer this from the endpoint the client submits requests to. Cannot be updated. In CamelCase. More info: https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#types-kinds</summary>
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "XStorageBucketList";

    /// <summary>ListMeta describes metadata that synthetic resources must have, including lists and various status objects. A resource may have only one of {ObjectMeta, ListMeta}.</summary>
    [JsonPropertyName("metadata")]
    public V1ListMeta? Metadata { get; set; }

    /// <summary>List of V1alpha1XStorageBucket objects.</summary>
    [JsonPropertyName("items")]
    public IList<V1alpha1XStorageBucket>? Items { get; set; }
}

/// <summary>Access control list for the storage bucket. Private, Blob (anonymous access for blobs), Container (anonymous access for containers and blobs)</summary>
[global::System.CodeDom.Compiler.GeneratedCode("KubernetesCRDModelGen", "1.5.1+3712eeae712149ba9ec495b842c026dd9f33b093")]
[JsonConverter(typeof(JsonStringEnumConverter<V1alpha1XStorageBucketSpecParametersAclEnum>))]
public enum V1alpha1XStorageBucketSpecParametersAclEnum
{
    [EnumMember(Value = "private"), JsonStringEnumMemberName("private")]
    Private,
    [EnumMember(Value = "blob"), JsonStringEnumMemberName("blob")]
    Blob,
    [EnumMember(Value = "container"), JsonStringEnumMemberName("container")]
    Container
}

/// <summary>Geographic location where the storage bucket will be created</summary>
[global::System.CodeDom.Compiler.GeneratedCode("KubernetesCRDModelGen", "1.5.1+3712eeae712149ba9ec495b842c026dd9f33b093")]
[JsonConverter(typeof(JsonStringEnumConverter<V1alpha1XStorageBucketSpecParametersLocationEnum>))]
public enum V1alpha1XStorageBucketSpecParametersLocationEnum
{
    [EnumMember(Value = "asia"), JsonStringEnumMemberName("asia")]
    Asia,
    [EnumMember(Value = "asiapacific"), JsonStringEnumMemberName("asiapacific")]
    Asiapacific,
    [EnumMember(Value = "australia"), JsonStringEnumMemberName("australia")]
    Australia,
    [EnumMember(Value = "australiacentral"), JsonStringEnumMemberName("australiacentral")]
    Australiacentral,
    [EnumMember(Value = "australiacentral2"), JsonStringEnumMemberName("australiacentral2")]
    Australiacentral2,
    [EnumMember(Value = "australiaeast"), JsonStringEnumMemberName("australiaeast")]
    Australiaeast,
    [EnumMember(Value = "australiasoutheast"), JsonStringEnumMemberName("australiasoutheast")]
    Australiasoutheast,
    [EnumMember(Value = "austriaeast"), JsonStringEnumMemberName("austriaeast")]
    Austriaeast,
    [EnumMember(Value = "brazil"), JsonStringEnumMemberName("brazil")]
    Brazil,
    [EnumMember(Value = "brazilsouth"), JsonStringEnumMemberName("brazilsouth")]
    Brazilsouth,
    [EnumMember(Value = "brazilsoutheast"), JsonStringEnumMemberName("brazilsoutheast")]
    Brazilsoutheast,
    [EnumMember(Value = "brazilus"), JsonStringEnumMemberName("brazilus")]
    Brazilus,
    [EnumMember(Value = "canada"), JsonStringEnumMemberName("canada")]
    Canada,
    [EnumMember(Value = "canadacentral"), JsonStringEnumMemberName("canadacentral")]
    Canadacentral,
    [EnumMember(Value = "canadaeast"), JsonStringEnumMemberName("canadaeast")]
    Canadaeast,
    [EnumMember(Value = "centralindia"), JsonStringEnumMemberName("centralindia")]
    Centralindia,
    [EnumMember(Value = "centralus"), JsonStringEnumMemberName("centralus")]
    Centralus,
    [EnumMember(Value = "centraluseuap"), JsonStringEnumMemberName("centraluseuap")]
    Centraluseuap,
    [EnumMember(Value = "centralusstage"), JsonStringEnumMemberName("centralusstage")]
    Centralusstage,
    [EnumMember(Value = "chilecentral"), JsonStringEnumMemberName("chilecentral")]
    Chilecentral,
    [EnumMember(Value = "eastasia"), JsonStringEnumMemberName("eastasia")]
    Eastasia,
    [EnumMember(Value = "eastasiastage"), JsonStringEnumMemberName("eastasiastage")]
    Eastasiastage,
    [EnumMember(Value = "eastus"), JsonStringEnumMemberName("eastus")]
    Eastus,
    [EnumMember(Value = "eastus2"), JsonStringEnumMemberName("eastus2")]
    Eastus2,
    [EnumMember(Value = "eastus2euap"), JsonStringEnumMemberName("eastus2euap")]
    Eastus2euap,
    [EnumMember(Value = "eastus2stage"), JsonStringEnumMemberName("eastus2stage")]
    Eastus2stage,
    [EnumMember(Value = "eastusstage"), JsonStringEnumMemberName("eastusstage")]
    Eastusstage,
    [EnumMember(Value = "eastusstg"), JsonStringEnumMemberName("eastusstg")]
    Eastusstg,
    [EnumMember(Value = "europe"), JsonStringEnumMemberName("europe")]
    Europe,
    [EnumMember(Value = "france"), JsonStringEnumMemberName("france")]
    France,
    [EnumMember(Value = "francecentral"), JsonStringEnumMemberName("francecentral")]
    Francecentral,
    [EnumMember(Value = "francesouth"), JsonStringEnumMemberName("francesouth")]
    Francesouth,
    [EnumMember(Value = "germany"), JsonStringEnumMemberName("germany")]
    Germany,
    [EnumMember(Value = "germanynorth"), JsonStringEnumMemberName("germanynorth")]
    Germanynorth,
    [EnumMember(Value = "germanywestcentral"), JsonStringEnumMemberName("germanywestcentral")]
    Germanywestcentral,
    [EnumMember(Value = "global"), JsonStringEnumMemberName("global")]
    Global,
    [EnumMember(Value = "india"), JsonStringEnumMemberName("india")]
    India,
    [EnumMember(Value = "indonesia"), JsonStringEnumMemberName("indonesia")]
    Indonesia,
    [EnumMember(Value = "indonesiacentral"), JsonStringEnumMemberName("indonesiacentral")]
    Indonesiacentral,
    [EnumMember(Value = "israel"), JsonStringEnumMemberName("israel")]
    Israel,
    [EnumMember(Value = "israelcentral"), JsonStringEnumMemberName("israelcentral")]
    Israelcentral,
    [EnumMember(Value = "italy"), JsonStringEnumMemberName("italy")]
    Italy,
    [EnumMember(Value = "italynorth"), JsonStringEnumMemberName("italynorth")]
    Italynorth,
    [EnumMember(Value = "japan"), JsonStringEnumMemberName("japan")]
    Japan,
    [EnumMember(Value = "japaneast"), JsonStringEnumMemberName("japaneast")]
    Japaneast,
    [EnumMember(Value = "japanwest"), JsonStringEnumMemberName("japanwest")]
    Japanwest,
    [EnumMember(Value = "jioindiacentral"), JsonStringEnumMemberName("jioindiacentral")]
    Jioindiacentral,
    [EnumMember(Value = "jioindiawest"), JsonStringEnumMemberName("jioindiawest")]
    Jioindiawest,
    [EnumMember(Value = "korea"), JsonStringEnumMemberName("korea")]
    Korea,
    [EnumMember(Value = "koreacentral"), JsonStringEnumMemberName("koreacentral")]
    Koreacentral,
    [EnumMember(Value = "koreasouth"), JsonStringEnumMemberName("koreasouth")]
    Koreasouth,
    [EnumMember(Value = "malaysia"), JsonStringEnumMemberName("malaysia")]
    Malaysia,
    [EnumMember(Value = "malaysiawest"), JsonStringEnumMemberName("malaysiawest")]
    Malaysiawest,
    [EnumMember(Value = "mexico"), JsonStringEnumMemberName("mexico")]
    Mexico,
    [EnumMember(Value = "mexicocentral"), JsonStringEnumMemberName("mexicocentral")]
    Mexicocentral,
    [EnumMember(Value = "newzealand"), JsonStringEnumMemberName("newzealand")]
    Newzealand,
    [EnumMember(Value = "newzealandnorth"), JsonStringEnumMemberName("newzealandnorth")]
    Newzealandnorth,
    [EnumMember(Value = "northcentralus"), JsonStringEnumMemberName("northcentralus")]
    Northcentralus,
    [EnumMember(Value = "northcentralusstage"), JsonStringEnumMemberName("northcentralusstage")]
    Northcentralusstage,
    [EnumMember(Value = "northeurope"), JsonStringEnumMemberName("northeurope")]
    Northeurope,
    [EnumMember(Value = "norway"), JsonStringEnumMemberName("norway")]
    Norway,
    [EnumMember(Value = "norwayeast"), JsonStringEnumMemberName("norwayeast")]
    Norwayeast,
    [EnumMember(Value = "norwaywest"), JsonStringEnumMemberName("norwaywest")]
    Norwaywest,
    [EnumMember(Value = "poland"), JsonStringEnumMemberName("poland")]
    Poland,
    [EnumMember(Value = "polandcentral"), JsonStringEnumMemberName("polandcentral")]
    Polandcentral,
    [EnumMember(Value = "qatar"), JsonStringEnumMemberName("qatar")]
    Qatar,
    [EnumMember(Value = "qatarcentral"), JsonStringEnumMemberName("qatarcentral")]
    Qatarcentral,
    [EnumMember(Value = "singapore"), JsonStringEnumMemberName("singapore")]
    Singapore,
    [EnumMember(Value = "southafrica"), JsonStringEnumMemberName("southafrica")]
    Southafrica,
    [EnumMember(Value = "southafricanorth"), JsonStringEnumMemberName("southafricanorth")]
    Southafricanorth,
    [EnumMember(Value = "southafricawest"), JsonStringEnumMemberName("southafricawest")]
    Southafricawest,
    [EnumMember(Value = "southcentralus"), JsonStringEnumMemberName("southcentralus")]
    Southcentralus,
    [EnumMember(Value = "southcentralusstage"), JsonStringEnumMemberName("southcentralusstage")]
    Southcentralusstage,
    [EnumMember(Value = "southcentralusstg"), JsonStringEnumMemberName("southcentralusstg")]
    Southcentralusstg,
    [EnumMember(Value = "southeastasia"), JsonStringEnumMemberName("southeastasia")]
    Southeastasia,
    [EnumMember(Value = "southeastasiastage"), JsonStringEnumMemberName("southeastasiastage")]
    Southeastasiastage,
    [EnumMember(Value = "southindia"), JsonStringEnumMemberName("southindia")]
    Southindia,
    [EnumMember(Value = "spain"), JsonStringEnumMemberName("spain")]
    Spain,
    [EnumMember(Value = "spaincentral"), JsonStringEnumMemberName("spaincentral")]
    Spaincentral,
    [EnumMember(Value = "sweden"), JsonStringEnumMemberName("sweden")]
    Sweden,
    [EnumMember(Value = "swedencentral"), JsonStringEnumMemberName("swedencentral")]
    Swedencentral,
    [EnumMember(Value = "switzerland"), JsonStringEnumMemberName("switzerland")]
    Switzerland,
    [EnumMember(Value = "switzerlandnorth"), JsonStringEnumMemberName("switzerlandnorth")]
    Switzerlandnorth,
    [EnumMember(Value = "switzerlandwest"), JsonStringEnumMemberName("switzerlandwest")]
    Switzerlandwest,
    [EnumMember(Value = "taiwan"), JsonStringEnumMemberName("taiwan")]
    Taiwan,
    [EnumMember(Value = "uae"), JsonStringEnumMemberName("uae")]
    Uae,
    [EnumMember(Value = "uaecentral"), JsonStringEnumMemberName("uaecentral")]
    Uaecentral,
    [EnumMember(Value = "uaenorth"), JsonStringEnumMemberName("uaenorth")]
    Uaenorth,
    [EnumMember(Value = "uk"), JsonStringEnumMemberName("uk")]
    Uk,
    [EnumMember(Value = "uksouth"), JsonStringEnumMemberName("uksouth")]
    Uksouth,
    [EnumMember(Value = "ukwest"), JsonStringEnumMemberName("ukwest")]
    Ukwest,
    [EnumMember(Value = "unitedstates"), JsonStringEnumMemberName("unitedstates")]
    Unitedstates,
    [EnumMember(Value = "unitedstateseuap"), JsonStringEnumMemberName("unitedstateseuap")]
    Unitedstateseuap,
    [EnumMember(Value = "westcentralus"), JsonStringEnumMemberName("westcentralus")]
    Westcentralus,
    [EnumMember(Value = "westeurope"), JsonStringEnumMemberName("westeurope")]
    Westeurope,
    [EnumMember(Value = "westindia"), JsonStringEnumMemberName("westindia")]
    Westindia,
    [EnumMember(Value = "westus"), JsonStringEnumMemberName("westus")]
    Westus,
    [EnumMember(Value = "westus2"), JsonStringEnumMemberName("westus2")]
    Westus2,
    [EnumMember(Value = "westus2stage"), JsonStringEnumMemberName("westus2stage")]
    Westus2stage,
    [EnumMember(Value = "westus3"), JsonStringEnumMemberName("westus3")]
    Westus3,
    [EnumMember(Value = "westusstage"), JsonStringEnumMemberName("westusstage")]
    Westusstage
}

[global::System.CodeDom.Compiler.GeneratedCode("KubernetesCRDModelGen", "1.5.1+3712eeae712149ba9ec495b842c026dd9f33b093")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class V1alpha1XStorageBucketSpecParameters
{
    /// <summary>Access control list for the storage bucket. Private, Blob (anonymous access for blobs), Container (anonymous access for containers and blobs)</summary>
    [JsonPropertyName("acl")]
    public required V1alpha1XStorageBucketSpecParametersAclEnum Acl { get; set; }

    /// <summary>Geographic location where the storage bucket will be created</summary>
    [JsonPropertyName("location")]
    public required V1alpha1XStorageBucketSpecParametersLocationEnum Location { get; set; }

    /// <summary>Enable versioning to maintain multiple versions of objects in the bucket</summary>
    [JsonPropertyName("versioning")]
    public required bool Versioning { get; set; }

    /// <summary>Enable public access to the Account</summary>
    [JsonPropertyName("public")]
    public bool? Public { get; set; }
}

[global::System.CodeDom.Compiler.GeneratedCode("KubernetesCRDModelGen", "1.5.1+3712eeae712149ba9ec495b842c026dd9f33b093")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class V1alpha1XStorageBucketSpecCrossplaneCompositionRef
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
}

[global::System.CodeDom.Compiler.GeneratedCode("KubernetesCRDModelGen", "1.5.1+3712eeae712149ba9ec495b842c026dd9f33b093")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class V1alpha1XStorageBucketSpecCrossplaneCompositionRevisionRef
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
}

[global::System.CodeDom.Compiler.GeneratedCode("KubernetesCRDModelGen", "1.5.1+3712eeae712149ba9ec495b842c026dd9f33b093")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class V1alpha1XStorageBucketSpecCrossplaneCompositionRevisionSelector
{
    [JsonPropertyName("matchLabels")]
    public required IDictionary<string, string> MatchLabels { get; set; }
}

[global::System.CodeDom.Compiler.GeneratedCode("KubernetesCRDModelGen", "1.5.1+3712eeae712149ba9ec495b842c026dd9f33b093")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class V1alpha1XStorageBucketSpecCrossplaneCompositionSelector
{
    [JsonPropertyName("matchLabels")]
    public required IDictionary<string, string> MatchLabels { get; set; }
}

[global::System.CodeDom.Compiler.GeneratedCode("KubernetesCRDModelGen", "1.5.1+3712eeae712149ba9ec495b842c026dd9f33b093")]
[JsonConverter(typeof(JsonStringEnumConverter<V1alpha1XStorageBucketSpecCrossplaneCompositionUpdatePolicyEnum>))]
public enum V1alpha1XStorageBucketSpecCrossplaneCompositionUpdatePolicyEnum
{
    [EnumMember(Value = "Automatic"), JsonStringEnumMemberName("Automatic")]
    Automatic,
    [EnumMember(Value = "Manual"), JsonStringEnumMemberName("Manual")]
    Manual
}

[global::System.CodeDom.Compiler.GeneratedCode("KubernetesCRDModelGen", "1.5.1+3712eeae712149ba9ec495b842c026dd9f33b093")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class V1alpha1XStorageBucketSpecCrossplaneResourceRefs
{
    [JsonPropertyName("apiVersion")]
    public required string ApiVersion { get; set; }

    [JsonPropertyName("kind")]
    public required string Kind { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

/// <summary>Configures how Crossplane will reconcile this composite resource</summary>
[global::System.CodeDom.Compiler.GeneratedCode("KubernetesCRDModelGen", "1.5.1+3712eeae712149ba9ec495b842c026dd9f33b093")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class V1alpha1XStorageBucketSpecCrossplane
{
    [JsonPropertyName("compositionRef")]
    public V1alpha1XStorageBucketSpecCrossplaneCompositionRef? CompositionRef { get; set; }

    [JsonPropertyName("compositionRevisionRef")]
    public V1alpha1XStorageBucketSpecCrossplaneCompositionRevisionRef? CompositionRevisionRef { get; set; }

    [JsonPropertyName("compositionRevisionSelector")]
    public V1alpha1XStorageBucketSpecCrossplaneCompositionRevisionSelector? CompositionRevisionSelector { get; set; }

    [JsonPropertyName("compositionSelector")]
    public V1alpha1XStorageBucketSpecCrossplaneCompositionSelector? CompositionSelector { get; set; }

    [JsonPropertyName("compositionUpdatePolicy")]
    public V1alpha1XStorageBucketSpecCrossplaneCompositionUpdatePolicyEnum? CompositionUpdatePolicy { get; set; }

    [JsonPropertyName("resourceRefs")]
    public IList<V1alpha1XStorageBucketSpecCrossplaneResourceRefs>? ResourceRefs { get; set; }
}

/// <summary>StorageBucketSpec defines the desired state of StorageBucket.</summary>
[global::System.CodeDom.Compiler.GeneratedCode("KubernetesCRDModelGen", "1.5.1+3712eeae712149ba9ec495b842c026dd9f33b093")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class V1alpha1XStorageBucketSpec
{
    [JsonPropertyName("parameters")]
    public required V1alpha1XStorageBucketSpecParameters Parameters { get; set; }

    /// <summary>Configures how Crossplane will reconcile this composite resource</summary>
    [JsonPropertyName("crossplane")]
    public V1alpha1XStorageBucketSpecCrossplane? Crossplane { get; set; }
}

[global::System.CodeDom.Compiler.GeneratedCode("KubernetesCRDModelGen", "1.5.1+3712eeae712149ba9ec495b842c026dd9f33b093")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class V1alpha1XStorageBucketStatusConditions
{
    [JsonPropertyName("lastTransitionTime")]
    public required DateTime LastTransitionTime { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("observedGeneration")]
    public long? ObservedGeneration { get; set; }

    [JsonPropertyName("reason")]
    public required string Reason { get; set; }

    [JsonPropertyName("status")]
    public required string Status { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; }
}

/// <summary>Status defines the observed state of the CompositeResourceDefinition.</summary>
[global::System.CodeDom.Compiler.GeneratedCode("KubernetesCRDModelGen", "1.5.1+3712eeae712149ba9ec495b842c026dd9f33b093")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class V1alpha1XStorageBucketStatus
{
    /// <summary>Conditions of the resource.</summary>
    [JsonPropertyName("conditions")]
    public IList<V1alpha1XStorageBucketStatusConditions>? Conditions { get; set; }
}

/// <summary>StorageBucket is the Schema for the StorageBucket API.</summary>
[global::System.CodeDom.Compiler.GeneratedCode("KubernetesCRDModelGen", "1.5.1+3712eeae712149ba9ec495b842c026dd9f33b093")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
[KubernetesEntity(Group = KubeGroup, Kind = KubeKind, ApiVersion = KubeApiVersion, PluralName = KubePluralName)]
public partial class V1alpha1XStorageBucket : IKubernetesObject<V1ObjectMeta>, ISpec<V1alpha1XStorageBucketSpec>, IStatus<V1alpha1XStorageBucketStatus?>
{
    public const string KubeApiVersion = "v1alpha1";
    public const string KubeKind = "XStorageBucket";
    public const string KubeGroup = "platform.example.com";
    public const string KubePluralName = "xstoragebuckets";
    /// <summary>APIVersion defines the versioned schema of this representation of an object. Servers should convert recognized schemas to the latest internal value, and may reject unrecognized values. More info: https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#resources</summary>
    [JsonPropertyName("apiVersion")]
    public string ApiVersion { get; set; } = "platform.example.com/v1alpha1";

    /// <summary>Kind is a string value representing the REST resource this object represents. Servers may infer this from the endpoint the client submits requests to. Cannot be updated. In CamelCase. More info: https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#types-kinds</summary>
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "XStorageBucket";

    /// <summary>Standard object&apos;s metadata. More info: https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#metadata</summary>
    [JsonPropertyName("metadata")]
    public V1ObjectMeta Metadata { get; set; }

    /// <summary>StorageBucketSpec defines the desired state of StorageBucket.</summary>
    [JsonPropertyName("spec")]
    public required V1alpha1XStorageBucketSpec Spec { get; set; }

    /// <summary>Status defines the observed state of the CompositeResourceDefinition.</summary>
    [JsonPropertyName("status")]
    public V1alpha1XStorageBucketStatus? Status { get; set; }
}
