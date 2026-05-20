namespace Umbraco.Workflow.Automate;

/// <summary>
/// Constants for the Workflow Automate package.
/// </summary>
public static class Constants
{
    /// <summary>
    /// Umbraco backoffice section aliases referenced by Workflow Automate step types.
    /// </summary>
    public static class Sections
    {
        /// <summary>
        /// The Workflow section — required for any step type that operates on workflow
        /// instances, content reviews, or workflow-related notifications.
        /// </summary>
        public const string Workflow = "workflow";
    }
}
