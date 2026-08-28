using Apiextensions.Fn.Proto.V1;
using k8s.Models;
using KubernetesCRDModelGen.Models.azure.m.upbound.io;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace Function.SDK.CSharp.Example.Tests;

public class CustomReadinessChecksTests
{
    [Fact]
    public void UpdateDesiredReadyStatusMarksCustomResourceReadyWhenAllChecksPass()
    {
        var (request, response) = CreateProviderConfigResponse();

        response.UpdateDesiredReadyStatus(
            request,
            NullLogger.Instance,
            [
                ResourceReadinessCheck.For<V1beta1ProviderConfig>(
                    static _ => true,
                    static _ => true)
            ]);

        response.Desired.Resources["resource"].Ready.ShouldBe(Ready.True);
    }

    [Fact]
    public void UpdateDesiredReadyStatusMarksCustomResourceNotReadyWhenAnyCheckFails()
    {
        var (request, response) = CreateProviderConfigResponse();

        response.UpdateDesiredReadyStatus(
            request,
            NullLogger.Instance,
            [
                ResourceReadinessCheck.For<V1beta1ProviderConfig>(
                    static _ => true,
                    static _ => false)
            ]);

        response.Desired.Resources["resource"].Ready.ShouldBe(Ready.False);
    }

    [Fact]
    public void UpdateDesiredReadyStatusAppliesCustomChecksOnlyToExactGvk()
    {
        var request = TestExtensions.GetFunctionRequest();
        var configMap = new V1ConfigMap();
        request.Desired.AddOrUpdate("resource", configMap);
        request.Observed.AddOrUpdate("resource", configMap);
        var response = request.To();

        response.UpdateDesiredReadyStatus(
            request,
            NullLogger.Instance,
            [
                ResourceReadinessCheck.For<V1beta1ProviderConfig>(
                    static _ => throw new InvalidOperationException("The predicate must not run for another GVK."))
            ]);

        response.Desired.Resources["resource"].Ready.ShouldBe(Ready.True);
    }

    [Fact]
    public void UpdateDesiredReadyStatusRejectsEmptyCustomChecks()
    {
        var (request, response) = CreateProviderConfigResponse();

        Should.Throw<ArgumentException>(() =>
            response.UpdateDesiredReadyStatus(
                request,
                NullLogger.Instance,
                []));
    }

    private static (RunFunctionRequest Request, RunFunctionResponse Response) CreateProviderConfigResponse()
    {
        var request = TestExtensions.GetFunctionRequest();
        var providerConfig = new V1beta1ProviderConfig
        {
            Spec = new()
            {
                Credentials = new()
                {
                    Source = new()
                }
            }
        };
        request.Desired.AddOrUpdate("resource", providerConfig);
        request.Observed.AddOrUpdate("resource", providerConfig);

        return (request, request.To());
    }
}
