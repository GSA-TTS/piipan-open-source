# Log Streaming

In keeping with [NIST SP 800-53 control AU-6](https://csrc.nist.gov/Projects/risk-management/sp800-53-controls/release-search#!/control?version=4.0&number=AU-6), resource logs are streamed to a central location where they can be [accessed by an external Security Information and Event Management (SIEM) tool](https://docs.microsoft.com/en-us/azure/azure-monitor/essentials/stream-monitoring-data-event-hubs#partner-tools-with-azure-monitor-integration). This is accomplished in Azure using a combination of [Event Hub](https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-about), resource [diagnostic settings](https://docs.microsoft.com/en-us/azure/azure-monitor/essentials/diagnostic-settings?tabs=CMD), and an [application registration](https://docs.microsoft.com/en-us/azure/event-hubs/authenticate-application) for accessing and reading logs.

Logs can also be accessed and analyzed within Azure using [Azure Monitor](https://docs.microsoft.com/en-us/azure/azure-monitor/essentials/monitor-azure-resource) and pointing diagnostic settings to a [Log Analytics workspace](https://docs.microsoft.com/en-us/azure/azure-monitor/essentials/resource-logs#send-to-log-analytics-workspace).

## Event Hub configuration

All resource logs are streamed to a central Event Hub. The IaC establishes the following configuration in each deployment environment:

- A single Event Hub namespace (`evh-monitoring`), with the default `RootManageSharedAccessKey` shared access policy.
- Within the namespace, a single event hub named `logs`.
- Two consumer groups are created; the default `$Default` consumer group and `Splunk` consumer group.
- Zone redundancy for regions that support multi-zones, configured in the environment bash file.

## Diagnostic Settings configuration

Each Azure resource (Function App, Database, etc.) is configured with a diagnostic setting that sends all logs to Event Hub and a Log Analytics workspace. To ensure consistency, a single diagnostic setting is used for both Event Hub and Log Analytics workspace.

For each resource, the IaC establishes a single diagnostic setting named `stream-logs-to-event-hub`. The only exception is the diagnostic setting for Activity Log, which is named `stream-logs-to-event-hub-${ENV}`, where `${ENV}` is defined by the environment file. Each diagnostic setting is configured to:

- Stream all desired logging categories
- Stream logs to the `logs` event hub within the `evh-monitoring` namespace
- Stream logs to the environment specific Log Analytics workspace
- Use the default `RootManageSharedAccessKey` as the event hub policy
- *Note: Changing the diagnostic setting name for existing Azure resource can cause IaC - ARM errors unless the previous diagnostic setting is removed. This results from the `RootManageSharedAccessKey` used across multiple diagnostic settings (old name and new name). Thus, the diagnostic setting will remain with the name `stream-logs-to-event-hub` rather than a more generic name like `stream-diagnostic-logs`.*

Resources have two categories of logging: "logs" and "metrics". Our default practice is to stream all logs categories and no metrics categories. Categories can be enabled/disabled as required — i.e., if the team analyzing audit logs determines a certain log category to produce too much noise and be unnecessary.

## Application logging

Some details required for NIST compliance are not built into default resource logging. For example, AU-3 requires logging the "identity of any individuals or subjects associated with the event." However, when a Function app that sits behind EasyAuth is called via a managed identity the identity information is not included in any built-in logging.

In these cases, log messages are explicitly written at the application level:

```
log.LogInformation("Executing request from user {User}", req.HttpContext?.User.Identity.Name);
```

## Accessing and reading logs

The IaC creates an application registration that can be used by an external SIEM tool to access and read logs. The application registration is explicitly granted the [`Azure Event Hubs Data Receiver` role](https://docs.microsoft.com/en-us/azure/event-hubs/authenticate-application#built-in-roles-for-azure-event-hubs) on the `logs` event hub.
