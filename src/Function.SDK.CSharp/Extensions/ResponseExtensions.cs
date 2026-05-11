using System.Text.Json;
using Apiextensions.Fn.Proto.V1;
using Google.Protobuf.WellKnownTypes;

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

    public static void UpdateDesiredReadyStatus(this RunFunctionResponse response, RunFunctionRequest request, ILogger _logger, string[]? ignoreNoReadyCondition = null)
    {
        var observed = request.GetObservedComposedResources();

        foreach (var dr in response.Desired.Resources.ToDictionary())
        {
            // If this desired resource doesn't exist in the observed resources, it
            // can't be ready because it doesn't yet exist
            if (observed.TryGetValue(dr.Key, out Resource? or))
            {
                // Check if Ready
                if (dr.Value.Ready == Ready.True)
                {
                    _logger.LogDebug("Ignoring desired resource that already has explicit readiness: {name} {ready}", dr.Key, dr.Value.Ready);
                    continue;
                }

                // Now we know this resource exists, and not ready
                _logger.LogDebug("Found desired resource with unknown readiness: {name}", dr.Key);

                // If this observed resource has a status condition with type: Ready,
                // status: True, we set its readiness to true.
                var condition = or.GetCondition("Ready");

                if (ignoreNoReadyCondition != null && condition == null && ignoreNoReadyCondition.Contains($"{or.Resource_.Fields["apiVersion"].StringValue}/{or.Resource_.Fields["kind"].StringValue}"))
                {
                    _logger.LogInformation("Resource has no Ready Condition and ignoreNoReadyCondition=true so resource is ready: {name}", dr.Key);
                    dr.Value.Ready = Ready.True;
                    continue;
                }

                if (condition?.Fields["status"].StringValue == "True")
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
    /// Get Desired Resource from the supplied response.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="response"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static T? GetDesiredResource<T>(this RunFunctionResponse response, string key)
    {
        if (response.Desired.Resources.TryGetValue(key, out var resource))
        {
            return resource.GetKubeResource<T>();
        }

        return default;
    }
}
