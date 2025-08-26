using Apiextensions.Fn.Proto.V1;
using Function.SDK.CSharp.SourceGenerator.Models.platform.example.com;
using Google.Protobuf.WellKnownTypes;
using Google.Protobuf;
using Grpc.Core.Testing;
using Grpc.Core.Utils;
using Grpc.Core;
using k8s;
using Microsoft.Extensions.Logging;
using Shouldly;
using KubernetesCRDModelGen.Models.azure.m.upbound.io;
using EnumsNET;

namespace Function.SDK.CSharp.Sample.Tests;

public class UnitTest1
{
    [Fact]
    public void TestMerge()
    {
        var xr = new V1alpha1XStorageBucket()
        {
            Metadata = new()
            {
                Name = "test",
                NamespaceProperty = "default"
            },
            Spec = new()
            {
                Parameters = new()
                {
                    Location = V1alpha1XStorageBucketSpecParametersLocationEnum.Eastus,
                    Versioning = true,
                    Acl = V1alpha1XStorageBucketSpecParametersAclEnum.Private,
                }
            }
        };

        var request = TestExtensions.GetFunctionRequest();
        request.SetCompositeResource(xr);

        var response1 = request.GetTestResponse();

        var desiredResource = new V1beta1ResourceGroup()
        {
            Spec = new()
            {
                ForProvider = new()
                {
                    Location = xr.Spec.Parameters.Location.AsString(EnumFormat.EnumMemberValue)
                }
            }
        };

        response1.Desired.GetResource<V1beta1ResourceGroup>("rg").ShouldBeEquivalentTo(desiredResource);

        // Update Desired Resource Status and rerun function

        var desiredResource2 = new V1beta1ResourceGroup()
        {
            Spec = new()
            {
                ForProvider = new()
                {
                    Location = xr.Spec.Parameters.Location.AsString(EnumFormat.EnumMemberValue)
                }
            },
            Status = new()
            {
                Conditions =
                [
                    new()
                    {
                        Status = "Ready"
                    }
                ]
            }
        };

        var request2 = TestExtensions.GetFunctionRequest();
        request2.SetCompositeResource(xr);

        request2.Desired.AddOrUpdate("rg", desiredResource2);

        var response2 = request2.GetTestResponse();

        response2.Desired.GetResource<V1beta1ResourceGroup>("rg").ShouldBeEquivalentTo(desiredResource2);
    }

    [Fact]
    public void TestReadyFalse()
    {
        var xr = new V1alpha1XStorageBucket()
        {
            Metadata = new()
            {
                Name = "test",
                NamespaceProperty = "default"
            },
            Spec = new()
            {
                Parameters = new()
                {
                    Location = V1alpha1XStorageBucketSpecParametersLocationEnum.Eastus,
                    Versioning = true,
                    Acl = V1alpha1XStorageBucketSpecParametersAclEnum.Private,
                }
            }
        };

        var desiredResource = new V1beta1ResourceGroup()
        {
            Spec = new()
            {
                ForProvider = new()
                {
                    Location = xr.Spec.Parameters.Location.AsString(EnumFormat.EnumMemberValue)
                }
            }
        };

        var observedResource = new V1beta1ResourceGroup()
        {
            Spec = new()
            {
                ForProvider = new()
                {
                    Location = xr.Spec.Parameters.Location.AsString(EnumFormat.EnumMemberValue)
                }
            },
            Status = new()
            {
                Conditions =
                [
                    new()
                    {
                        Type = "Ready",
                        Status = "False"
                    }
                ]
            }
        };

        var request = TestExtensions.GetFunctionRequest();
        request.SetCompositeResource(xr);
        request.Desired.AddOrUpdate("rg", desiredResource);
        request.Observed.AddOrUpdate("rg", observedResource);

        var response1 = request.GetTestResponse();

        var desiredResourceResponse = response1.Desired.Resources["rg"];
        desiredResourceResponse.Ready.ShouldBe(Ready.False);
    }

    [Fact]
    public void TestReadyTrue()
    {
        var xr = new V1alpha1XStorageBucket()
        {
            Metadata = new()
            {
                Name = "test",
                NamespaceProperty = "default"
            },
            Spec = new()
            {
                Parameters = new()
                {
                    Location = V1alpha1XStorageBucketSpecParametersLocationEnum.Eastus,
                    Versioning = true,
                    Acl = V1alpha1XStorageBucketSpecParametersAclEnum.Private,
                }
            }
        };

        var desiredResource = new V1beta1ResourceGroup()
        {
            Spec = new()
            {
                ForProvider = new()
                {
                    Location = xr.Spec.Parameters.Location.AsString(EnumFormat.EnumMemberValue)
                }
            }
        };

        var observedResource = new V1beta1ResourceGroup()
        {
            Spec = new()
            {
                ForProvider = new()
                {
                    Location = xr.Spec.Parameters.Location.AsString(EnumFormat.EnumMemberValue)
                }
            },
            Status = new()
            {
                Conditions =
                [
                    new()
                    {
                        Type = "Ready",
                        Status = "True"
                    }
                ]
            }
        };

        var request = TestExtensions.GetFunctionRequest();
        request.SetCompositeResource(xr);
        request.Desired.AddOrUpdate("rg", desiredResource);
        request.Observed.AddOrUpdate("rg", observedResource);

        var response1 = request.GetTestResponse();
        var desiredResourceResponse = response1.Desired.Resources["rg"];
        desiredResourceResponse.Ready.ShouldBe(Ready.True);
    }
}

public static class TestExtensions
{
    public static RunFunctionRequest GetFunctionRequest()
    {
        var request = new RunFunctionRequest
        {
            Observed = new()
            {
                Composite = new()
            },
            Desired = new(),
            Context = new(),
        };

        return request;
    }

    public static RunFunctionResponse GetTestResponse(this RunFunctionRequest request)
    {
        var svc = new RunFunctionService(new LoggerFactory().CreateLogger<RunFunctionService>());
        var fakeServerCallContext = TestServerCallContext.Create("/apiextensions.fn.proto.v1.FunctionRunnerService/RunFunction", null, DateTime.UtcNow.AddHours(1), [], CancellationToken.None, "127.0.0.1", null, null, (metadata) => TaskUtils.CompletedTask, () => new WriteOptions(), (writeOptions) => { });

        return svc.RunFunction(request, fakeServerCallContext)
            .GetAwaiter()
            .GetResult();
    }

    public static void SetCompositeResource(this RunFunctionRequest request, IKubernetesObject obj)
    {
        var kubeObj = Struct.Parser.ParseJson(KubernetesJson.Serialize(obj));
        request.Observed.Composite.Resource_ = kubeObj;
    }

    public static T GetResource<T>(this State state, string key)
    {
        string json = JsonFormatter.Default.Format(state.Resources[key].Resource_);

        return KubernetesJson.Deserialize<T>(json);
    }
}