using EnumsNET;
using Function.SDK.CSharp;
using Function.SDK.CSharp.SourceGenerator.Models.platform.example.com;
using KubernetesCRDModelGen.Models.storage.azure.m.upbound.io;
using KubernetesCRDModelGen.Models.azure.m.upbound.io;
using Shouldly;

namespace Function.SDK.CSharp.Sample.Tests;

public class UnitTest1
{
    [Fact]
    public void TestDesired()
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

        var response = request.GetTestResponse();

        var expectedResourceGroup = new V1beta1ResourceGroup()
        {
            Metadata = new()
            {
                Name = xr.Metadata.Name.Replace("-", ""),
                NamespaceProperty = xr.Metadata.NamespaceProperty
            },
            Spec = new V1beta1ResourceGroupSpec
            {
                ForProvider = new()
                {
                    Location = xr.Spec.Parameters.Location.AsString(EnumFormat.EnumMemberValue)
                }
            }
        };

        response.Desired.GetResource<V1beta1ResourceGroup>("rg").ShouldBeEquivalentTo(expectedResourceGroup);

        var expectedAccount = new V1beta1Account()
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
                    AccountTier = "Standard",
                    AccountReplicationType = "LRS",
                    Location = xr.Spec.Parameters.Location.AsString(EnumFormat.EnumMemberValue),
                    InfrastructureEncryptionEnabled = true,
                    PublicNetworkAccessEnabled = xr.Spec.Parameters.Public,
                    BlobProperties = new()
                    {
                        VersioningEnabled = xr.Spec.Parameters.Versioning
                    },
                    ResourceGroupNameSelector = new()
                    {
                        MatchControllerRef = true
                    }
                }
            }
        };

        response.Desired.GetResource<V1beta1Account>("account").ShouldBeEquivalentTo(expectedAccount);

        var desiredContainer = new V1beta1Container()
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
                    ContainerAccessType = xr.Spec.Parameters.Acl.AsString(EnumFormat.EnumMemberValue),
                    StorageAccountNameSelector = new()
                    {
                        MatchControllerRef = true
                    }
                }
            }
        };
        response.Desired.GetResource<V1beta1Container>("container").ShouldBeEquivalentTo(desiredContainer);

    }
}
