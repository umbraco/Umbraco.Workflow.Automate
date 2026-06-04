## Umbraco.Workflow.Automate

Umbraco Workflow triggers for Umbraco Automate - react to content approval workflow events from your automations.

### Features

- **10 Triggers** - React to workflow lifecycle, task assignment, email, and content review events (e.g. Workflow Approved, Workflow Rejected, Task Assigned, Content Review Completed)
- **Rich Trigger Outputs** - Node IDs, entity keys, workflow types and statuses, authors, comments, and timestamps
- **Zero Configuration** - Workflow publishes standard CMS notifications, so triggers subscribe natively; no further wiring required

Example: notify a channel when a workflow is rejected, or escalate when reminder emails pile up.

### Requirements

- Umbraco CMS 17.x
- Umbraco Workflow 17.x
- Umbraco.Automate 17.0+
- .NET 10.0
