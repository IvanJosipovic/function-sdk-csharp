# function-sdk-csharp

[![codecov](https://codecov.io/gh/IvanJosipovic/function-sdk-csharp/graph/badge.svg?token=Xzi1otVyUo)](https://codecov.io/gh/IvanJosipovic/function-sdk-csharp)
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Function.SDK.CSharp.svg?style=flat-square)](https://www.nuget.org/packages?q=Function.SDK.CSharp)
[![Nuget (with prereleases)](https://img.shields.io/nuget/dt/Function.SDK.CSharp.svg?style=flat-square)](https://www.nuget.org/packages?q=Function.SDK.CSharp)

The C# SDK for writing [composition functions](https://docs.crossplane.io/latest/composition/compositions/).

Working example, https://github.com/IvanJosipovic/function-kubemodelrepo

## Getting started

### C# Template

[Template Repository](https://github.com/IvanJosipovic/function-template-csharp)

[Download .Net 10 SDK](https://dotnet.microsoft.com/en-us/download)

```shell
dotnet new install function-template-csharp

dotnet new function-csharp -n TheFunction -o c:\repos\func
```

## Features

### Code generation

- **XRD to model generation**
  - Modify `xrd.yaml` and models are generated automatically.
- **CRD to model generation**
  - Add one or more `crd.yaml` files to the project and models are generated
    automatically.
  - Most Crossplane providers already publish
    [KubernetesCRDModelGen.Models](https://github.com/IvanJosipovic/KubernetesCRDModelGen.Models#generated-packages).

    | Group | NuGet |
    | --- | --- |
    | aws.upbound.io | [Link](https://www.nuget.org/packages/KubernetesCRDModelGen.Models.aws.upbound.io/) |
    | azapi.upbound.io | [Link](https://www.nuget.org/packages/KubernetesCRDModelGen.Models.azapi.upbound.io/) |
    | azure.upbound.io | [Link](https://www.nuget.org/packages/KubernetesCRDModelGen.Models.azure.upbound.io/) |
    | azuread.upbound.io | [Link](https://www.nuget.org/packages/KubernetesCRDModelGen.Models.azuread.upbound.io/) |
    | crossplane.io | [Link](https://www.nuget.org/packages/KubernetesCRDModelGen.Models.crossplane.io/) |
    | databricks.crossplane.io | [Link](https://www.nuget.org/packages/KubernetesCRDModelGen.Models.databricks.crossplane.io/) |
    | gcp.upbound.io | [Link](https://www.nuget.org/packages/KubernetesCRDModelGen.Models.gcp.upbound.io/) |
    | helm.crossplane.io | [Link](https://www.nuget.org/packages/KubernetesCRDModelGen.Models.helm.crossplane.io/) |
    | kubernetes.crossplane.io | [Link](https://www.nuget.org/packages/KubernetesCRDModelGen.Models.kubernetes.crossplane.io/) |
    | opentofu.upbound.io | [Link](https://www.nuget.org/packages/KubernetesCRDModelGen.Models.opentofu.upbound.io/) |
    | tf.upbound.io | [Link](https://www.nuget.org/packages/KubernetesCRDModelGen.Models.tf.upbound.io/) |
    | upbound.io | [Link](https://www.nuget.org/packages/KubernetesCRDModelGen.Models.upbound.io/) |
    | vault.upbound.io | [Link](https://www.nuget.org/packages/KubernetesCRDModelGen.Models.vault.upbound.io/) |

### Compatibility

- Supports Crossplane v2 or greater.

## Extensions

All extensions are available from the `Function.SDK.CSharp` namespace.

### Application setup

| Extension | Description |
| --- | --- |
| `ConfigureFunction(WebApplicationBuilder, string[])` | Configures the gRPC function host, HTTP/2, TLS, logging, and reflection endpoints. |
| `MapFunctionService<TService>(WebApplication)` | Maps a `FunctionRunnerServiceBase` implementation and gRPC reflection service. |

```csharp
var builder = WebApplication.CreateSlimBuilder(args);
builder.ConfigureFunction(args);

var app = builder.Build();
app.MapFunctionService<RunFunctionService>();

await app.RunAsync();
```

### Request extensions

| Extension | Description |
| --- | --- |
| `To()` | Creates a response from a request using the default one-minute TTL and copies desired state and context. |
| `To(Duration)` | Creates a response using a custom TTL. |
| `GetObservedCompositeResource<T>()` | Deserializes the observed composite resource as `T`. |
| `GetObservedResources()` | Returns the raw observed resource dictionary. |
| `GetObservedResource<T>(key)` | Gets an observed Kubernetes resource using its canonical API version, kind, and key. |
| `GetObservedResources<T>()` | Enumerates observed resources matching the API version and kind of `T`. |
| `GetDesiredResources()` | Returns the raw desired resource dictionary from the request. |
| `GetDesiredResource<T>(key)` | Deserializes a desired request resource using its existing dictionary key. |
| `GetRequiredResource<T>(key)` | Deserializes all required resources registered under a key. |

```csharp
var response = request.To();
var composite = request.GetObservedCompositeResource<V1alpha1Example>();
var observed = request.GetObservedResource<V1ConfigMap>("settings");
var configMaps = request.GetObservedResources<V1ConfigMap>();
```

### Response extensions

#### Results and events

| Extension | Description |
| --- | --- |
| `Fatal(message)` | Adds a fatal result to the response. |
| `Warning(message)` | Adds a warning result to the response. |
| `Normal(message)` | Adds a normal result to the response. |
| `NormalF(message, args)` | Adds a formatted normal result to the response. |
| `SetOutput(output)` | Sets operation output from a `Dictionary<string, object>` or protobuf `Struct`. |

#### Resource requirements

| Extension | Description |
| --- | --- |
| `RequireResources(...)` | Requests resources by name or labels for the next function invocation. |

#### Desired resources

| Extension | Description |
| --- | --- |
| `AddDesiredResource(resource, key)` | Adds or merges a desired Kubernetes resource using a canonical key. The optional key is used when `metadata.name` is absent. |
| `AddDesiredUsage(by, of, replayDeletion)` | Adds a Crossplane `Usage` that protects one desired resource while another uses it. |
| `GetDesiredResource<T>(key)` | Gets a desired Kubernetes resource using its canonical API version, kind, and key. |
| `GetDesiredResources<T>()` | Enumerates desired resources matching the API version and kind of `T`. |

##### Canonical resource keys

Canonical resource keys use the following format:

```text
{apiVersion}/{kind}/{key}
```

Namespaced resources include their namespace:

```text
{apiVersion}/{kind}/{namespace}/{key}
```

For grouped resources, `apiVersion` includes the group, for example `apps/v1/Deployment/default/example`. Core resources use keys such as `v1/ConfigMap/default/settings`. Cluster-scoped resources continue to use `{apiVersion}/{kind}/{key}`.

#### Readiness and validation

| Extension | Description |
| --- | --- |
| `UpdateDesiredReadyStatus(...)` | Updates desired readiness from custom typed predicates, standard Kubernetes health, or observed `Ready` and `Synced` conditions. Unhealthy resources are explicitly marked not ready on every invocation. |
| `ValidateKubeResourceNames()` | Validates desired `metadata.name` values as RFC 1123 DNS labels. |

##### `UpdateDesiredReadyStatus`

`UpdateDesiredReadyStatus` provides the readiness behavior of Crossplane's
`function-auto-ready` function directly in the SDK. It evaluates observed
composed resources and updates their desired `Ready` fields, so a separate
`function-auto-ready` pipeline step is not required when this method is used.

```csharp
response.AddDesiredResource(new V1ConfigMap
{
    Metadata = new V1ObjectMeta { Name = "settings" },
    Data = new Dictionary<string, string> { ["environment"] = "production" }
});

var configMap = response.GetDesiredResource<V1ConfigMap>("settings");

response.UpdateDesiredReadyStatus(
    request,
    logger,
    [
        ResourceReadinessCheck.For<MyCustomResource>(
            resource => resource.Status?.Phase == "Available",
            resource => resource.Status?.Healthy == true)
    ]);

response.ValidateKubeResourceNames();
```

##### Standard Kubernetes resources

`UpdateDesiredReadyStatus` automatically evaluates these standard Kubernetes
resources using their native status fields:

###### `v1`

- `ConfigMap`, `Namespace`, `Secret`, `ServiceAccount`
  - Always ready.
- `PersistentVolumeClaim`
  - Ready when `status.phase` is `Bound`.
- `Pod`
  - Ready when `status.phase` is `Succeeded`, or when it is `Running` with
    `spec.restartPolicy: Always` and `Ready=True`.
- `Service`
  - Ready unless it is a `LoadBalancer` without a load balancer ingress.

###### `apps/v1`

- `Deployment`
  - Ready when updated and available replicas match the desired replica count
    and `Available=True`.
- `StatefulSet`
  - Ready when ready and current replicas match the desired count and the
    current and update revisions match.
- `DaemonSet`
  - Ready when desired replicas match ready, updated, and available replicas.
- `ReplicaSet`
  - Ready when the observed generation is current, there is no
    `ReplicaFailure=True`, and available replicas meet the desired count.

###### `batch/v1`

- `CronJob`
  - Ready when suspended, has an active Job, or has completed a schedule
    successfully.
- `Job`
  - Ready when `Complete=True` and it is not suspended or failed.
  - A `Failed=True` condition marks the Job not ready and reports a fatal
    result, so the composite does not remain in a processing state after a
    terminal Job failure.

###### `autoscaling/v2`

- `HorizontalPodAutoscaler`
  - Ready when scaling is active or limited, unless one of its scale or metric
    retrieval conditions reports failure.

###### `networking.k8s.io/v1`

- `Ingress`
  - Ready when at least one load balancer ingress is present.

For a custom resource type, pass an array of `ResourceReadinessCheck` instances
created with `ResourceReadinessCheck.For<T>(...)`. Checks apply only to observed
resources with the exact API version and kind of `T`, and all matching checks
and predicates must return `true` for the resource to be ready.
`Synced=False` always takes precedence. Other resource types continue to use the
Crossplane-style `Ready` condition.

### State and resource extensions

| Extension | Description |
| --- | --- |
| `AddOrUpdate(key, resource)` | Adds a Kubernetes object to a `State`, initializes missing API identity, and merges an existing entry using protobuf merge semantics. |
| `GetKubeResource<T>()` | Deserializes a protobuf function `Resource` as a Kubernetes object. |
| `GetCondition(conditionType)` | Gets a condition from a resource status by condition type. |

```csharp
response.Desired.AddOrUpdate("settings", configMap);

var resource = response.Desired.Resources["settings"];
var typedResource = resource.GetKubeResource<V1ConfigMap>();
var readyCondition = resource.GetCondition("Ready");
```
