#!/usr/bin/env bash
#
# Configures Activity Log, Activity Log Alerts, and Alert Groups.
#
# Assumes the script is executed by an Azure user with the Owner/Contributor role for the subscription.
#
# usage: configure-activity-log.bash <azure-env>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../../tools/common.bash || exit

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  source "$(dirname "$0")"/../env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/../iac-common.bash
  verify_cloud
  set_defaults

  echo "Configure Activity Logs"
  # Configure Activity Log for all Log categories
  # Send Events from Subscription's Activity log to Event Hub / Log Analytics workspace
  # Diagnostic Settings Name must use prefix, as Activity Log is Subscription wide
  az deployment sub create \
    --name "activity-log-diagnostics-${ENV}" \
    --location "${LOCATION}" \
    --template-file "$(dirname "$0")"/../arm-templates/security/activity-log.json \
    --parameters \
      diagnosticSettingName="${DIAGNOSTIC_SETTINGS_NAME}-${ENV}" \
      eventHubAuthorizationRuleId="${EH_RULE_ID}" \
      eventHubName="${EVENT_HUB_NAME}" \
      workspaceId="${LOG_ANALYTICS_WORKSPACE_ID}"

  echo "Configure Activity Log Alerts - Administrative"
  az deployment group create \
    --name "activity-log-alerts-administrative-${ENV}" \
    --resource-group "${SECURITY_RESOURCE_GROUP}" \
    --template-file "$(dirname "$0")"/../arm-templates/security/activity-log-alert-administrative.json \
    --parameters \
      emailAddress="${ACTIVITY_LOG_ALERT_EMAIL}" \
      eventHubName="${EVENT_HUB_NAME}" \
      eventHubNameSpace="${EVENT_HUB_NAMESPACE}"

  echo "Configure Activity Log Alerts - Security"
  az deployment group create \
    --name "activity-log-alerts-policy-${ENV}" \
    --resource-group "${SECURITY_RESOURCE_GROUP}" \
    --template-file "$(dirname "$0")"/../arm-templates/security/activity-log-alert-security.json \
    --parameters \
      emailAddress="${ACTIVITY_LOG_ALERT_EMAIL}" \
      eventHubName="${EVENT_HUB_NAME}" \
      eventHubNameSpace="${EVENT_HUB_NAMESPACE}"

  echo "Configure Activity Log Alerts - Service Health"
  az deployment group create \
    --name "activity-log-alerts-service-health-${ENV}" \
    --resource-group "${SECURITY_RESOURCE_GROUP}" \
    --template-file "$(dirname "$0")"/../arm-templates/security/activity-log-alert-service-health.json \
    --parameters \
      emailAddress="${ACTIVITY_LOG_ALERT_EMAIL}" \
      eventHubName="${EVENT_HUB_NAME}" \
      eventHubNameSpace="${EVENT_HUB_NAMESPACE}"

  script_completed
}

main "$@"
