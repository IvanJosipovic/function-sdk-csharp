using Apiextensions.Fn.Proto.V1;
using k8s.Models;
using KubernetesCRDModelGen.Models.azure.m.upbound.io;
using KubernetesCRDModelGen.Models.protection.crossplane.io;
using Shouldly;

namespace Function.SDK.CSharp.Example.Tests;

public class ResponseResourceExtensionsTests
{
    [Fact]
    public void WhenAddingGroupedResourceThenCanonicalKeyIsUsed()
    {
        var response = CreateResponse();
        var resource = CreateResourceGroup("example");

        response.AddDesiredResource(resource);

        response.Desired.Resources.ShouldContainKey("azure.m.upbound.io/v1beta1/ResourceGroup/example");
    }

    [Fact]
    public void WhenAddingCoreResourceThenCanonicalKeyIsUsed()
    {
        var response = CreateResponse();
        var resource = CreateConfigMap("example");

        response.AddDesiredResource(resource);

        response.Desired.Resources.ShouldContainKey("v1/ConfigMap/example");
    }

    [Fact]
    public void WhenAddingResourceWithoutTypeIdentityThenIdentityIsInitialized()
    {
        var response = CreateResponse();
        var resource = CreateConfigMap("example");
        resource.ApiVersion = string.Empty;
        resource.Kind = string.Empty;

        response.AddDesiredResource(resource);

        (resource.ApiVersion, resource.Kind).ShouldBe(("v1", "ConfigMap"));
    }

    [Fact]
    public void WhenAddingUnnamedResourceWithFallbackThenFallbackKeyIsUsed()
    {
        var response = CreateResponse();
        var resource = new V1ConfigMap();

        response.AddDesiredResource(resource, "fallback");

        response.Desired.Resources.ShouldContainKey("v1/ConfigMap/fallback");
    }

    [Fact]
    public void WhenAddingUnnamedResourceWithoutFallbackThenInvalidOperationIsThrown()
    {
        var response = CreateResponse();
        var resource = new V1ConfigMap();

        Should.Throw<InvalidOperationException>(() => response.AddDesiredResource(resource));
    }

    [Fact]
    public void WhenAddingSameDesiredResourceTwiceThenExistingResourceIsMerged()
    {
        var response = CreateResponse();
        var first = CreateConfigMap("example");
        first.Data = new Dictionary<string, string> { ["first"] = "one" };
        var second = CreateConfigMap("example");
        second.Data = new Dictionary<string, string> { ["second"] = "two" };

        response.AddDesiredResource(first);
        response.AddDesiredResource(second);

        var merged = response.GetDesiredResource<V1ConfigMap>("example");
        (response.Desired.Resources.Count, merged!.Data["second"]).ShouldBe((1, "two"));
    }

    [Fact]
    public void WhenObservedResourceExistsThenCanonicalLookupReturnsIt()
    {
        var request = CreateRequest();
        request.Observed.AddOrUpdate("v1/ConfigMap/example", CreateConfigMap("example"));

        var resource = request.GetObservedResource<V1ConfigMap>("example");

        resource!.Name().ShouldBe("example");
    }

    [Fact]
    public void WhenObservedResourcesContainMultipleTypesThenTypedEnumerationFiltersThem()
    {
        var request = CreateRequest();
        request.Observed.AddOrUpdate("v1/ConfigMap/example", CreateConfigMap("example"));
        request.Observed.AddOrUpdate("v1/Secret/other", new V1Secret { Metadata = new() { Name = "other" } });

        var resources = request.GetObservedResources<V1ConfigMap>().ToList();

        resources.Single().Name().ShouldBe("example");
    }

    [Fact]
    public void WhenDesiredResourceExistsThenCanonicalLookupReturnsIt()
    {
        var response = CreateResponse();
        response.AddDesiredResource(CreateConfigMap("example"));

        var resource = response.GetDesiredResource<V1ConfigMap>("example");

        resource!.Name().ShouldBe("example");
    }

    [Fact]
    public void WhenDesiredResourcesContainMultipleTypesThenTypedEnumerationFiltersThem()
    {
        var response = CreateResponse();
        response.AddDesiredResource(CreateConfigMap("example"));
        response.AddDesiredResource(new V1Secret { Metadata = new() { Name = "other" } });

        var resources = response.GetDesiredResources<V1ConfigMap>().ToList();

        resources.Single().Name().ShouldBe("example");
    }

    [Fact]
    public void WhenAddingUsageThenResourceReferencesArePopulated()
    {
        var response = CreateResponse();
        var by = CreateConfigMap("consumer");
        var of = new V1Secret { Metadata = new() { Name = "provider" } };

        response.AddDesiredUsage(by, of, replayDeletion: false);

        var usage = response.GetDesiredResources<V1beta1Usage>().Single();
        (usage.Spec!.By!.ResourceRef!.Name, usage.Spec.Of!.ResourceRef!.Name, usage.Spec.ReplayDeletion)
            .ShouldBe(("consumer", "provider", false));
    }

    [Fact]
    public void WhenAddingUsageWithUnnamedResourceThenInvalidOperationIsThrown()
    {
        var response = CreateResponse();
        var by = new V1ConfigMap();
        var of = new V1Secret { Metadata = new() { Name = "provider" } };

        Should.Throw<InvalidOperationException>(() => response.AddDesiredUsage(by, of));
    }

    [Fact]
    public void WhenDesiredResourceNameIsValidThenValidationSucceeds()
    {
        var response = CreateResponse();
        response.AddDesiredResource(CreateConfigMap("valid-name-1"));

        Should.NotThrow(response.ValidateKubeResourceNames);
    }

    [Fact]
    public void WhenDesiredResourceHasNoNameThenValidationSkipsIt()
    {
        var response = CreateResponse();
        response.AddDesiredResource(new V1ConfigMap(), "fallback");

        Should.NotThrow(response.ValidateKubeResourceNames);
    }

    [Fact]
    public void WhenDesiredResourceNameIsInvalidThenValidationIdentifiesIt()
    {
        var response = CreateResponse();
        response.AddDesiredResource(CreateConfigMap("Invalid_Name"));

        var exception = Should.Throw<ArgumentException>(response.ValidateKubeResourceNames);

        exception.Message.ShouldContain("Invalid_Name");
    }

    private static RunFunctionRequest CreateRequest()
    {
        return new RunFunctionRequest
        {
            Observed = new State(),
            Desired = new State()
        };
    }

    private static RunFunctionResponse CreateResponse()
    {
        return new RunFunctionResponse { Desired = new State() };
    }

    private static V1ConfigMap CreateConfigMap(string name)
    {
        return new V1ConfigMap { Metadata = new() { Name = name } };
    }

    private static V1beta1ResourceGroup CreateResourceGroup(string name)
    {
        return new V1beta1ResourceGroup
        {
            Metadata = new() { Name = name },
            Spec = new() { ForProvider = new() }
        };
    }
}
