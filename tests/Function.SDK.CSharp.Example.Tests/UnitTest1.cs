using Apiextensions.Fn.Proto.V1;
using EnumsNET;
using Function.SDK.CSharp.SourceGenerator.Models.platform.example.com;
using KubernetesCRDModelGen.Models.azure.m.upbound.io;
using Shouldly;

namespace Function.SDK.CSharp.Example.Tests;

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
                        Status = "Ready",
                        LastTransitionTime = "01/01/2025",
                        Reason = "test",
                        Type = "testType"
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
                        Status = "Ready",
                        LastTransitionTime = "01/01/2025",
                        Reason = "test",
                        Type = "testType"
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
                        LastTransitionTime = "01/01/2025",
                        Reason = "test",
                        Status = "True",
                        Type = "Ready",
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

    [Fact]
    public void TestReadyIgnore()
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

        var desiredResource = new V1beta1ProviderConfig()
        {
            Spec = new()
            {
                Credentials = new()
                {
                    Source = new()
                    {
                    }
                }
            }
        };

        var observedResource = new V1beta1ProviderConfig()
        {
            Spec = new()
            {
                Credentials = new()
                {
                    Source = new()
                    {
                    }
                }
            }
        };

        var request = TestExtensions.GetFunctionRequest();
        request.SetCompositeResource(xr);
        request.Desired.AddOrUpdate("resource", desiredResource);
        request.Observed.AddOrUpdate("resource", observedResource);

        var response1 = request.GetTestResponse();
        var desiredResourceResponse = response1.Desired.Resources["resource"];
        desiredResourceResponse.Ready.ShouldBe(Ready.True);
    }
}
