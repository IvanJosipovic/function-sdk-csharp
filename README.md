# function-sdk-csharp

[![codecov](https://codecov.io/gh/IvanJosipovic/function-sdk-csharp/graph/badge.svg?token=Xzi1otVyUo)](https://codecov.io/gh/IvanJosipovic/function-sdk-csharp)
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Function.SDK.CSharp.svg?style=flat-square)](https://www.nuget.org/packages?q=Function.SDK.CSharp)
[![Nuget (with prereleases)](https://img.shields.io/nuget/dt/Function.SDK.CSharp.svg?style=flat-square)](https://www.nuget.org/packages?q=Function.SDK.CSharp)

The C# SDK for writing [composition functions](https://docs.crossplane.io/latest/composition/compositions/).

Working example, https://github.com/IvanJosipovic/function-kubemodelrepo

## Features

- XRD to Model Generation
  - Modify the xrd.yaml and models will be automatically generated
- CRD to Model Generation
  - Add crd.yaml(s) to the project and models will be automatically generated
  - Most Crossplane Providers already published [KubernetesCRDModelGen](https://github.com/IvanJosipovic/KubernetesCRDModelGen?tab=readme-ov-file#published-packages)

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

- Supports Crossplane v1.17 or greater

## How to Test

You can run your function locally and test it using `crossplane render`
with the example manifests.

### Download Crank and rename to Crossplane

https://releases.crossplane.io/stable/current/bin

## Run Function In IDE

Download the lastest [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)

```shell
dotnet debug
```

## Run Function In Docker

```shell
docker build -t function-sdk-csharp-example -f src/Function.SDK.CSharp.Example/Dockerfile src
docker run -it -p 9443:9443 function-sdk-csharp-example
```

## Run Test

Then, in another terminal, call it with these example manifests

```shell
crossplane render example/xr.yaml example/composition.yaml example/functions.yaml
```

```yaml
---
apiVersion: platform.example.com/v1alpha1
kind: XStorageBucket
metadata:
  name: example
status:
  conditions:
  - lastTransitionTime: "2024-01-01T00:00:00Z"
    message: 'Unready resources: account, container, and rg'
    reason: Creating
    status: "False"
    type: Ready
---
apiVersion: storage.azure.upbound.io/v1beta1
kind: Account
metadata:
  annotations:
    crossplane.io/composition-resource-name: account
  labels:
    crossplane.io/composite: example
  name: example
  ownerReferences:
  - apiVersion: platform.example.com/v1alpha1
    blockOwnerDeletion: true
    controller: true
    kind: XStorageBucket
    name: example
    uid: ""
spec:
  forProvider:
    accountReplicationType: LRS
    accountTier: Standard
    blobProperties:
    - versioningEnabled: true
    infrastructureEncryptionEnabled: true
    location: eastus
    resourceGroupNameSelector:
      matchLabels:
        matchControllerRef: "True"
---
apiVersion: storage.azure.upbound.io/v1beta1
kind: Container
metadata:
  annotations:
    crossplane.io/composition-resource-name: container
  generateName: example-
  labels:
    crossplane.io/composite: example
  ownerReferences:
  - apiVersion: platform.example.com/v1alpha1
    blockOwnerDeletion: true
    controller: true
    kind: XStorageBucket
    name: example
    uid: ""
spec:
  forProvider:
    containerAccessType: public
    storageAccountNameSelector:
      matchLabels:
        matchControllerRef: "True"
---
apiVersion: azure.upbound.io/v1beta1
kind: ResourceGroup
metadata:
  annotations:
    crossplane.io/composition-resource-name: rg
  generateName: example-
  labels:
    crossplane.io/composite: example
  ownerReferences:
  - apiVersion: platform.example.com/v1alpha1
    blockOwnerDeletion: true
    controller: true
    kind: XStorageBucket
    name: example
    uid: ""
spec:
  forProvider:
    location: eastus
```
