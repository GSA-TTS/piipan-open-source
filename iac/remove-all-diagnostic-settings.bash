#!/usr/bin/env bash
#
# Removes the specified diagnostic settings for every resource in an Azure Subscription.
# This is a helper script created to clean-up old resource settings.
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# usage: remove-all-diagnostic-settings.bash <azure-env>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit

# Removes the specified diagnostic setting for every resource in an Azure Subscription
remove_diagnostic_settings () {
  local diagnostic_settings_name=$1
  resources=$(\
    az resource list \
      --subscription "${SUBSCRIPTION_ID}" \
      --query [].id \
      --output tsv)

  while IFS=$',\t\r\n' read -r resource; do
    echo "Removing Diagnostic Setting: '${diagnostic_settings_name}' from '${resource}'"
    az monitor diagnostic-settings delete \
      --name "${diagnostic_settings_name}" \
      --resource "${resource}" || echo "Nothing to remove"
  done <<< "${resources}"

  # Removes the specified diagnostic setting from the subscription
  echo "Removing Activity Log Diagnostic Settings: '${diagnostic_settings_name}'"
  az monitor diagnostic-settings subscription delete \
   --name "${diagnostic_settings_name}" \
   --subscription "${SUBSCRIPTION_ID}" \
   --yes
}

# Deleting a Storage Account will not always remove the nested resource diagnostic settings
# Remove diagnostic settings to avoid conflicts with existing reources
# TODO: It's possible this is happening due to the soft-deleteRetentionPolicy,
# or this could just be a bug with Azure. Should examine further.
remove_storage_account_diagnostic_settings () {
  local diagnostic_settings_name=$1
  storage_accounts=$(\
    az storage account list \
      --subscription "${SUBSCRIPTION_ID}" \
      --query [].id \
      --output tsv)

  while IFS=$',\t\r\n' read -r storage; do
    # Storage Account nested resources
    local blob_service="/blobServices/default"
    local file_service="/fileServices/default"
    local queue_service="/queueServices/default"
    local table_service="/tableServices/default"

    # Empty string is the parent storage account
    for resource in "" "${blob_service}" "${file_service}" "${queue_service}" "${table_service}"
    do
      # AZ delete command fails gracefully if the diagnostic setting doesn't exist
      echo "Removing Diagnostic Setting: '${diagnostic_settings_name}' from '${storage}${resource}'"
      az monitor diagnostic-settings delete \
        --name "${diagnostic_settings_name}" \
        --resource "${storage}${resource}"
    done
  done <<< "${storage_accounts}"
}

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  source "$(dirname "$0")"/env/"${azure_env}".bash
  source "$(dirname "$0")"/iac-common.bash
  verify_cloud
  set_defaults

  local diagnostic_settings_name="stream-logs-to-event-hub"
  remove_diagnostic_settings ${diagnostic_settings_name}
  remove_storage_account_diagnostic_settings ${diagnostic_settings_name}

  script_completed
}

main "$@"
