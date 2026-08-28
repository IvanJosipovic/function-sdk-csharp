using Apiextensions.Fn.Proto.V1;
using Google.Protobuf.WellKnownTypes;

namespace Function.SDK.CSharp;

internal static class KubernetesHealthChecks
{
    private static readonly IReadOnlyDictionary<(string ApiVersion, string Kind), Func<Struct, bool>> Checks =
        new Dictionary<(string ApiVersion, string Kind), Func<Struct, bool>>
        {
            [("v1", "ConfigMap")] = AlwaysReady,
            [("v1", "Namespace")] = AlwaysReady,
            [("v1", "PersistentVolumeClaim")] = CheckPersistentVolumeClaim,
            [("v1", "Pod")] = CheckPod,
            [("v1", "Secret")] = AlwaysReady,
            [("v1", "Service")] = CheckService,
            [("v1", "ServiceAccount")] = AlwaysReady,
            [("apps/v1", "DaemonSet")] = CheckDaemonSet,
            [("apps/v1", "Deployment")] = CheckDeployment,
            [("apps/v1", "ReplicaSet")] = CheckReplicaSet,
            [("apps/v1", "StatefulSet")] = CheckStatefulSet,
            [("autoscaling/v2", "HorizontalPodAutoscaler")] = CheckHorizontalPodAutoscaler,
            [("batch/v1", "CronJob")] = CheckCronJob,
            [("batch/v1", "Job")] = CheckJob,
            [("networking.k8s.io/v1", "Ingress")] = CheckIngress
        };

    public static bool TryEvaluate(Resource resource, out bool ready)
    {
        ready = false;
        if (!TryGetString(resource.Resource_, out var apiVersion, "apiVersion")
            || !TryGetString(resource.Resource_, out var kind, "kind")
            || !Checks.TryGetValue((apiVersion, kind), out var check))
        {
            return false;
        }

        ready = check(resource.Resource_);
        return true;
    }

    private static bool AlwaysReady(Struct _) => true;

    private static bool CheckPod(Struct resource)
    {
        if (!TryGetString(resource, out var phase, "status", "phase"))
        {
            return false;
        }

        if (phase == "Succeeded")
        {
            return true;
        }

        return phase == "Running"
            && TryGetString(resource, out var restartPolicy, "spec", "restartPolicy")
            && restartPolicy == "Always"
            && HasTrueCondition(resource, "Ready");
    }

    private static bool CheckService(Struct resource)
    {
        var serviceType = TryGetString(resource, out var type, "spec", "type") ? type : "ClusterIP";
        return serviceType != "LoadBalancer"
            || TryGetList(resource, out var ingress, "status", "loadBalancer", "ingress") && ingress.Values.Count > 0;
    }

    private static bool CheckPersistentVolumeClaim(Struct resource)
    {
        return TryGetString(resource, out var phase, "status", "phase") && phase == "Bound";
    }

    private static bool CheckDeployment(Struct resource)
    {
        var replicas = GetReplicasOrDefault(resource);
        return TryGetInt64(resource, out var updatedReplicas, "status", "updatedReplicas")
            && TryGetInt64(resource, out var availableReplicas, "status", "availableReplicas")
            && replicas == updatedReplicas
            && replicas == availableReplicas
            && HasTrueCondition(resource, "Available");
    }

    private static bool CheckStatefulSet(Struct resource)
    {
        var replicas = GetReplicasOrDefault(resource);
        return TryGetInt64(resource, out var readyReplicas, "status", "readyReplicas")
            && TryGetInt64(resource, out var currentReplicas, "status", "currentReplicas")
            && replicas == readyReplicas
            && replicas == currentReplicas
            && TryGetString(resource, out var currentRevision, "status", "currentRevision")
            && TryGetString(resource, out var updateRevision, "status", "updateRevision")
            && currentRevision == updateRevision;
    }

    private static bool CheckDaemonSet(Struct resource)
    {
        return TryGetInt64(resource, out var desired, "status", "desiredNumberScheduled")
            && TryGetInt64(resource, out var ready, "status", "numberReady")
            && TryGetInt64(resource, out var updated, "status", "updatedNumberScheduled")
            && TryGetInt64(resource, out var available, "status", "numberAvailable")
            && desired == ready
            && desired == updated
            && desired == available;
    }

    private static bool CheckReplicaSet(Struct resource)
    {
        var replicas = GetReplicasOrDefault(resource);
        return TryGetInt64(resource, out var generation, "metadata", "generation")
            && TryGetInt64(resource, out var observedGeneration, "status", "observedGeneration")
            && observedGeneration >= generation
            && !HasTrueCondition(resource, "ReplicaFailure")
            && TryGetInt64(resource, out var availableReplicas, "status", "availableReplicas")
            && availableReplicas >= replicas;
    }

    private static bool CheckJob(Struct resource)
    {
        return !HasTrueCondition(resource, "Failed")
            && !HasTrueCondition(resource, "Suspended")
            && HasTrueCondition(resource, "Complete");
    }

    private static bool CheckCronJob(Struct resource)
    {
        if (TryGetBoolean(resource, out var suspended, "spec", "suspend") && suspended)
        {
            return true;
        }

        if (!TryGetDateTime(resource, out var lastScheduleTime, "status", "lastScheduleTime"))
        {
            return false;
        }

        if (TryGetList(resource, out var active, "status", "active") && active.Values.Count > 0)
        {
            return true;
        }

        return TryGetDateTime(resource, out var lastSuccessfulTime, "status", "lastSuccessfulTime")
            && lastSuccessfulTime >= lastScheduleTime;
    }

    private static bool CheckHorizontalPodAutoscaler(Struct resource)
    {
        if (HasTrueCondition(resource, "FailedGetScale")
            || HasTrueCondition(resource, "FailedUpdateScale")
            || HasTrueCondition(resource, "FailedGetResourceMetric")
            || HasTrueCondition(resource, "InvalidSelector"))
        {
            return false;
        }

        return HasTrueCondition(resource, "ScalingActive") || HasTrueCondition(resource, "ScalingLimited");
    }

    private static bool CheckIngress(Struct resource)
    {
        return TryGetList(resource, out var ingress, "status", "loadBalancer", "ingress")
            && ingress.Values.Count > 0;
    }

    private static long GetReplicasOrDefault(Struct resource)
    {
        return TryGetInt64(resource, out var replicas, "spec", "replicas") ? replicas : 1;
    }

    private static bool HasTrueCondition(Struct resource, string conditionType)
    {
        if (!TryGetList(resource, out var conditions, "status", "conditions"))
        {
            return false;
        }

        foreach (var condition in conditions.Values)
        {
            if (condition.KindCase != Value.KindOneofCase.StructValue)
            {
                continue;
            }

            var conditionValue = condition.StructValue;
            if (TryGetString(conditionValue, out var type, "type")
                && type == conditionType
                && TryGetString(conditionValue, out var status, "status")
                && status == "True")
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryGetString(Struct resource, out string value, params string[] path)
    {
        value = string.Empty;
        if (!TryGetValue(resource, out var field, path) || field.KindCase != Value.KindOneofCase.StringValue)
        {
            return false;
        }

        value = field.StringValue;
        return true;
    }

    private static bool TryGetInt64(Struct resource, out long value, params string[] path)
    {
        value = 0;
        if (!TryGetValue(resource, out var field, path)
            || field.KindCase != Value.KindOneofCase.NumberValue
            || field.NumberValue < long.MinValue
            || field.NumberValue > long.MaxValue
            || Math.Truncate(field.NumberValue) != field.NumberValue)
        {
            return false;
        }

        value = (long)field.NumberValue;
        return true;
    }

    private static bool TryGetBoolean(Struct resource, out bool value, params string[] path)
    {
        value = false;
        if (!TryGetValue(resource, out var field, path) || field.KindCase != Value.KindOneofCase.BoolValue)
        {
            return false;
        }

        value = field.BoolValue;
        return true;
    }

    private static bool TryGetDateTime(Struct resource, out DateTimeOffset value, params string[] path)
    {
        value = default;
        return TryGetString(resource, out var text, path)
            && DateTimeOffset.TryParse(
                text,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal,
                out value);
    }

    private static bool TryGetList(Struct resource, out ListValue value, params string[] path)
    {
        value = new ListValue();
        if (!TryGetValue(resource, out var field, path) || field.KindCase != Value.KindOneofCase.ListValue)
        {
            return false;
        }

        value = field.ListValue;
        return true;
    }

    private static bool TryGetValue(Struct resource, out Value value, params string[] path)
    {
        value = null!;
        var current = resource;
        for (var index = 0; index < path.Length; index++)
        {
            if (!current.Fields.TryGetValue(path[index], out value))
            {
                return false;
            }

            if (index == path.Length - 1)
            {
                return true;
            }

            if (value.KindCase != Value.KindOneofCase.StructValue)
            {
                return false;
            }

            current = value.StructValue;
        }

        return false;
    }
}
