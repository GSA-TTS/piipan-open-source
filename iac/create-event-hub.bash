#!/usr/bin/env bash
#
# Creates Event Hub in Azure
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# usage: create-event-hub.bash <azure-env>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit


main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  source "$(dirname "$0")"/env/"${azure_env}".bash
  source "$(dirname "$0")"/iac-common.bash
  verify_cloud
  set_defaults

  # Create an Event Hub namespace and hub where resource logs will be streamed,
  # as well as an application registration that can be used to read logs
  siem_app_id=$(\
    az ad sp list \
      --display-name "${SIEM_RECEIVER}" \
      --filter "displayname eq '${SIEM_RECEIVER}'" \
      --query "[0].id" \
      --output tsv)
  # Avoid resetting password by only creating app registration if it does not exist
  if [ -z "${siem_app_id}" ]; then
    siem_app_id=$(\
      az ad sp create-for-rbac \
        --name "${SIEM_RECEIVER}" \
        --role "Reader" \
        --scopes "/subscriptions/${SUBSCRIPTION_ID}" \
        --query "[0].id" \
        --output tsv)

    # Wait to avoid "InvalidPrincipalId" on app registration use below
    sleep 60
  fi

  # Create event hub and assign role to app registration
  # EH_ZONE_REDUNDANCY must be set to false for regions that do not
  # support zone redundancy, like USGov-Arizona in a DR scenario
  local eh_arm
  if [ "${EH_ZONE_REDUNDANCY}" = "true" ] ; then
    eh_arm="event-hub-monitoring-zr.json"
  else
    eh_arm="event-hub-monitoring.json"
  fi

  echo "Creating Event Hub"
  az deployment group create \
   --name monitoring \
   --resource-group "${RESOURCE_GROUP}" \
   --template-file  "./arm-templates/${eh_arm}" \
   --parameters \
     resourceTags="${RESOURCE_TAGS}" \
     location="${LOCATION}" \
     env="${ENV}" \
     prefix="${PREFIX}" \
     receiverId="${siem_app_id}" \
     diagnosticSettingName="${DIAGNOSTIC_SETTINGS_NAME}" \
     workspaceId="${LOG_ANALYTICS_WORKSPACE_ID}"

  echo "Send Log Analytics workspace logs to Event Hub"
  update_diagnostic_settings "${LOG_ANALYTICS_WORKSPACE_ID}" "${DIAGNOSTIC_SETTINGS_WORKSPACE}"

  script_completed
}

main "$@"
