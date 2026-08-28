using Apiextensions.Fn.Proto.V1;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace Function.SDK.CSharp.Example.Tests;

public class KubernetesHealthChecksTests
{
    public static TheoryData<IKubernetesObject, bool> HealthCases => new()
    {
        { new V1ConfigMap(), true },
        { new V1Namespace(), true },
        { new V1Secret(), true },
        { new V1ServiceAccount(), true },
        { new V1PersistentVolumeClaim { Status = new() { Phase = "Bound" } }, true },
        { new V1PersistentVolumeClaim { Status = new() { Phase = "Pending" } }, false },
        { Pod("Running", "Always", "True"), true },
        { Pod("Running", "Always", "False"), false },
        { Pod("Running", "Never"), false },
        { Pod("Pending"), false },
        { Pod("Succeeded"), true },
        { new V1Service { Spec = new() }, true },
        { new V1Service { Spec = new() { Type = "ClusterIP" } }, true },
        { new V1Service { Spec = new() { Type = "NodePort" } }, true },
        {
            new V1Service
            {
                Spec = new() { Type = "LoadBalancer" },
                Status = new() { LoadBalancer = new() { Ingress = [new() { Ip = "192.0.2.1" }] } }
            },
            true
        },
        { new V1Service { Spec = new() { Type = "LoadBalancer" }, Status = new() { LoadBalancer = new() } }, false },
        { Deployment(null, 1, 1, "True"), true },
        { Deployment(2, 2, 2, "True"), true },
        { Deployment(2, 2, 1, "True"), false },
        { new V1Deployment { Spec = new() { Replicas = 1 }, Status = new() }, false },
        { StatefulSet("rev-2", "rev-2"), true },
        { StatefulSet("rev-1", "rev-2"), false },
        { DaemonSet(3, 3, 3, 3), true },
        { DaemonSet(3, 2, 3, 2), false },
        { ReplicaSet(2, 2, 2), true },
        { ReplicaSet(2, 1, 2), false },
        { ReplicaSet(2, 2, 2, "True"), false },
        { Job(("Complete", "True")), true },
        { Job(("Complete", "True"), ("Failed", "True")), false },
        { Job(("Suspended", "True")), false },
        { new V1CronJob { Spec = new() { Suspend = true } }, true },
        {
            new V1CronJob
            {
                Status = new()
                {
                    LastScheduleTime = ScheduledAt,
                    Active = [new V1ObjectReference { Name = "job-1" }]
                }
            },
            true
        },
        { CronJob(ScheduledAt, ScheduledAt), true },
        { CronJob(ScheduledAt.AddMinutes(5), ScheduledAt), false },
        { HorizontalPodAutoscaler(("ScalingActive", "True")), true },
        { HorizontalPodAutoscaler(("ScalingLimited", "True")), true },
        { HorizontalPodAutoscaler(("ScalingActive", "True"), ("FailedGetScale", "True")), false },
        {
            new V1Ingress
            {
                Status = new() { LoadBalancer = new() { Ingress = [new() { Hostname = "example.test" }] } }
            },
            true
        },
        { new V1Ingress { Status = new() { LoadBalancer = new() } }, false }
    };

    [Theory]
    [MemberData(nameof(HealthCases))]
    public void UpdateDesiredReadyStatusEvaluatesStandardKubernetesResources(
        IKubernetesObject resource,
        bool expectedReady)
    {
        var response = Evaluate(resource);

        response.Desired.Resources["resource"].Ready.ShouldBe(expectedReady ? Ready.True : Ready.False);
    }

    [Fact]
    public void UpdateDesiredReadyStatusMarksRegressedKubernetesResourceNotReady()
    {
        var firstResponse = Evaluate(Deployment(1, 1, 1, "True"));
        firstResponse.Desired.Resources["resource"].Ready.ShouldBe(Ready.True);

        var request = TestExtensions.GetFunctionRequest();
        request.Desired.MergeFrom(firstResponse.Desired);
        request.Observed.AddOrUpdate("resource", Deployment(1, 1, 0, "False"));
        var secondResponse = request.To();
        secondResponse.UpdateDesiredReadyStatus(request, NullLogger.Instance);

        secondResponse.Desired.Resources["resource"].Ready.ShouldBe(Ready.False);
    }

    [Fact]
    public void UpdateDesiredReadyStatusGivesSyncedFailurePrecedence()
    {
        var deployment = new V1Deployment
        {
            Spec = new() { Replicas = 1 },
            Status = new()
            {
                UpdatedReplicas = 1,
                AvailableReplicas = 1,
                Conditions =
                [
                    new() { Type = "Available", Status = "True" },
                    new() { Type = "Synced", Status = "False" }
                ]
            }
        };

        Evaluate(deployment).Desired.Resources["resource"].Ready.ShouldBe(Ready.False);
    }

    [Fact]
    public void UpdateDesiredReadyStatusReportsFailedJobAsFatal()
    {
        var response = Evaluate(Job(("Failed", "True")));

        response.Desired.Resources["resource"].Ready.ShouldBe(Ready.False);
        response.Results.Count.ShouldBe(1);
        response.Results[0].Severity.ShouldBe(Severity.Fatal);
        response.Results[0].Message.ShouldContain("Job failed");
    }

    private static readonly DateTime ScheduledAt = new(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);

    private static RunFunctionResponse Evaluate(IKubernetesObject resource)
    {
        var request = TestExtensions.GetFunctionRequest();
        request.Desired.AddOrUpdate("resource", resource);
        request.Observed.AddOrUpdate("resource", resource);

        var response = request.To();
        response.UpdateDesiredReadyStatus(request, NullLogger.Instance);
        return response;
    }

    private static V1Pod Pod(string phase, string? restartPolicy = null, string? ready = null)
    {
        return new()
        {
            Spec = new() { RestartPolicy = restartPolicy },
            Status = new()
            {
                Phase = phase,
                Conditions = ready == null ? null : [new() { Type = "Ready", Status = ready }]
            }
        };
    }

    private static V1Deployment Deployment(int? replicas, int updated, int available, string conditionStatus)
    {
        return new()
        {
            Spec = new() { Replicas = replicas },
            Status = new()
            {
                UpdatedReplicas = updated,
                AvailableReplicas = available,
                Conditions = [new() { Type = "Available", Status = conditionStatus }]
            }
        };
    }

    private static V1StatefulSet StatefulSet(string currentRevision, string updateRevision)
    {
        return new()
        {
            Spec = new() { Replicas = 2 },
            Status = new()
            {
                ReadyReplicas = 2,
                CurrentReplicas = 2,
                CurrentRevision = currentRevision,
                UpdateRevision = updateRevision
            }
        };
    }

    private static V1DaemonSet DaemonSet(int desired, int ready, int updated, int available)
    {
        return new()
        {
            Status = new()
            {
                DesiredNumberScheduled = desired,
                NumberReady = ready,
                UpdatedNumberScheduled = updated,
                NumberAvailable = available
            }
        };
    }

    private static V1ReplicaSet ReplicaSet(
        long generation,
        long observedGeneration,
        int available,
        string? failureStatus = null)
    {
        return new()
        {
            Metadata = new() { Generation = generation },
            Spec = new() { Replicas = 2 },
            Status = new()
            {
                ObservedGeneration = observedGeneration,
                AvailableReplicas = available,
                Conditions = failureStatus == null
                    ? null
                    : [new() { Type = "ReplicaFailure", Status = failureStatus }]
            }
        };
    }

    private static V1Job Job(params (string Type, string Status)[] conditions)
    {
        return new()
        {
            Status = new()
            {
                Conditions = conditions.Select(static condition =>
                    new V1JobCondition { Type = condition.Type, Status = condition.Status }).ToList()
            }
        };
    }

    private static V1CronJob CronJob(DateTime lastScheduleTime, DateTime lastSuccessfulTime)
    {
        return new()
        {
            Status = new()
            {
                LastScheduleTime = lastScheduleTime,
                LastSuccessfulTime = lastSuccessfulTime
            }
        };
    }

    private static V2HorizontalPodAutoscaler HorizontalPodAutoscaler(
        params (string Type, string Status)[] conditions)
    {
        return new()
        {
            Status = new()
            {
                Conditions = conditions.Select(static condition =>
                    new V2HorizontalPodAutoscalerCondition
                    {
                        Type = condition.Type,
                        Status = condition.Status
                    }).ToList()
            }
        };
    }
}
