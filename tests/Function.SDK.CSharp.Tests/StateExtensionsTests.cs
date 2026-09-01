using System.Text.Json.Serialization;
using Apiextensions.Fn.Proto.V1;
using Google.Protobuf;
using k8s;
using k8s.Models;
using Shouldly;

namespace Function.SDK.CSharp.Example.Tests;

public class StateExtensionsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void AddOrUpdateRejectsInvalidKey(string? key)
    {
        var state = new State();

        var exception = Should.Throw<ArgumentException>(() => state.AddOrUpdate(key!, CreateResource()));

        exception.ParamName.ShouldBe("key");
    }

    [Fact]
    public void AddOrUpdateReplacesOmittedNestedFields()
    {
        var state = new State();
        state.AddOrUpdate("job", CreateResource(new Schedule { QuartzCronExpression = "0 0 13 * * ?" }));

        state.AddOrUpdate("job", CreateResource());

        var forProvider = state.Resources["job"].Resource_.Fields["spec"].StructValue.Fields["forProvider"].StructValue;
        forProvider.Fields.ShouldNotContainKey("schedule");
    }

    [Fact]
    public void AddOrUpdatePreservesOtherResourceEntries()
    {
        var state = new State();
        state.AddOrUpdate("a", CreateResource(new Schedule { QuartzCronExpression = "old" }));
        state.AddOrUpdate("b", CreateResource(new Schedule { QuartzCronExpression = "unchanged" }));
        var otherResource = state.Resources["b"].Clone();

        state.AddOrUpdate("a", CreateResource());

        state.Resources.Count.ShouldBe(2);
        state.Resources["b"].ShouldBe(otherResource);
    }

    [Fact]
    public void AddOrUpdatePreservesOuterProtocolState()
    {
        var state = new State();
        state.AddOrUpdate("job", CreateResource(new Schedule { QuartzCronExpression = "old" }));
        state.Resources["job"].Ready = Ready.True;
        state.Resources["job"].ConnectionDetails["endpoint"] = ByteString.CopyFromUtf8("example.test");

        state.AddOrUpdate("job", CreateResource());

        state.Resources["job"].Ready.ShouldBe(Ready.True);
        state.Resources["job"].ConnectionDetails["endpoint"].ToStringUtf8().ShouldBe("example.test");
    }

    [Fact]
    public void AddOrUpdateInitializesKubernetesIdentity()
    {
        var state = new State();
        var resource = CreateResource();
        resource.ApiVersion = string.Empty;
        resource.Kind = string.Empty;

        state.AddOrUpdate("job", resource);

        (resource.ApiVersion, resource.Kind).ShouldBe(("example.org/v1", "ScheduledResource"));
        state.Resources["job"].Resource_.Fields["apiVersion"].StringValue.ShouldBe("example.org/v1");
        state.Resources["job"].Resource_.Fields["kind"].StringValue.ShouldBe("ScheduledResource");
    }

    [Fact]
    public void AddOrUpdateAddsNewResource()
    {
        var state = new State();

        state.AddOrUpdate("job", CreateResource(new Schedule { QuartzCronExpression = "new" }));

        state.Resources.ShouldContainKey("job");
        state.Resources["job"].Resource_.Fields["spec"].StructValue.Fields["forProvider"].StructValue
            .Fields["schedule"].StructValue.Fields["quartzCronExpression"].StringValue.ShouldBe("new");
    }

    private static ScheduledResource CreateResource(Schedule? schedule = null) => new()
    {
        Metadata = new V1ObjectMeta { Name = "example" },
        Spec = new ScheduledResourceSpec
        {
            ForProvider = new ForProvider { Schedule = schedule }
        }
    };

    [KubernetesEntity(Group = "example.org", ApiVersion = "v1", Kind = "ScheduledResource", PluralName = "scheduledresources")]
    private sealed class ScheduledResource : IKubernetesObject<V1ObjectMeta>
    {
        [JsonPropertyName("apiVersion")]
        public string ApiVersion { get; set; } = "example.org/v1";

        [JsonPropertyName("kind")]
        public string Kind { get; set; } = "ScheduledResource";

        [JsonPropertyName("metadata")]
        public V1ObjectMeta Metadata { get; set; } = new();

        [JsonPropertyName("spec")]
        public ScheduledResourceSpec Spec { get; set; } = new();
    }

    private sealed class ScheduledResourceSpec
    {
        [JsonPropertyName("forProvider")]
        public ForProvider ForProvider { get; set; } = new();
    }

    private sealed class ForProvider
    {
        [JsonPropertyName("schedule")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Schedule? Schedule { get; set; }
    }

    private sealed class Schedule
    {
        [JsonPropertyName("quartzCronExpression")]
        public string QuartzCronExpression { get; set; } = string.Empty;
    }
}