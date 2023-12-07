#!/usr/bin/env bash
#
# Provisions and configures Event Grid Topics, excluding the
# Event Grid System Topic used for State Uploads (create blob events).
#
# TODO: The excluded Event Grid System Topic is created in
# create-resources.bash, but should be refactored.
#
# Assumes the script is executed by an Azure user with the Owner/Contributor
# role for the subscription.
#
# azure-env is the name of the deployment environment (e.g., "tenant/dev").
# See iac/env for available environments.
#
# usage: create-event-grid-topics.bash <azure-env>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  # shellcheck source=./iac/env/tts/dev.bash
  source "$(dirname "$0")"/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/iac-common.bash
  set_defaults

  echo "Create Event Grid Topic for Bulk Upload Metrics"
  event_grid_topic_id=$(\
    az eventgrid topic create \
      --location "${LOCATION}" \
      --name "${BU_METRICS_TOPIC_NAME}" \
      --resource-group "${RESOURCE_GROUP}" \
      --identity "systemassigned" \
      --output tsv \
      --query id)
  # Stream Event Grid topic diagnostic logs to Event Hub / Log Analytisc workspace
  update_event_grid_topic_diagnostic_settings "${event_grid_topic_id}"

  echo "Create Event Grid Topic for Match Metrics"
  event_grid_topic_id=$(\
    az eventgrid topic create \
      --location "${LOCATION}" \
      --name "${MATCH_METRICS_TOPIC_NAME}" \
      --resource-group "${RESOURCE_GROUP}" \
      --identity "systemassigned" \
      --output tsv \
      --query id)
  # Stream Event Grid topic diagnostic logs to Event Hub / Log Analytisc workspace
  update_event_grid_topic_diagnostic_settings "${event_grid_topic_id}"

  echo "Create Event Grid Topic for Search Metrics"
  event_grid_topic_id=$(\
    az eventgrid topic create \
      --location "${LOCATION}" \
      --name "${SEARCH_METRICS_TOPIC_NAME}" \
      --resource-group "${RESOURCE_GROUP}" \
      --identity "systemassigned" \
      --output tsv \
      --query id)
  # Stream Event Grid topic diagnostic logs to Event Hub / Log Analytisc workspace
  update_event_grid_topic_diagnostic_settings "${event_grid_topic_id}"

  echo "Create Event Grid Topic for Notify"
  event_grid_topic_id=$(\
    az eventgrid topic create \
      --location "${LOCATION}" \
      --name "${NOTIFY_TOPIC_NAME}" \
      --resource-group "${RESOURCE_GROUP}" \
      --identity "systemassigned" \
      --output tsv \
      --query id)
  # Stream Event Grid topic diagnostic logs to Event Hub / Log Analytisc workspace
  update_event_grid_topic_diagnostic_settings "${event_grid_topic_id}"

  script_completed
}

main "$@"

