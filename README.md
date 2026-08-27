# function-sdk-csharp

[![codecov](https://codecov.io/gh/IvanJosipovic/function-sdk-csharp/graph/badge.svg?token=Xzi1otVyUo)](https://codecov.io/gh/IvanJosipovic/function-sdk-csharp)
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Function.SDK.CSharp.svg?style=flat-square)](https://www.nuget.org/packages?q=Function.SDK.CSharp)
[![Nuget (with prereleases)](https://img.shields.io/nuget/dt/Function.SDK.CSharp.svg?style=flat-square)](https://www.nuget.org/packages?q=Function.SDK.CSharp)

The C# SDK for writing [composition functions](https://docs.crossplane.io/latest/composition/compositions/).

Working example, https://github.com/IvanJosipovic/function-kubemodelrepo

## C# Template

[Template Repository](https://github.com/IvanJosipovic/function-template-csharp)

[Download .Net 10 SDK](https://dotnet.microsoft.com/en-us/download)

```shell
dotnet new install function-template-csharp

dotnet new function-csharp -n TheFunction -o c:\repos\func
```

## Features

- XRD to Model Generation
  - Modify the xrd.yaml and models will be automatically generated
- CRD to Model Generation
  - Add crd.yaml(s) to the project and models will be automatically generated
  - Most Crossplane Providers already published [KubernetesCRDModelGen.Models](https://github.com/IvanJosipovic/KubernetesCRDModelGen.Models#generated-packages)

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

- Supports Crossplane v2 or greater

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

| Extension | Description |
| --- | --- |
| `Fatal(message)` | Adds a fatal result to the response. |
| `Warning(message)` | Adds a warning result to the response. |
| `Normal(message)` | Adds a normal result to the response. |
| `NormalF(message, args)` | Adds a formatted normal result to the response. |
| `SetOutput(output)` | Sets operation output from a `Dictionary<string, object>` or protobuf `Struct`. |
| `RequireResources(...)` | Requests resources by name or labels for the next function invocation. |
| `UpdateDesiredReadyStatus(...)` | Updates desired readiness from observed `Ready` and `Synced` conditions. Types passed through `ignoreNoReadyCondition` are treated as ready when both conditions are absent. |
| `AddDesiredResource(resource, key)` | Adds or merges a desired Kubernetes resource using a canonical key. The optional key is used when `metadata.name` is absent. |
| `AddDesiredUsage(by, of, replayDeletion)` | Adds a Crossplane `Usage` that protects one desired resource while another uses it. |
| `GetDesiredResource<T>(key)` | Gets a desired Kubernetes resource using its canonical API version, kind, and key. |
| `GetDesiredResources<T>()` | Enumerates desired resources matching the API version and kind of `T`. |
| `ValidateKubeResourceNames()` | Validates desired `metadata.name` values as RFC 1123 DNS labels. |

Canonical resource keys use the following format:

```text
{apiVersion}/{kind}/{key}
```

Namespaced resources include their namespace:

```text
{apiVersion}/{kind}/{namespace}/{key}
```

For grouped resources, `apiVersion` includes the group, for example `apps/v1/Deployment/default/example`. Core resources use keys such as `v1/ConfigMap/default/settings`. Cluster-scoped resources continue to use `{apiVersion}/{kind}/{key}`.

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
    [typeof(V1Secret), typeof(V1ConfigMap)]);

response.ValidateKubeResourceNames();
```

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
