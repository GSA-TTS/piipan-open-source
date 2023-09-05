#!/usr/bin/env bash
#
# Provisions and configures Event Grid Topic Subscriptions, excluding the
# Event Grid System Topic Subscription used for State Uploads (create blob events).
#
# TODO: The excluded Event Grid System Topic Subscription is created in
# create-resources.bash, but should be refactored.
#
# Assumes the script is executed by an Azure user with the Owner/Contributor
# role for the subscription.
#
# azure-env is the name of the deployment environment (e.g., "fns/dev").
# See iac/env for available environments.
#
# usage: create-event-grid-subscriptions.bash <azure-env>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  # shellcheck source=./iac/env/tts/dev.bash
  source "$(dirname "$0")"/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/iac-common.bash
  verify_cloud
  set_defaults

  # Retrieve Metrics Collection Storage Account
  storageId=$(get_storage_account_id "${METRICS_COLLECT_STORAGE_NAME}" "${RESOURCE_GROUP}")

  # Create Event Grid Topic Subscription for Bulk Upload Metrics (Update)
  # Used by Metrics Collection Function
  echo "Create Event Grid Topic Subscription for Bulk Upload Metrics - Update"
  az eventgrid event-subscription create \
    --source-resource-id "${GRID_TOPIC_PROVIDERS}/Microsoft.EventGrid/topics/${BU_METRICS_TOPIC_NAME}" \
    --name "${BU_METRICS_TOPIC_SUB}" \
    --endpoint "${storageId}${STORAGE_QUEUE}${METRICS_COLLECT_STORAGE_QUEUE_UPDATE_BU}" \
    --endpoint-type "storagequeue"

  # Create Event Grid Topic Subscription for Match Metrics
  # Used by Metrics Collection Function
  echo "Create Event Grid Topic Subscription for Match Metrics"
  az eventgrid event-subscription create \
    --source-resource-id "${GRID_TOPIC_PROVIDERS}/Microsoft.EventGrid/topics/${MATCH_METRICS_TOPIC_NAME}" \
    --name "${MATCH_METRICS_TOPIC_SUB}" \
    --endpoint "${storageId}${STORAGE_QUEUE}${METRICS_COLLECT_STORAGE_QUEUE_MATCH}" \
    --endpoint-type "storagequeue"

  # Create Event Grid Topic Subscription for Search Metrics
  # Used by Metrics Collection Function
  echo "Create Event Grid Topic Subscription for Search Metrics"
  az eventgrid event-subscription create \
    --source-resource-id "${GRID_TOPIC_PROVIDERS}/Microsoft.EventGrid/topics/${SEARCH_METRICS_TOPIC_NAME}" \
    --name "${SEARCH_METRICS_TOPIC_SUB}" \
    --endpoint "${storageId}${STORAGE_QUEUE}${METRICS_COLLECT_STORAGE_QUEUE_SEARCH}" \
    --endpoint-type "storagequeue"

  # Create Event Grid System Topic Subscription for State Upload Metrics
  while IFS=$',\t\r\n' read -r abbr name _; do
    abbr=$(echo "${abbr}" | tr '[:upper:]' '[:lower:]')
    topic_name="evgt-${abbr}-upload-${ENV}"
    sub_name="evgs-${abbr}-create-bulk-upload-metrics"

    echo "Create Event Grid System Topic Subscription for State Upload Metrics - ${abbr}"
    az eventgrid system-topic event-subscription create \
      --name "${sub_name}" \
      --system-topic-name "${topic_name}" \
      --resource-group "${RESOURCE_GROUP}" \
      --endpoint "${storageId}${STORAGE_QUEUE}${METRICS_COLLECT_STORAGE_QUEUE_CREATE_BU}" \
      --endpoint-type storagequeue \
      --included-event-types Microsoft.Storage.BlobCreated \
      --subject-begins-with /blobServices/default/containers/upload/blobs/
  done < env/"${azure_env}"/states.csv

  # Create an Event Grid Subscription for the Notify Storage Account Queue
  echo "Create Event Grid Topic Subscription for Notify"
  notifyStorageId=$(get_storage_account_id "${NOTIFY_FUNC_APP_STORAGE_NAME}" "${RESOURCE_GROUP}")
  az eventgrid event-subscription create \
    --source-resource-id "${GRID_TOPIC_PROVIDERS}/Microsoft.EventGrid/topics/${NOTIFY_TOPIC_NAME}" \
    --name "${NOTIFY_TOPIC_SUB}" \
    --endpoint "${notifyStorageId}${STORAGE_QUEUE}${NOTIFY_FUNC_APP_STORAGE_QUEUE}" \
    --endpoint-type "storagequeue"

  script_completed
}

main "$@"

