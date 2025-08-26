using Function.SDK.CSharp.Models;
using Function.SDK.CSharp.SourceGenerator.Models.platform.example.com;
using k8s;
using System.Text.Json;

namespace Function.SDK.CSharp.Sample
{
    public static class ConversionWebhook
    {
        public static V1ConversionReview Convert(V1ConversionReview conversion)
        {
            if (conversion.Kind == V1ConversionReview.KubeKind
                && conversion.ApiVersion == V1ConversionReview.KubeGroup + "/" + V1ConversionReview.KubeApiVersion
                && conversion.Request?.DesiredApiVersion == V1alpha1XStorageBucket.KubeApiVersion
               )
            {
                foreach (var item in conversion.Request.Objects)
                {
                    var source = KubernetesJson.Deserialize<V1alpha2XStorageBucket>(item.GetRawText());
                    source.ApiVersion = V1alpha1XStorageBucket.KubeApiVersion;

                    var converted = JsonSerializer.Deserialize<JsonElement>(KubernetesJson.Serialize(source));

                    conversion.Response.ConvertedObjects.Add(converted);
                }

                conversion.Response.Uid = conversion.Request.Uid;
            }
            else
            {
                conversion.Response.Uid = conversion.Request.Uid;
                conversion.Response.Result.Status = V1ConversionReviewResponseStatusEnum.Failure;
                conversion.Response.Result.Message = "Unknown Version";
            }

            return conversion;
        }
    }
}