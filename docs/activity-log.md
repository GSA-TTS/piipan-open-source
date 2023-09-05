# [Activity Log](https://learn.microsoft.com/en-us/azure/azure-monitor/essentials/activity-log)

The Azure Monitor Activity Log is a platform log in Azure that provides insight into subscription-level events. The Activity Log includes information like when a resource is created, modified or deleted.

## Activity Log Configuration

Activity Log is configured at the subscription-level, not for each resource group.
Activity Log events are retained in Azure for 90 days and then deleted.

All Activity Log events are streamed to a central Event Hub and Log Analytics workspace. Activity Log events include the following types:

- Administrative
- Alert
- Autoscale
- Policy
- Recommendation
- Resource Health
- Security
- Service Health

## [Alerts](https://learn.microsoft.com/en-us/azure/azure-monitor/alerts/alerts-overview)

Activity Logs provide auditing of all actions that occurred on resources. Alerts are triggered when a specific event happens to a resource, such as the creation or deletion of a resource.

Activity Log Alerts are configured to be compliant with CIS Microsoft Azure Foundations Benchmark v1.3.0. The following Activity Log Alerts are configured:

- Create Policy Assignment
- Delete Policy Assignment
- Create or Update Network Security Group
- Delete Network Security Group
- Create or Update Network Security Group Rule
- Delete Security Group Rule
- Create or Update Security Solution
- Delete Security Solution
- Create or Update or Delete SQL Server Firewall Rule
- Delete SQL Server Firewall Rule

## [Action Groups](https://learn.microsoft.com/en-us/azure/azure-monitor/alerts/action-groups)

When an Activity Log Alert is triggered, Action Groups notify users about the alert and take an action. An Action Group is a collection of notification preferences that are defined by the owner of an Azure subscription.

Each Activity Group is configured to:

- Notify a security contact via email, configured through the environment file
- Stream the alert to a central Event Hub and Log Analytics workspace
- Optional configuration (via ARM) to notify Azure Resource Manager Roles: users who are assigned to certain subscription-level Azure Resource Manager roles
