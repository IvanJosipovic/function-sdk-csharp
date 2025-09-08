using System.Text.Json;
using Function.SDK.CSharp.Models;
using Function.SDK.CSharp.SourceGenerator.Models.platform.example.com;
using k8s;
using Shouldly;

namespace Function.SDK.CSharp.Sample.Tests;

public class ConversionWebhookTests
{
    [Fact]
    public void V2toV1ConversionTest()
    {
        var input = new V1ConversionReview()
        {
            Request = new()
            {
                Uid = Guid.NewGuid().ToString(),
                DesiredApiVersion = V1alpha1XStorageBucket.KubeApiVersion,
                Objects = [
                    new V1alpha2XStorageBucket()
                    {
                        Spec = new()
                        {
                            Parameters = new()
                            {
                                Acl = V1alpha2XStorageBucketSpecParametersAclEnum.Private,
                                Location = V1alpha2XStorageBucketSpecParametersLocationEnum.Asia,
                                Versioning = true
                            }
                        }
                    }.ToJsonElement()
                ]
            }
        };

        var output = ConversionWebhook.Convert(input);

        var response = output.Response;

        response.ShouldNotBeNull();

        response.Uid.ShouldBe(input.Request.Uid);

        response.Result.Code.ShouldBe(200);

        response.ConvertedObjects[0].GetProperty("apiVersion").GetString().ShouldBe(V1alpha1XStorageBucket.KubeApiVersion);
    }
}

public static class Extensions
{
    public static JsonElement ToJsonElement(this IKubernetesObject obj)
    {
        return JsonDocument.Parse(KubernetesJson.Serialize(obj)).RootElement.Clone();
    }
}
