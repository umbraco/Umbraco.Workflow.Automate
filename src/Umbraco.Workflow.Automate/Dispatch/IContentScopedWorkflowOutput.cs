namespace Umbraco.Workflow.Automate.Dispatch;

/// <summary>
/// Marker for workflow trigger outputs whose event subject is a CMS content node. Surfaces
/// the content key so <see cref="WorkflowContentDispatchAuthorizer"/> can authorise the
/// workspace service account against the node's start-node path and Browse permission.
/// </summary>
/// <remarks>
/// Internal because the contract is between Workflow.Automate's outputs and its own
/// authoriser — third-party packages should declare their own marker if they need similar
/// gating.
/// </remarks>
internal interface IContentScopedWorkflowOutput
{
    /// <summary>
    /// Returns the content key the event refers to, or <c>null</c> when the event isn't
    /// bound to a specific node (the authoriser will skip the check in that case).
    /// </summary>
    Guid? GetContentKey();
}
