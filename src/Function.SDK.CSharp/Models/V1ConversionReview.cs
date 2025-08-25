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
    public V1ConversionReviewConversionRequest? Request { get; set; }

    [JsonPropertyName("response")]
    public V1ConversionReviewConversionResponse? Response { get; set; }
}

public sealed class V1ConversionReviewConversionRequest
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

public sealed class V1ConversionReviewConversionResponse
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
    public JsonElement[] ConvertedObjects { get; set; } = default!;

    [JsonPropertyName("result")]
    public V1ConversionReviewConversionResponseStatus Result { get; set; } = new();
}

public sealed class V1ConversionReviewConversionResponseStatus
{
    [JsonPropertyName("status")]
    public string StatusText { get; set; } = "Success";

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("code")]
    public int? Code { get; set; }
}