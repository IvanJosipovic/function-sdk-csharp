using Apiextensions.Fn.Proto.V1;
using EnumsNET;
using Function.SDK.CSharp.SourceGenerator.Models.platform.example.com;
using KubernetesCRDModelGen.Models.azure.m.upbound.io;
using Shouldly;

namespace Function.SDK.CSharp.Example.Tests;

public class UnitTest1
{
    [Fact]
    public void TestDesiredResourceReplacement()
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
            Metadata = new()
            {
                Name = xr.Metadata.Name.Replace("-", ""),
                NamespaceProperty = xr.Metadata.NamespaceProperty
            },
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
            Metadata = new()
            {
                Name = xr.Metadata.Name.Replace("-", ""),
                NamespaceProperty = xr.Metadata.NamespaceProperty
            },
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
                        LastTransitionTime = DateTime.SpecifyKind(DateTime.Parse("2025-01-01T08:00:00Z"), DateTimeKind.Utc),
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

        response2.Desired.GetResource<V1beta1ResourceGroup>("rg").ShouldBeEquivalentTo(desiredResource);
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
                        LastTransitionTime = DateTime.SpecifyKind(DateTime.Parse("2025-01-01T08:00:00Z"), DateTimeKind.Utc),
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
                        LastTransitionTime = DateTime.SpecifyKind(DateTime.Parse("2025-01-01T08:00:00Z"), DateTimeKind.Utc),
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
    public void TestReadyBecomingFalseAfterPreviouslyBeingTrue()
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

        var readyObservedResource = new V1beta1ResourceGroup()
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
                        LastTransitionTime = DateTime.UnixEpoch,
                        Reason = "test",
                        Status = "True",
                        Type = "Ready"
                    }
                ]
            }
        };

        var notReadyObservedResource = new V1beta1ResourceGroup()
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
                        LastTransitionTime = DateTime.UnixEpoch,
                        Reason = "test",
                        Status = "False",
                        Type = "Ready"
                    }
                ]
            }
        };

        var request1 = TestExtensions.GetFunctionRequest();
        request1.SetCompositeResource(xr);
        request1.Desired.AddOrUpdate("rg", desiredResource);
        request1.Observed.AddOrUpdate("rg", readyObservedResource);

        var response1 = request1.GetTestResponse();
        response1.Desired.Resources["rg"].Ready.ShouldBe(Ready.True);

        var request2 = TestExtensions.GetFunctionRequest();
        request2.SetCompositeResource(xr);
        request2.Desired.MergeFrom(response1.Desired);
        request2.Observed.AddOrUpdate("rg", notReadyObservedResource);

        var response2 = request2.GetTestResponse();
        response2.Desired.Resources["rg"].Ready.ShouldBe(Ready.False);
    }

    [Theory]
    [InlineData(true, false, true, true, false)]
    [InlineData(false, false, true, false, false)]
    [InlineData(true, true, true, true, true)]
    [InlineData(true, true, false, false, false)]
    public void TestReadyReflectsSyncAndReadyHealth(bool healthySynced, bool failedSynced, bool ready, bool expectedResponse1Ready, bool expectedResponse2Ready)
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

        var desiredResource = new V1beta1ResourceGroup
        {
            Spec = new()
            {
                ForProvider = new()
                {
                    Location = xr.Spec.Parameters.Location.AsString(EnumFormat.EnumMemberValue)
                }
            }
        };

        var healthyObservedResource = new V1beta1ResourceGroup
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
                    new() { Type = "Ready", Status = "True", Reason = "Available", LastTransitionTime = DateTime.UnixEpoch },
                    new() { Type = "Synced", Status = "True", Reason = "ReconcileSuccess", LastTransitionTime = DateTime.UnixEpoch },
                    new() { Type = "LastAsyncOperation", Status = "True", Reason = "Success", LastTransitionTime = DateTime.UnixEpoch }
                ]
            }
        };

        var failedObservedResource = new V1beta1ResourceGroup
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
                    new() { Type = "Ready", Status = "True", Reason = "Available", LastTransitionTime = DateTime.UnixEpoch },
                    new() { Type = "Synced", Status = "False", Reason = "ReconcileError", LastTransitionTime = DateTime.UnixEpoch },
                    new() { Type = "LastAsyncOperation", Status = "False", Reason = "AsyncUpdateFailure", LastTransitionTime = DateTime.UnixEpoch }
                ]
            }
        };

        healthyObservedResource.Status!.Conditions![0].Status = ready ? "True" : "False";
        failedObservedResource.Status!.Conditions![0].Status = ready ? "True" : "False";
        healthyObservedResource.Status!.Conditions![1].Status = healthySynced ? "True" : "False";
        failedObservedResource.Status!.Conditions![1].Status = failedSynced ? "True" : "False";

        var request1 = TestExtensions.GetFunctionRequest();
        request1.SetCompositeResource(xr);
        request1.Desired.AddOrUpdate("rg", desiredResource);
        request1.Observed.AddOrUpdate("rg", healthyObservedResource);
        var response1 = request1.GetTestResponse();
        response1.Desired.Resources["rg"].Ready.ShouldBe(expectedResponse1Ready ? Ready.True : Ready.False);

        var request2 = TestExtensions.GetFunctionRequest();
        request2.SetCompositeResource(xr);
        request2.Desired.MergeFrom(response1.Desired);
        request2.Observed.AddOrUpdate("rg", failedObservedResource);

        var response2 = request2.GetTestResponse();
        response2.Desired.Resources["rg"].Ready.ShouldBe(expectedResponse2Ready ? Ready.True : Ready.False);
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

        observedResource.Status = new()
        {
            Conditions =
            [
                new()
                {
                    Type = "Synced",
                    Status = "False",
                    Reason = "ReconcileError",
                    LastTransitionTime = DateTime.UnixEpoch
                }
            ]
        };

        var request2 = TestExtensions.GetFunctionRequest();
        request2.SetCompositeResource(xr);
        request2.Desired.MergeFrom(response1.Desired);
        request2.Observed.AddOrUpdate("resource", observedResource);

        var response2 = request2.GetTestResponse();
        response2.Desired.Resources["resource"].Ready.ShouldBe(Ready.False);
    }
}
