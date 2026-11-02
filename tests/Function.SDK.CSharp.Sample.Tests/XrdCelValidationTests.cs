using Function.SDK.CSharp.SourceGenerator.Models.platform.example.com;
using Shouldly;

namespace Function.SDK.CSharp.Sample.Tests;

public class XrdCelValidationTests
{
    private static readonly Lazy<XrdCelValidation> Validator = new(() =>
        new XrdCelValidation(Path.Combine(AppContext.BaseDirectory, "xrd.yaml")));

    [Fact]
    public void XrdCelRulesCompile()
    {
        Validator.Value.RuleCount.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void PrivateBucketPassesAllCelRules()
    {
        AssertValidXr(CreateXr(V1alpha1XStorageBucketSpecParametersAclEnum.Private, isPublic: false));
    }

    [Fact]
    public void PublicBlobBucketPassesAllCelRules()
    {
        AssertValidXr(CreateXr(V1alpha1XStorageBucketSpecParametersAclEnum.Blob, isPublic: true));
    }

    [Fact]
    public void PublicContainerBucketPassesAllCelRules()
    {
        AssertValidXr(CreateXr(V1alpha1XStorageBucketSpecParametersAclEnum.Container, isPublic: true));
    }

    [Fact]
    public void PrivateNetworkBlobBucketReportsCelFailure()
    {
        AssertInvalidXr(CreateXr(V1alpha1XStorageBucketSpecParametersAclEnum.Blob, isPublic: false));
    }

    [Fact]
    public void PrivateNetworkContainerBucketReportsCelFailure()
    {
        AssertInvalidXr(CreateXr(V1alpha1XStorageBucketSpecParametersAclEnum.Container, isPublic: false));
    }

    private static void AssertValidXr(V1alpha1XStorageBucket xr)
    {
        Validator.Value.Validate(xr).ShouldBeEmpty();
    }

    private static void AssertInvalidXr(V1alpha1XStorageBucket xr)
    {
        var failures = Validator.Value.Validate(xr);

        failures.ShouldNotBeEmpty();
        failures.All(failure =>
                !string.IsNullOrWhiteSpace(failure.Rule) &&
                !string.IsNullOrWhiteSpace(failure.Message) &&
                !string.IsNullOrWhiteSpace(failure.InstancePath))
            .ShouldBeTrue();
    }

    private static V1alpha1XStorageBucket CreateXr(
        V1alpha1XStorageBucketSpecParametersAclEnum acl,
        bool isPublic)
    {
        return new V1alpha1XStorageBucket
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
                    Acl = acl,
                    Location = V1alpha1XStorageBucketSpecParametersLocationEnum.Eastus,
                    Versioning = true,
                    Public = isPublic
                }
            }
        };
    }
}
