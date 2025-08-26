using k8s;
using k8s.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Function.SDK.CSharp.SourceGenerator.Models.platform.example.com;
#nullable enable
/// <summary>StorageBucket is the Schema for the StorageBucket API.</summary>
[global::System.CodeDom.Compiler.GeneratedCode("KubernetesCRDModelGen.Tool", "1.0.0.0"), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
[KubernetesEntity(Group = KubeGroup, Kind = KubeKind, ApiVersion = KubeApiVersion, PluralName = KubePluralName)]
public partial class V1alpha2XStorageBucketList : IKubernetesObject<V1ListMeta>, IItems<V1alpha2XStorageBucket>
{
    public const string KubeApiVersion = "v1alpha2";
    public const string KubeKind = "XStorageBucketList";
    public const string KubeGroup = "platform.example.com";
    public const string KubePluralName = "xstoragebuckets";
    /// <summary>APIVersion defines the versioned schema of this representation of an object. Servers should convert recognized schemas to the latest internal value, and may reject unrecognized values. More info: https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#resources</summary>
    [JsonPropertyName("apiVersion")]
    public string ApiVersion { get; set; } = "platform.example.com/v1alpha2";

    /// <summary>Kind is a string value representing the REST resource this object represents. Servers may infer this from the endpoint the client submits requests to. Cannot be updated. In CamelCase. More info: https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#types-kinds</summary>
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "XStorageBucketList";

    /// <summary>ListMeta describes metadata that synthetic resources must have, including lists and various status objects. A resource may have only one of {ObjectMeta, ListMeta}.</summary>
    [JsonPropertyName("metadata")]
    public V1ListMeta Metadata { get; set; }

    /// <summary>List of V1alpha2XStorageBucket objects.</summary>
    [JsonPropertyName("items")]
    public IList<V1alpha2XStorageBucket> Items { get; set; }
}
#nullable disable

/// <summary>Access control list for the storage bucket. Private, Blob (anonymous access for blobs), Container (anonymous access for containers and blobs)</summary>
[global::System.CodeDom.Compiler.GeneratedCode("KubernetesCRDModelGen.Tool", "1.0.0.0")]
public enum V1alpha2XStorageBucketSpecParametersAclEnum
{
    [EnumMember(Value = "private"), JsonStringEnumMemberName("private")]
    /// <summary>private</summary>
    Private,
    [EnumMember(Value = "blob"), JsonStringEnumMemberName("blob")]
    /// <summary>blob</summary>
    Blob,
    [EnumMember(Value = "container"), JsonStringEnumMemberName("container")]
    /// <summary>container</summary>
    Container
}

/// <summary>Geographic location where the storage bucket will be created</summary>
[global::System.CodeDom.Compiler.GeneratedCode("KubernetesCRDModelGen.Tool", "1.0.0.0")]
public enum V1alpha2XStorageBucketSpecParametersLocationEnum
{
    [EnumMember(Value = "asia"), JsonStringEnumMemberName("asia")]
    /// <summary>asia</summary>
    Asia,
    [EnumMember(Value = "asiapacific"), JsonStringEnumMemberName("asiapacific")]
    /// <summary>asiapacific</summary>
    Asiapacific,
    [EnumMember(Value = "australia"), JsonStringEnumMemberName("australia")]
    /// <summary>australia</summary>
    Australia,
    [EnumMember(Value = "australiacentral"), JsonStringEnumMemberName("australiacentral")]
    /// <summary>australiacentral</summary>
    Australiacentral,
    [EnumMember(Value = "australiacentral2"), JsonStringEnumMemberName("australiacentral2")]
    /// <summary>australiacentral2</summary>
    Australiacentral2,
    [EnumMember(Value = "australiaeast"), JsonStringEnumMemberName("australiaeast")]
    /// <summary>australiaeast</summary>
    Australiaeast,
    [EnumMember(Value = "australiasoutheast"), JsonStringEnumMemberName("australiasoutheast")]
    /// <summary>australiasoutheast</summary>
    Australiasoutheast,
    [EnumMember(Value = "austriaeast"), JsonStringEnumMemberName("austriaeast")]
    /// <summary>austriaeast</summary>
    Austriaeast,
    [EnumMember(Value = "brazil"), JsonStringEnumMemberName("brazil")]
    /// <summary>brazil</summary>
    Brazil,
    [EnumMember(Value = "brazilsouth"), JsonStringEnumMemberName("brazilsouth")]
    /// <summary>brazilsouth</summary>
    Brazilsouth,
    [EnumMember(Value = "brazilsoutheast"), JsonStringEnumMemberName("brazilsoutheast")]
    /// <summary>brazilsoutheast</summary>
    Brazilsoutheast,
    [EnumMember(Value = "brazilus"), JsonStringEnumMemberName("brazilus")]
    /// <summary>brazilus</summary>
    Brazilus,
    [EnumMember(Value = "canada"), JsonStringEnumMemberName("canada")]
    /// <summary>canada</summary>
    Canada,
    [EnumMember(Value = "canadacentral"), JsonStringEnumMemberName("canadacentral")]
    /// <summary>canadacentral</summary>
    Canadacentral,
    [EnumMember(Value = "canadaeast"), JsonStringEnumMemberName("canadaeast")]
    /// <summary>canadaeast</summary>
    Canadaeast,
    [EnumMember(Value = "centralindia"), JsonStringEnumMemberName("centralindia")]
    /// <summary>centralindia</summary>
    Centralindia,
    [EnumMember(Value = "centralus"), JsonStringEnumMemberName("centralus")]
    /// <summary>centralus</summary>
    Centralus,
    [EnumMember(Value = "centraluseuap"), JsonStringEnumMemberName("centraluseuap")]
    /// <summary>centraluseuap</summary>
    Centraluseuap,
    [EnumMember(Value = "centralusstage"), JsonStringEnumMemberName("centralusstage")]
    /// <summary>centralusstage</summary>
    Centralusstage,
    [EnumMember(Value = "chilecentral"), JsonStringEnumMemberName("chilecentral")]
    /// <summary>chilecentral</summary>
    Chilecentral,
    [EnumMember(Value = "eastasia"), JsonStringEnumMemberName("eastasia")]
    /// <summary>eastasia</summary>
    Eastasia,
    [EnumMember(Value = "eastasiastage"), JsonStringEnumMemberName("eastasiastage")]
    /// <summary>eastasiastage</summary>
    Eastasiastage,
    [EnumMember(Value = "eastus"), JsonStringEnumMemberName("eastus")]
    /// <summary>eastus</summary>
    Eastus,
    [EnumMember(Value = "eastus2"), JsonStringEnumMemberName("eastus2")]
    /// <summary>eastus2</summary>
    Eastus2,
    [EnumMember(Value = "eastus2euap"), JsonStringEnumMemberName("eastus2euap")]
    /// <summary>eastus2euap</summary>
    Eastus2euap,
    [EnumMember(Value = "eastus2stage"), JsonStringEnumMemberName("eastus2stage")]
    /// <summary>eastus2stage</summary>
    Eastus2stage,
    [EnumMember(Value = "eastusstage"), JsonStringEnumMemberName("eastusstage")]
    /// <summary>eastusstage</summary>
    Eastusstage,
    [EnumMember(Value = "eastusstg"), JsonStringEnumMemberName("eastusstg")]
    /// <summary>eastusstg</summary>
    Eastusstg,
    [EnumMember(Value = "europe"), JsonStringEnumMemberName("europe")]
    /// <summary>europe</summary>
    Europe,
    [EnumMember(Value = "france"), JsonStringEnumMemberName("france")]
    /// <summary>france</summary>
    France,
    [EnumMember(Value = "francecentral"), JsonStringEnumMemberName("francecentral")]
    /// <summary>francecentral</summary>
    Francecentral,
    [EnumMember(Value = "francesouth"), JsonStringEnumMemberName("francesouth")]
    /// <summary>francesouth</summary>
    Francesouth,
    [EnumMember(Value = "germany"), JsonStringEnumMemberName("germany")]
    /// <summary>germany</summary>
    Germany,
    [EnumMember(Value = "germanynorth"), JsonStringEnumMemberName("germanynorth")]
    /// <summary>germanynorth</summary>
    Germanynorth,
    [EnumMember(Value = "germanywestcentral"), JsonStringEnumMemberName("germanywestcentral")]
    /// <summary>germanywestcentral</summary>
    Germanywestcentral,
    [EnumMember(Value = "global"), JsonStringEnumMemberName("global")]
    /// <summary>global</summary>
    Global,
    [EnumMember(Value = "india"), JsonStringEnumMemberName("india")]
    /// <summary>india</summary>
    India,
    [EnumMember(Value = "indonesia"), JsonStringEnumMemberName("indonesia")]
    /// <summary>indonesia</summary>
    Indonesia,
    [EnumMember(Value = "indonesiacentral"), JsonStringEnumMemberName("indonesiacentral")]
    /// <summary>indonesiacentral</summary>
    Indonesiacentral,
    [EnumMember(Value = "israel"), JsonStringEnumMemberName("israel")]
    /// <summary>israel</summary>
    Israel,
    [EnumMember(Value = "israelcentral"), JsonStringEnumMemberName("israelcentral")]
    /// <summary>israelcentral</summary>
    Israelcentral,
    [EnumMember(Value = "italy"), JsonStringEnumMemberName("italy")]
    /// <summary>italy</summary>
    Italy,
    [EnumMember(Value = "italynorth"), JsonStringEnumMemberName("italynorth")]
    /// <summary>italynorth</summary>
    Italynorth,
    [EnumMember(Value = "japan"), JsonStringEnumMemberName("japan")]
    /// <summary>japan</summary>
    Japan,
    [EnumMember(Value = "japaneast"), JsonStringEnumMemberName("japaneast")]
    /// <summary>japaneast</summary>
    Japaneast,
    [EnumMember(Value = "japanwest"), JsonStringEnumMemberName("japanwest")]
    /// <summary>japanwest</summary>
    Japanwest,
    [EnumMember(Value = "jioindiacentral"), JsonStringEnumMemberName("jioindiacentral")]
    /// <summary>jioindiacentral</summary>
    Jioindiacentral,
    [EnumMember(Value = "jioindiawest"), JsonStringEnumMemberName("jioindiawest")]
    /// <summary>jioindiawest</summary>
    Jioindiawest,
    [EnumMember(Value = "korea"), JsonStringEnumMemberName("korea")]
    /// <summary>korea</summary>
    Korea,
    [EnumMember(Value = "koreacentral"), JsonStringEnumMemberName("koreacentral")]
    /// <summary>koreacentral</summary>
    Koreacentral,
    [EnumMember(Value = "koreasouth"), JsonStringEnumMemberName("koreasouth")]
    /// <summary>koreasouth</summary>
    Koreasouth,
    [EnumMember(Value = "malaysia"), JsonStringEnumMemberName("malaysia")]
    /// <summary>malaysia</summary>
    Malaysia,
    [EnumMember(Value = "malaysiawest"), JsonStringEnumMemberName("malaysiawest")]
    /// <summary>malaysiawest</summary>
    Malaysiawest,
    [EnumMember(Value = "mexico"), JsonStringEnumMemberName("mexico")]
    /// <summary>mexico</summary>
    Mexico,
    [EnumMember(Value = "mexicocentral"), JsonStringEnumMemberName("mexicocentral")]
    /// <summary>mexicocentral</summary>
    Mexicocentral,
    [EnumMember(Value = "newzealand"), JsonStringEnumMemberName("newzealand")]
    /// <summary>newzealand</summary>
    Newzealand,
    [EnumMember(Value = "newzealandnorth"), JsonStringEnumMemberName("newzealandnorth")]
    /// <summary>newzealandnorth</summary>
    Newzealandnorth,
    [EnumMember(Value = "northcentralus"), JsonStringEnumMemberName("northcentralus")]
    /// <summary>northcentralus</summary>
    Northcentralus,
    [EnumMember(Value = "northcentralusstage"), JsonStringEnumMemberName("northcentralusstage")]
    /// <summary>northcentralusstage</summary>
    Northcentralusstage,
    [EnumMember(Value = "northeurope"), JsonStringEnumMemberName("northeurope")]
    /// <summary>northeurope</summary>
    Northeurope,
    [EnumMember(Value = "norway"), JsonStringEnumMemberName("norway")]
    /// <summary>norway</summary>
    Norway,
    [EnumMember(Value = "norwayeast"), JsonStringEnumMemberName("norwayeast")]
    /// <summary>norwayeast</summary>
    Norwayeast,
    [EnumMember(Value = "norwaywest"), JsonStringEnumMemberName("norwaywest")]
    /// <summary>norwaywest</summary>
    Norwaywest,
    [EnumMember(Value = "poland"), JsonStringEnumMemberName("poland")]
    /// <summary>poland</summary>
    Poland,
    [EnumMember(Value = "polandcentral"), JsonStringEnumMemberName("polandcentral")]
    /// <summary>polandcentral</summary>
    Polandcentral,
    [EnumMember(Value = "qatar"), JsonStringEnumMemberName("qatar")]
    /// <summary>qatar</summary>
    Qatar,
    [EnumMember(Value = "qatarcentral"), JsonStringEnumMemberName("qatarcentral")]
    /// <summary>qatarcentral</summary>
    Qatarcentral,
    [EnumMember(Value = "singapore"), JsonStringEnumMemberName("singapore")]
    /// <summary>singapore</summary>
    Singapore,
    [EnumMember(Value = "southafrica"), JsonStringEnumMemberName("southafrica")]
    /// <summary>southafrica</summary>
    Southafrica,
    [EnumMember(Value = "southafricanorth"), JsonStringEnumMemberName("southafricanorth")]
    /// <summary>southafricanorth</summary>
    Southafricanorth,
    [EnumMember(Value = "southafricawest"), JsonStringEnumMemberName("southafricawest")]
    /// <summary>southafricawest</summary>
    Southafricawest,
    [EnumMember(Value = "southcentralus"), JsonStringEnumMemberName("southcentralus")]
    /// <summary>southcentralus</summary>
    Southcentralus,
    [EnumMember(Value = "southcentralusstage"), JsonStringEnumMemberName("southcentralusstage")]
    /// <summary>southcentralusstage</summary>
    Southcentralusstage,
    [EnumMember(Value = "southcentralusstg"), JsonStringEnumMemberName("southcentralusstg")]
    /// <summary>southcentralusstg</summary>
    Southcentralusstg,
    [EnumMember(Value = "southeastasia"), JsonStringEnumMemberName("southeastasia")]
    /// <summary>southeastasia</summary>
    Southeastasia,
    [EnumMember(Value = "southeastasiastage"), JsonStringEnumMemberName("southeastasiastage")]
    /// <summary>southeastasiastage</summary>
    Southeastasiastage,
    [EnumMember(Value = "southindia"), JsonStringEnumMemberName("southindia")]
    /// <summary>southindia</summary>
    Southindia,
    [EnumMember(Value = "spain"), JsonStringEnumMemberName("spain")]
    /// <summary>spain</summary>
    Spain,
    [EnumMember(Value = "spaincentral"), JsonStringEnumMemberName("spaincentral")]
    /// <summary>spaincentral</summary>
    Spaincentral,
    [EnumMember(Value = "sweden"), JsonStringEnumMemberName("sweden")]
    /// <summary>sweden</summary>
    Sweden,
    [EnumMember(Value = "swedencentral"), JsonStringEnumMemberName("swedencentral")]
    /// <summary>swedencentral</summary>
    Swedencentral,
    [EnumMember(Value = "switzerland"), JsonStringEnumMemberName("switzerland")]
    /// <summary>switzerland</summary>
    Switzerland,
    [EnumMember(Value = "switzerlandnorth"), JsonStringEnumMemberName("switzerlandnorth")]
    /// <summary>switzerlandnorth</summary>
    Switzerlandnorth,
    [EnumMember(Value = "switzerlandwest"), JsonStringEnumMemberName("switzerlandwest")]
    /// <summary>switzerlandwest</summary>
    Switzerlandwest,
    [EnumMember(Value = "taiwan"), JsonStringEnumMemberName("taiwan")]
    /// <summary>taiwan</summary>
    Taiwan,
    [EnumMember(Value = "uae"), JsonStringEnumMemberName("uae")]
    /// <summary>uae</summary>
    Uae,
    [EnumMember(Value = "uaecentral"), JsonStringEnumMemberName("uaecentral")]
    /// <summary>uaecentral</summary>
    Uaecentral,
    [EnumMember(Value = "uaenorth"), JsonStringEnumMemberName("uaenorth")]
    /// <summary>uaenorth</summary>
    Uaenorth,
    [EnumMember(Value = "uk"), JsonStringEnumMemberName("uk")]
    /// <summary>uk</summary>
    Uk,
    [EnumMember(Value = "uksouth"), JsonStringEnumMemberName("uksouth")]
    /// <summary>uksouth</summary>
    Uksouth,
    [EnumMember(Value = "ukwest"), JsonStringEnumMemberName("ukwest")]
    /// <summary>ukwest</summary>
    Ukwest,
    [EnumMember(Value = "unitedstates"), JsonStringEnumMemberName("unitedstates")]
    /// <summary>unitedstates</summary>
    Unitedstates,
    [EnumMember(Value = "unitedstateseuap"), JsonStringEnumMemberName("unitedstateseuap")]
    /// <summary>unitedstateseuap</summary>
    Unitedstateseuap,
    [EnumMember(Value = "westcentralus"), JsonStringEnumMemberName("westcentralus")]
    /// <summary>westcentralus</summary>
    Westcentralus,
    [EnumMember(Value = "westeurope"), JsonStringEnumMemberName("westeurope")]
    /// <summary>westeurope</summary>
    Westeurope,
    [EnumMember(Value = "westindia"), JsonStringEnumMemberName("westindia")]
    /// <summary>westindia</summary>
    Westindia,
    [EnumMember(Value = "westus"), JsonStringEnumMemberName("westus")]
    /// <summary>westus</summary>
    Westus,
    [EnumMember(Value = "westus2"), JsonStringEnumMemberName("westus2")]
    /// <summary>westus2</summary>
    Westus2,
    [EnumMember(Value = "westus2stage"), JsonStringEnumMemberName("westus2stage")]
    /// <summary>westus2stage</summary>
    Westus2stage,
    [EnumMember(Value = "westus3"), JsonStringEnumMemberName("westus3")]
    /// <summary>westus3</summary>
    Westus3,
    [EnumMember(Value = "westusstage"), JsonStringEnumMemberName("westusstage")]
    /// <summary>westusstage</summary>
    Westusstage
}

#nullable enable
/// <summary></summary>
[global::System.CodeDom.Compiler.GeneratedCode("KubernetesCRDModelGen.Tool", "1.0.0.0"), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class V1alpha2XStorageBucketSpecParameters
{
    public V1alpha2XStorageBucketSpecParameters()
    {
    }

    /// <summary>Access control list for the storage bucket. Private, Blob (anonymous access for blobs), Container (anonymous access for containers and blobs)</summary>
    [JsonPropertyName("acl")]
    [JsonConverter(typeof(JsonStringEnumConverter<V1alpha2XStorageBucketSpecParametersAclEnum>))]
    public V1alpha2XStorageBucketSpecParametersAclEnum Acl { get; set; }

    /// <summary>Geographic location where the storage bucket will be created</summary>
    [JsonPropertyName("location")]
    [JsonConverter(typeof(JsonStringEnumConverter<V1alpha2XStorageBucketSpecParametersLocationEnum>))]
    public V1alpha2XStorageBucketSpecParametersLocationEnum Location { get; set; }

    /// <summary>Enable versioning to maintain multiple versions of objects in the bucket</summary>
    [JsonPropertyName("versioning")]
    public bool Versioning { get; set; }
}
#nullable disable

#nullable enable
/// <summary></summary>
[global::System.CodeDom.Compiler.GeneratedCode("KubernetesCRDModelGen.Tool", "1.0.0.0"), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class V1alpha2XStorageBucketSpecCrossplaneCompositionRef
{
    public V1alpha2XStorageBucketSpecCrossplaneCompositionRef()
    {
    }

    /// <summary></summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }
}
#nullable disable

#nullable enable
/// <summary></summary>
[global::System.CodeDom.Compiler.GeneratedCode("KubernetesCRDModelGen.Tool", "1.0.0.0"), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class V1alpha2XStorageBucketSpecCrossplaneCompositionRevisionRef
{
    public V1alpha2XStorageBucketSpecCrossplaneCompositionRevisionRef()
    {
    }

    /// <summary></summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }
}
#nullable disable

#nullable enable
/// <summary></summary>
[global::System.CodeDom.Compiler.GeneratedCode("KubernetesCRDModelGen.Tool", "1.0.0.0"), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class V1alpha2XStorageBucketSpecCrossplaneCompositionRevisionSelector
{
    public V1alpha2XStorageBucketSpecCrossplaneCompositionRevisionSelector()
    {
    }

    /// <summary></summary>
    [JsonPropertyName("matchLabels")]
    public IDictionary<string, string> MatchLabels { get; set; }
}
#nullable disable

#nullable enable
/// <summary></summary>
[global::System.CodeDom.Compiler.GeneratedCode("KubernetesCRDModelGen.Tool", "1.0.0.0"), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class V1alpha2XStorageBucketSpecCrossplaneCompositionSelector
{
    public V1alpha2XStorageBucketSpecCrossplaneCompositionSelector()
    {
    }

    /// <summary></summary>
    [JsonPropertyName("matchLabels")]
    public IDictionary<string, string> MatchLabels { get; set; }
}
#nullable disable

/// <summary></summary>
[global::System.CodeDom.Compiler.GeneratedCode("KubernetesCRDModelGen.Tool", "1.0.0.0")]
public enum V1alpha2XStorageBucketSpecCrossplaneCompositionUpdatePolicyEnum
{
    [EnumMember(Value = "Automatic"), JsonStringEnumMemberName("Automatic")]
    /// <summary>Automatic</summary>
    Automatic,
    [EnumMember(Value = "Manual"), JsonStringEnumMemberName("Manual")]
    /// <summary>Manual</summary>
    Manual
}

#nullable enable
/// <summary></summary>
[global::System.CodeDom.Compiler.GeneratedCode("KubernetesCRDModelGen.Tool", "1.0.0.0"), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class V1alpha2XStorageBucketSpecCrossplaneResourceRefs
{
    public V1alpha2XStorageBucketSpecCrossplaneResourceRefs()
    {
    }

    /// <summary></summary>
    [JsonPropertyName("apiVersion")]
    public string ApiVersion { get; set; }

    /// <summary></summary>
    [JsonPropertyName("kind")]
    public string Kind { get; set; }

    /// <summary></summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
#nullable disable

#nullable enable
/// <summary>Configures how Crossplane will reconcile this composite resource</summary>
[global::System.CodeDom.Compiler.GeneratedCode("KubernetesCRDModelGen.Tool", "1.0.0.0"), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class V1alpha2XStorageBucketSpecCrossplane
{
    public V1alpha2XStorageBucketSpecCrossplane()
    {
    }

    /// <summary></summary>
    [JsonPropertyName("compositionRef")]
    public V1alpha2XStorageBucketSpecCrossplaneCompositionRef? CompositionRef { get; set; }

    /// <summary></summary>
    [JsonPropertyName("compositionRevisionRef")]
    public V1alpha2XStorageBucketSpecCrossplaneCompositionRevisionRef? CompositionRevisionRef { get; set; }

    /// <summary></summary>
    [JsonPropertyName("compositionRevisionSelector")]
    public V1alpha2XStorageBucketSpecCrossplaneCompositionRevisionSelector? CompositionRevisionSelector { get; set; }

    /// <summary></summary>
    [JsonPropertyName("compositionSelector")]
    public V1alpha2XStorageBucketSpecCrossplaneCompositionSelector? CompositionSelector { get; set; }

    /// <summary></summary>
    [JsonPropertyName("compositionUpdatePolicy")]
    [JsonConverter(typeof(JsonStringEnumConverter<V1alpha2XStorageBucketSpecCrossplaneCompositionUpdatePolicyEnum>))]
    public V1alpha2XStorageBucketSpecCrossplaneCompositionUpdatePolicyEnum? CompositionUpdatePolicy { get; set; }

    /// <summary></summary>
    [JsonPropertyName("resourceRefs")]
    public IList<V1alpha2XStorageBucketSpecCrossplaneResourceRefs>? ResourceRefs { get; set; }
}
#nullable disable

#nullable enable
/// <summary>StorageBucketSpec defines the desired state of StorageBucket.</summary>
[global::System.CodeDom.Compiler.GeneratedCode("KubernetesCRDModelGen.Tool", "1.0.0.0"), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class V1alpha2XStorageBucketSpec
{
    public V1alpha2XStorageBucketSpec()
    {
    }

    /// <summary></summary>
    [JsonPropertyName("parameters")]
    public V1alpha2XStorageBucketSpecParameters Parameters { get; set; }

    /// <summary>Configures how Crossplane will reconcile this composite resource</summary>
    [JsonPropertyName("crossplane")]
    public V1alpha2XStorageBucketSpecCrossplane? Crossplane { get; set; }
}
#nullable disable

#nullable enable
/// <summary></summary>
[global::System.CodeDom.Compiler.GeneratedCode("KubernetesCRDModelGen.Tool", "1.0.0.0"), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class V1alpha2XStorageBucketStatusConditions
{
    public V1alpha2XStorageBucketStatusConditions()
    {
    }

    /// <summary></summary>
    [JsonPropertyName("lastTransitionTime")]
    public string LastTransitionTime { get; set; }

    /// <summary></summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary></summary>
    [JsonPropertyName("observedGeneration")]
    public long? ObservedGeneration { get; set; }

    /// <summary></summary>
    [JsonPropertyName("reason")]
    public string Reason { get; set; }

    /// <summary></summary>
    [JsonPropertyName("status")]
    public string Status { get; set; }

    /// <summary></summary>
    [JsonPropertyName("type")]
    public string Type { get; set; }
}
#nullable disable

#nullable enable
/// <summary>Status defines the observed state of the CompositeResourceDefinition.</summary>
[global::System.CodeDom.Compiler.GeneratedCode("KubernetesCRDModelGen.Tool", "1.0.0.0"), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class V1alpha2XStorageBucketStatus
{
    public V1alpha2XStorageBucketStatus()
    {
    }

    /// <summary>Conditions of the resource.</summary>
    [JsonPropertyName("conditions")]
    public IList<V1alpha2XStorageBucketStatusConditions>? Conditions { get; set; }
}
#nullable disable

#nullable enable
/// <summary>StorageBucket is the Schema for the StorageBucket API.</summary>
[global::System.CodeDom.Compiler.GeneratedCode("KubernetesCRDModelGen.Tool", "1.0.0.0"), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
[KubernetesEntity(Group = KubeGroup, Kind = KubeKind, ApiVersion = KubeApiVersion, PluralName = KubePluralName)]
public partial class V1alpha2XStorageBucket : IKubernetesObject<V1ObjectMeta>, ISpec<V1alpha2XStorageBucketSpec>, IStatus<V1alpha2XStorageBucketStatus>
{
    public V1alpha2XStorageBucket()
    {
    }

    public const string KubeApiVersion = "v1alpha2";
    public const string KubeKind = "XStorageBucket";
    public const string KubeGroup = "platform.example.com";
    public const string KubePluralName = "xstoragebuckets";
    /// <summary>APIVersion defines the versioned schema of this representation of an object. Servers should convert recognized schemas to the latest internal value, and may reject unrecognized values. More info: https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#resources</summary>
    [JsonPropertyName("apiVersion")]
    public string ApiVersion { get; set; } = "platform.example.com/v1alpha2";

    /// <summary>Kind is a string value representing the REST resource this object represents. Servers may infer this from the endpoint the client submits requests to. Cannot be updated. In CamelCase. More info: https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#types-kinds</summary>
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "XStorageBucket";

    /// <summary>Standard object's metadata. More info: https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#metadata</summary>
    [JsonPropertyName("metadata")]
    public V1ObjectMeta Metadata { get; set; }

    /// <summary>StorageBucketSpec defines the desired state of StorageBucket.</summary>
    [JsonPropertyName("spec")]
    public V1alpha2XStorageBucketSpec Spec { get; set; }

    /// <summary>Status defines the observed state of the CompositeResourceDefinition.</summary>
    [JsonPropertyName("status")]
    public V1alpha2XStorageBucketStatus? Status { get; set; }
}
#nullable disable
