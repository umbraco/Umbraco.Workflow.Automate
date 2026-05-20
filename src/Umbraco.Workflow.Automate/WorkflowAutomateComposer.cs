using Umbraco.Automate.Extensions;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Workflow.Automate.Dispatch;

namespace Umbraco.Workflow.Automate;

/// <summary>
/// Registers Workflow Automate with the Umbraco composition pipeline.
/// </summary>
/// <remarks>
/// No bridge handlers are required because Workflow notifications already implement
/// <c>INotification</c> and are published through the Umbraco CMS notification pipeline.
/// The Automate framework auto-discovers trigger classes via the <c>[Trigger]</c> attribute.
/// </remarks>
public sealed class WorkflowAutomateComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // Gates dispatch of content-scoped workflow triggers against the workspace service
        // account's start-node path. The built-in NodeScopedTriggerDispatchAuthorizer only
        // recognises Automate's own content/media triggers — Workflow's are distinct
        // outputs, so we register a sibling authoriser keyed on IContentScopedWorkflowOutput.
        builder.AutomateTriggerDispatchAuthorizers()
            .Add<WorkflowContentDispatchAuthorizer>();
    }
}
