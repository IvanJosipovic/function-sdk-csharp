using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Function.SDK.CSharp.Models;

public sealed class V1ConversionReview
{
    [JsonPropertyName("apiVersion")]
    public string ApiVersion { get; set; } = "apiextensions.k8s.io/v1";

    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "ConversionReview";

    [JsonPropertyName("request")]
    public V1ConversionReviewRequest? Request { get; set; }

    [JsonPropertyName("response")]
    public V1ConversionReviewResponse Response { get; set; } = new();
}

public sealed class V1ConversionReviewRequest
{
    /// <summary>
    /// Random uid uniquely identifying this conversion call
    /// </summary>
    [JsonPropertyName("uid")]
    public string Uid { get; set; } = default!;

    /// <summary>
    /// The API group and version the objects should be converted to
    /// </summary>
    [JsonPropertyName("desiredAPIVersion")]
    public string DesiredApiVersion { get; set; } = default!;

    /// <summary>
    /// # The list of objects to convert. May contain one or more objects, in one or more versions.
    /// </summary>
    [JsonPropertyName("objects")]
    public JsonElement[] Objects { get; set; } = default!;
}

public sealed class V1ConversionReviewResponse
{
    /// <summary>
    /// Must match &lt;request.uid&gt;
    /// </summary>
    [JsonPropertyName("uid")]
    public string Uid { get; set; } = "";

    /// <summary>
    /// Objects must match the order of request.objects, and have apiVersion set to &lt;request.desiredAPIVersion&gt;.
    /// kind, metadata.uid, metadata.name, and metadata.namespace fields must not be changed by the webhook.
    /// metadata.labels and metadata.annotations fields may be changed by the webhook.
    /// All other changes to metadata fields by the webhook are ignored.
    /// </summary>
    [JsonPropertyName("convertedObjects")]
    public List<JsonElement> ConvertedObjects { get; set; } = [];

    [JsonPropertyName("result")]
    public V1ConversionReviewResponseStatus Result { get; set; } = new();
}

/// <summary>
/// Webhook Response Status
/// </summary>
public enum V1ConversionReviewResponseStatusEnum
{
    ///<summary>Success</summary>
    [EnumMember(Value = "Success"), JsonStringEnumMemberName("Success")]
    Success,
    ///<summary>Failure</summary>
    [EnumMember(Value = "Failure"), JsonStringEnumMemberName("Failure")]
    Failure,
}

public sealed class V1ConversionReviewResponseStatus
{
    [JsonPropertyName("status")]
    public V1ConversionReviewResponseStatusEnum Status { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("code")]
    public int Code { get; set; } = 200;
}