namespace Function.SDK.CSharp;

/// <summary>
/// Well-known Kubernetes annotations used by Crossplane.
/// </summary>
public static class Annotations
{
    /// <summary>
    /// The external name of a managed resource.
    /// </summary>
    public const string CrossplaneExternalName = "crossplane.io/external-name";

    /// <summary>
    /// The timestamp when an external resource creation was initiated.
    /// </summary>
    public const string CrossplaneExternalCreatePending = "crossplane.io/external-create-pending";

    /// <summary>
    /// The timestamp when an external resource creation succeeded.
    /// </summary>
    public const string CrossplaneExternalCreateSucceeded = "crossplane.io/external-create-succeeded";

    /// <summary>
    /// The timestamp when an external resource creation failed.
    /// </summary>
    public const string CrossplaneExternalCreateFailed = "crossplane.io/external-create-failed";

    /// <summary>
    /// Indicates that Crossplane should pause reconciliation of a resource.
    /// </summary>
    public const string CrossplanePaused = "crossplane.io/paused";

    /// <summary>
    /// The interval at which Crossplane polls an external resource.
    /// </summary>
    public const string CrossplanePollInterval = "crossplane.io/poll-interval";

    /// <summary>
    /// The timestamp of a request to trigger resource reconciliation.
    /// </summary>
    public const string CrossplaneReconcileRequestedAt = "crossplane.io/reconcile-requested-at";
}
