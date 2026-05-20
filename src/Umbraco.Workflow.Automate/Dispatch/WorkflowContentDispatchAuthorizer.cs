using Umbraco.Automate.Core.Dispatch.Authorization;
using Umbraco.Automate.Core.Security;
using Umbraco.Cms.Core.Actions;

namespace Umbraco.Workflow.Automate.Dispatch;

/// <summary>
/// Dispatch-time authoriser for workflow triggers whose output is bound to a CMS content
/// node. Mirrors the built-in <c>NodeScopedTriggerDispatchAuthorizer</c> for Automate's own
/// content triggers, but is owned by Workflow.Automate because the relevant outputs live
/// here and the built-in marker is internal to Automate.Core.
/// </summary>
internal sealed class WorkflowContentDispatchAuthorizer : ITriggerDispatchAuthorizer
{
    private static readonly IReadOnlySet<string> BrowsePermissions = new HashSet<string> { ActionBrowse.ActionLetter };

    private readonly IAutomationActionAuthorizer _nodeAuthorizer;

    public WorkflowContentDispatchAuthorizer(IAutomationActionAuthorizer nodeAuthorizer)
    {
        _nodeAuthorizer = nodeAuthorizer;
    }

    public async Task<AutomationAuthorizationResult> AuthorizeAsync(
        TriggerDispatchAuthorizationContext context,
        CancellationToken cancellationToken)
    {
        if (context.TypedOutput is not IContentScopedWorkflowOutput output)
        {
            return AutomationAuthorizationResult.Success;
        }

        if (output.GetContentKey() is not { } contentKey)
        {
            // Workflow can run against entities that aren't documents (e.g. settings nodes
            // with no key) — treat the absence of a key as "not applicable" rather than deny.
            return AutomationAuthorizationResult.Success;
        }

        return await _nodeAuthorizer.AuthorizeContentAsync(
            context.ServiceAccount, contentKey, BrowsePermissions, cancellationToken);
    }
}
