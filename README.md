# function-sdk-csharp

[![codecov](https://codecov.io/gh/IvanJosipovic/function-sdk-csharp/graph/badge.svg?token=Xzi1otVyUo)](https://codecov.io/gh/IvanJosipovic/function-sdk-csharp)
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Function.SDK.CSharp.svg?style=flat-square)](https://www.nuget.org/packages?q=Function.SDK.CSharp)
[![Nuget (with prereleases)](https://img.shields.io/nuget/dt/Function.SDK.CSharp.svg?style=flat-square)](https://www.nuget.org/packages?q=Function.SDK.CSharp)

The C# SDK for writing [composition functions](https://docs.crossplane.io/latest/composition/compositions/).

Working example, https://github.com/IvanJosipovic/function-kubemodelrepo

## C# Template

[Template Repository](https://github.com/IvanJosipovic/function-template-csharp)

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
