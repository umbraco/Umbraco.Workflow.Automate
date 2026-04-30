using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

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
    }
}
