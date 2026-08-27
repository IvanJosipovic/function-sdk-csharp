using Apiextensions.Fn.Proto.V1;
using EnumsNET;
using Function.SDK.CSharp.SourceGenerator.Models.platform.example.com;
using Grpc.Core;
using k8s.Models;
using KubernetesCRDModelGen.Models.azure.m.upbound.io;
using KubernetesCRDModelGen.Models.storage.azure.m.upbound.io;
using Microsoft.Extensions.Logging;
using static Apiextensions.Fn.Proto.V1.FunctionRunnerService;

namespace Function.SDK.CSharp.Example.Tests;

public class TestRunFunctionService(ILogger<TestRunFunctionService> logger) : FunctionRunnerServiceBase
{
    public override Task<RunFunctionResponse> RunFunction(RunFunctionRequest request, ServerCallContext context)
    {
        var resp = request.To(RequestExtensions.DefaultTTL);

        var observedXR = request.GetObservedCompositeResource<V1alpha1XStorageBucket>();

        if (observedXR == null)
        {
            resp.Fatal("XR is null");
            return Task.FromResult(resp);
        }

        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["xr-apiversion"] = observedXR.ApiVersion,
            ["xr-kind"] = observedXR.Kind,
            ["xr-name"] = observedXR.Name()
        }))
        {
            logger.LogInformation("Running Function");

            var @params = observedXR.Spec.Parameters;

            // Create Resource Group
            var desiredGroup = new V1beta1ResourceGroup()
            {
                Metadata = new()
                {
                    Name = observedXR.Metadata.Name.Replace("-", ""),
                    NamespaceProperty = observedXR.Metadata.NamespaceProperty
                },
                Spec = new V1beta1ResourceGroupSpec
                {
                    ForProvider = new()
                    {
                        Location = @params.Location.AsString(EnumFormat.EnumMemberValue)
                    }
                }
            };

            resp.Desired.AddOrUpdate("rg", desiredGroup);

            // Example dependent Cron Job which deploys if the Resource Group is ready
            //var observedGroup = request.GetObservedResource<V1beta1ResourceGroup>("rg");
            //if (observedGroup != null && observedGroup.Status?.Conditions?.First(x => x.Type == "Ready").Status.Equals("True") == true)
            //{
            //    var cron = new V1CronJob()
            //    {
            //        Spec = new()
            //        {
            //            Suspend = false
            //        }
            //    };

            //    resp.Desired.AddOrUpdate("cron", cron);
            //}

            // Create Storage Account
            var desiredAccount = new V1beta1Account()
            {
                Metadata = new()
                {
                    Name = observedXR.Metadata.Name.Replace("-", ""),
                    NamespaceProperty = observedXR.Metadata.NamespaceProperty
                },
                Spec = new()
                {
                    ForProvider = new()
                    {
                        AccountTier = "Standard",
                        AccountReplicationType = "LRS",
                        Location = @params.Location.AsString(EnumFormat.EnumMemberValue),
                        InfrastructureEncryptionEnabled = true,
                        PublicNetworkAccessEnabled = @params.Public,
                        BlobProperties = new()
                        {
                            VersioningEnabled = @params.Versioning
                        },
                        ResourceGroupNameSelector = new()
                        {
                            MatchControllerRef = true
                        }
                    }
                }
            };

            resp.Desired.AddOrUpdate("account", desiredAccount);

            // Create Container
            var desiredContainer = new V1beta1Container()
            {
                Metadata = new()
                {
                    Name = observedXR.Metadata.Name.Replace("-", ""),
                    NamespaceProperty = observedXR.Metadata.NamespaceProperty
                },
                Spec = new()
                {
                    ForProvider = new()
                    {
                        ContainerAccessType = @params.Acl.AsString(EnumFormat.EnumMemberValue),
                        StorageAccountNameSelector = new()
                        {
                            MatchControllerRef = true
                        }
                    }
                }
            };

            resp.Desired.AddOrUpdate("container", desiredContainer);

            // Get Desired resources and update Status if Ready
            resp.UpdateDesiredReadyStatus(request, logger,
            [
                typeof(V1beta1ProviderConfig)
            ]);

            return Task.FromResult(resp);
        }
    }
}
