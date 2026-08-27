using System.Text.Json;
using Apiextensions.Fn.Proto.V1;
using Google.Protobuf.WellKnownTypes;
using k8s;
using k8s.Models;
using KubernetesCRDModelGen.Models.protection.crossplane.io;

namespace Function.SDK.CSharp;

/// <summary>
/// Package response contains utilities for working with RunFunctionResponses.
/// </summary>
public static partial class ResponseExtensions
{
    /// <summary>
    /// Fatal adds a fatal result to the supplied RunFunctionResponse.
    /// An event will be created for the Composite Resource.
    /// A fatal result cannot target the claim.
    /// </summary>
    /// <param name="response"></param>
    /// <param name="message"></param>
    public static void Fatal(this RunFunctionResponse response, string message)
    {
        response.Results.Add(new Result
        {
            Severity = Severity.Fatal,
            Message = message
        });
    }

    /// <summary>
    /// Warning adds a warning result to the supplied RunFunctionResponse.
    /// An event will be created for the Composite Resource.
    /// </summary>
    /// <param name="response"></param>
    /// <param name="message"></param>
    public static void Warning(this RunFunctionResponse response, string message)
    {
        response.Results.Add(new Result
        {
            Severity = Severity.Warning,
            Message = message
        });
    }

    /// <summary>
    /// Normal adds a normal result to the supplied RunFunctionResponse.
    /// An event will be created for the Composite Resource.
    /// </summary>
    /// <param name="response"></param>
    /// <param name="message"></param>
    public static void Normal(this RunFunctionResponse response, string message)
    {
        response.Results.Add(new Result
        {
            Severity = Severity.Normal,
            Message = message
        });
    }

    /// <summary>
    /// NormalF adds a normal result to the supplied RunFunctionResponse.
    /// An event will be created for the Composite Resource.
    /// </summary>
    /// <param name="response"></param>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void NormalF(this RunFunctionResponse response, string message, params string[] args)
    {
        response.Results.Add(new Result
        {
            Severity = Severity.Normal,
            Message = string.Format(message, args)
        });
    }

    /// <summary>
    /// Set the output field in a RunFunctionResponse for operation functions.
    /// Operation functions can return arbitrary output data that will be written
    /// to the Operation's status.pipeline field. This function sets that output
    /// on the response.
    /// </summary>
    /// <param name="response">The RunFunctionResponse to update.</param>
    /// <param name="output">The output data as a Dictionary or protobuf Struct.</param>
    /// <exception cref="TypeAccessException">Thrown if the output type is not supported.</exception>
    public static void SetOutput(this RunFunctionResponse response, object output)
    {
        response.Output = output switch
        {
            Dictionary<string, object> dict => Struct.Parser.ParseJson(JsonSerializer.Serialize(dict)),
            Struct s => s,
            _ => throw new TypeAccessException($"Unsupported output type: {output?.GetType()}"),
        };
    }

    /// <summary>
    /// Add a resource requirement to the response.
    /// This tells Crossplane to fetch the specified resources and include them
    /// in the next call to the function in req.required_resources[name].
    /// </summary>
    /// <param name="rsp">The RunFunctionResponse to update.</param>
    /// <param name="name">The name to use for this requirement.</param>
    /// <param name="apiVersion">The API version of resources to require.</param>
    /// <param name="kind">The kind of resources to require.</param>
    /// <param name="matchName">Match a resource by name (mutually exclusive with matchLabels).</param>
    /// <param name="matchLabels">Match resources by labels (mutually exclusive with matchName).</param>
    /// <param name="namespace">The namespace to search in (optional).</param>
    /// <exception cref="ArgumentException">Thrown if both matchName and matchLabels are provided, or neither.</exception>
    public static void RequireResources(
        this RunFunctionResponse rsp,
        string name,
        string apiVersion,
        string kind,
        string? matchName = null,
        Dictionary<string, string>? matchLabels = null,
        string? @namespace = null)
    {
        if (matchName == null == (matchLabels == null))
        {
            throw new ArgumentException("Exactly one of matchName or matchLabels must be provided");
        }

        var selector = new ResourceSelector
        {
            ApiVersion = apiVersion,
            Kind = kind
        };

        if (matchName != null)
        {
            selector.MatchName = matchName;
        }

        if (matchLabels != null)
        {
            selector.MatchLabels = new MatchLabels();
            foreach (var kvp in matchLabels)
            {
                selector.MatchLabels.Labels.Add(kvp.Key, kvp.Value);
            }
        }

        if (@namespace != null)
        {
            selector.Namespace = @namespace;
        }

        rsp.Requirements ??= new Requirements();
        rsp.Requirements.Resources[name] = selector;
    }

    /// <summary>
    /// UpdateDesiredReadyStatus iterates through the desired resources in the response and updates their Ready status based on the observed resources in the request.
    /// </summary>
    /// <param name="response">The RunFunctionResponse containing the desired resources.</param>
    /// <param name="request">The RunFunctionRequest containing the observed resources.</param>
    /// <param name="_logger">The logger to use for logging information.</param>
    /// <param name="ignoreNoReadyCondition">Optional types implementing <see cref="IKubernetesObject"/> to mark ready when they have no Ready condition and no Synced=False condition.</param>
    public static void UpdateDesiredReadyStatus(this RunFunctionResponse response, RunFunctionRequest request, ILogger _logger, System.Type[]? ignoreNoReadyCondition = null)
    {
        var observed = request.GetObservedResources();
        var ignoredResourceTypes = ignoreNoReadyCondition?
            .Select(KubernetesResourceIdentity.Get)
            .Select(static identity => $"{identity.ApiVersion}/{identity.Kind}")
            .ToHashSet(StringComparer.Ordinal);

        foreach (var dr in response.Desired.Resources.ToDictionary())
        {
            // If this desired resource does not exist in the observed resources,
            // there is no observed readiness to derive, so leave it unchanged.
            if (observed.TryGetValue(dr.Key, out Resource? or))
            {
                var condition = or.GetCondition("Ready");
                var syncFailed = or.GetCondition("Synced")?.Fields["status"].StringValue == "False";

                // Preserve an explicitly ready desired resource when the observed
                // resource has no Ready condition and no synchronization failure.
                if (!syncFailed && dr.Value.Ready == Ready.True && condition == null)
                {
                    _logger.LogDebug("Ignoring desired resource that already has explicit readiness: {name} {ready}", dr.Key, dr.Value.Ready);
                    continue;
                }

                // Re-evaluate readiness from the observed resource on every invocation.
                // An observed resource may become not ready after previously being ready.
                _logger.LogDebug("Found desired resource to evaluate readiness: {name}", dr.Key);

                // A managed resource can retain Ready=True while its latest
                // reconciliation has failed. Synced=False takes precedence over
                // Ready=True and over the no-Ready-condition override below.
                if (!syncFailed && ignoredResourceTypes != null && condition == null && ignoredResourceTypes.Contains($"{or.Resource_.Fields["apiVersion"].StringValue}/{or.Resource_.Fields["kind"].StringValue}"))
                {
                    _logger.LogInformation("Resource has no Ready Condition and ignoreNoReadyCondition=true so resource is ready: {name}", dr.Key);
                    dr.Value.Ready = Ready.True;
                    continue;
                }

                if (!syncFailed && condition?.Fields["status"].StringValue == "True")
                {
                    _logger.LogInformation("Automatically determined that composed resource is ready: {name}", dr.Key);
                    dr.Value.Ready = Ready.True;
                }
                else
                {
                    _logger.LogInformation("Automatically determined that composed resource is not ready: {name}", dr.Key);
                    dr.Value.Ready = Ready.False;
                }
            }
            else
            {
                _logger.LogDebug("Ignoring desired resource that does not appear in observed resources: {name}", dr.Key);
                continue;
            }
        }

        if (response.Desired.Resources.All(x => x.Value.Ready == Ready.True))
        {
            _logger.LogInformation("All Desired Resources are ready");
        }
    }

    /// <summary>
    /// Adds or updates a desired Kubernetes resource using its canonical resource key.
    /// </summary>
    /// <param name="response">The response whose desired state will be updated.</param>
    /// <param name="resource">The Kubernetes resource to add or update.</param>
    /// <param name="key">A fallback key to use when the resource has no metadata name.</param>
    /// <exception cref="InvalidOperationException">Thrown when the resource has no metadata name and no key is provided.</exception>
    public static void AddDesiredResource(
        this RunFunctionResponse response,
        IKubernetesObject<V1ObjectMeta> resource,
        string? key = null)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(resource);

        var name = resource.Name();
        if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("Resource name is missing and no fallback key was provided.");
        }

        InitializeKubernetesIdentity(resource);

        var resourceKey = KubernetesResourceIdentity.CreateKey(
            resource,
            string.IsNullOrWhiteSpace(name) ? key! : name);
        response.Desired.AddOrUpdate(resourceKey, resource);
    }

    /// <summary>
    /// Adds a Crossplane Usage that protects one desired resource while another resource uses it.
    /// </summary>
    /// <param name="response">The response whose desired state will be updated.</param>
    /// <param name="by">The resource that uses the protected resource.</param>
    /// <param name="of">The resource being protected.</param>
    /// <param name="replayDeletion">Whether deletion of the protected resource is replayed after the usage is removed.</param>
    /// <exception cref="InvalidOperationException">Thrown when either resource has no metadata name.</exception>
    public static void AddDesiredUsage(
        this RunFunctionResponse response,
        IKubernetesObject<V1ObjectMeta> by,
        IKubernetesObject<V1ObjectMeta> of,
        bool replayDeletion = true)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(by);
        ArgumentNullException.ThrowIfNull(of);

        InitializeKubernetesIdentity(by);
        InitializeKubernetesIdentity(of);

        var byName = by.Name();
        var ofName = of.Name();
        if (string.IsNullOrWhiteSpace(byName) || string.IsNullOrWhiteSpace(ofName))
        {
            throw new InvalidOperationException("Both the using and protected resources must have metadata names.");
        }

        var usage = new V1beta1Usage
        {
            Spec = new()
            {
                ReplayDeletion = replayDeletion,
                By = new()
                {
                    ApiVersion = by.ApiVersion,
                    Kind = by.Kind,
                    ResourceRef = new() { Name = byName }
                },
                Of = new()
                {
                    ApiVersion = of.ApiVersion,
                    Kind = of.Kind,
                    ResourceRef = new() { Name = ofName }
                }
            }
        };

        var usageKey = $"{by.ApiVersion}-{by.Kind}-{byName}-{of.ApiVersion}-{of.Kind}-{ofName}";
        response.AddDesiredResource(usage, usageKey);
    }

    /// <summary>
    /// Gets a desired resource of the requested Kubernetes type.
    /// </summary>
    /// <typeparam name="T">The Kubernetes resource type.</typeparam>
    /// <param name="response">The response containing desired resources.</param>
    /// <param name="key">The key of the resource.</param>
    /// <returns>The desired resource, or null when no matching resource exists.</returns>
    public static T? GetDesiredResource<T>(this RunFunctionResponse response, string key)
        where T : IKubernetesObject<V1ObjectMeta>
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var resourceKey = KubernetesResourceIdentity.CreateKey<T>(key);
        if (response.Desired.Resources.TryGetValue(resourceKey, out var resource))
        {
            return resource.GetKubeResource<T>();
        }

        return default;
    }

    /// <summary>
    /// Gets all desired resources of the requested Kubernetes type.
    /// </summary>
    /// <typeparam name="T">The Kubernetes resource type.</typeparam>
    /// <param name="response">The response containing desired resources.</param>
    /// <returns>Desired resources whose API version and kind match the requested type.</returns>
    public static IEnumerable<T> GetDesiredResources<T>(this RunFunctionResponse response)
        where T : IKubernetesObject<V1ObjectMeta>
    {
        ArgumentNullException.ThrowIfNull(response);

        var identity = KubernetesResourceIdentity.Get<T>();
        foreach (var resource in response.Desired.Resources.Values)
        {
            if (!resource.Resource_.Fields.TryGetValue("apiVersion", out var apiVersion)
                || !resource.Resource_.Fields.TryGetValue("kind", out var kind)
                || !string.Equals(apiVersion.StringValue, identity.ApiVersion, StringComparison.Ordinal)
                || !string.Equals(kind.StringValue, identity.Kind, StringComparison.Ordinal))
            {
                continue;
            }

            yield return resource.GetKubeResource<T>();
        }
    }

    /// <summary>
    /// Validates that desired Kubernetes resource names are RFC 1123 DNS labels.
    /// </summary>
    /// <param name="response">The response containing desired resources to validate.</param>
    /// <exception cref="ArgumentException">Thrown when a desired resource has an invalid metadata name.</exception>
    public static void ValidateKubeResourceNames(this RunFunctionResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        foreach (var resource in response.Desired.Resources)
        {
            if (!resource.Value.Resource_.Fields.TryGetValue("metadata", out var metadata)
                || metadata.StructValue is not { } metadataStruct
                || !metadataStruct.Fields.TryGetValue("name", out var name)
                || string.IsNullOrEmpty(name.StringValue))
            {
                continue;
            }

            var resourceName = name.StringValue;
            if (IsRfc1123DnsLabel(resourceName))
            {
                continue;
            }

            resource.Value.Resource_.Fields.TryGetValue("kind", out var kind);
            var resourceKind = string.IsNullOrEmpty(kind?.StringValue) ? "unknown" : kind.StringValue;

            throw new ArgumentException(
                $"Resource '{resource.Key}' (kind '{resourceKind}') has invalid metadata.name '{resourceName}'. "
                + "Expected an RFC 1123 DNS label: 1-63 lowercase alphanumeric or '-' characters, starting and ending with an alphanumeric character.",
                nameof(response));
        }
    }

    private static bool IsRfc1123DnsLabel(string value)
    {
        if (value.Length is < 1 or > 63 || !char.IsAsciiLetterOrDigit(value[0]) || !char.IsAsciiLetterOrDigit(value[^1]))
        {
            return false;
        }

        foreach (var character in value)
        {
            if (!(char.IsAsciiLetterLower(character) || char.IsAsciiDigit(character) || character == '-'))
            {
                return false;
            }
        }

        return true;
    }

    private static void InitializeKubernetesIdentity(IKubernetesObject resource)
    {
        if (string.IsNullOrEmpty(resource.ApiVersion) || string.IsNullOrEmpty(resource.Kind))
        {
            resource.Initialize();
        }
    }

}
