#!/usr/bin/env bash
#
# Provisions and configures the infrastructure components for all Piipan Metrics subsystems.
# Assumes an Azure user with the Global Administrator role has signed in with the Azure CLI.
# Assumes Piipan base resource groups, resources have been created in the same environment
# (for example, state-specific blob topics).
# Must be run from a trusted network.
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# usage: create-metrics-resources.bash <azure-env>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit

set_constants () {
  DB_SERVER_NAME=$PREFIX-psql-core-$ENV
  DB_NAME=metrics

  # VNet, which should always exist when running this script
  VNET_ID=$(get_vnet)

  set_defaults
}

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  # shellcheck source=./iac/env/tts/dev.bash
  source "$(dirname "$0")"/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/iac-common.bash
  verify_cloud
  set_constants

  # Create Metrics Collect Function Storage Account
  echo "Creating Storage Account: ${METRICS_COLLECT_STORAGE_NAME}"
  az deployment group create \
    --name "${METRICS_COLLECT_STORAGE_NAME}" \
    --resource-group "${RESOURCE_GROUP}" \
    --template-file ./arm-templates/function-storage.json \
    --parameters \
      storageAccountName="${METRICS_COLLECT_STORAGE_NAME}" \
      resourceTags="${RESOURCE_TAGS}" \
      location="${LOCATION}" \
      diagnosticSettingName="${DIAGNOSTIC_SETTINGS_NAME}" \
      eventHubAuthorizationRuleId="${EH_RULE_ID}" \
      eventHubName="${EVENT_HUB_NAME}" \
      queueNames="('${METRICS_COLLECT_STORAGE_QUEUE_CREATE_BU}','${METRICS_COLLECT_STORAGE_QUEUE_UPDATE_BU}','${METRICS_COLLECT_STORAGE_QUEUE_MATCH}','${METRICS_COLLECT_STORAGE_QUEUE_SEARCH}')" \
      sku="${STORAGE_SKU}" \
      subnet="${FUNC_SUBNET_NAME}" \
      vnet="${VNET_ID}" \
      workspaceId="${LOG_ANALYTICS_WORKSPACE_ID}"

  # Create Metrics Collect Function App in Azure
  echo "Creating function: ${METRICS_COLLECT_APP_NAME}"
  az functionapp create \
    --resource-group "${RESOURCE_GROUP}" \
    --tags "Project=${PROJECT_TAG}" \
    --plan "${APP_SERVICE_PLAN_FUNC_NAME}" \
    --runtime "dotnet" \
    --functions-version 4 \
    --name "${METRICS_COLLECT_APP_NAME}" \
    --storage-account "${METRICS_COLLECT_STORAGE_NAME}" \
    --assign-identity "[system]" \
    --disable-app-insights true \
    --https-only true

  # Create an Active Directory app registration associated with the app.
  az ad app create \
    --display-name "${METRICS_COLLECT_APP_NAME}" \
    --sign-in-audience "AzureADMyOrg"

  # Integrate function app into Virtual Network
  echo "Integrating ${METRICS_COLLECT_APP_NAME} into virtual network"
  az functionapp vnet-integration add \
    --name "${METRICS_COLLECT_APP_NAME}" \
    --resource-group "${RESOURCE_GROUP}" \
    --subnet "${FUNC_SUBNET_NAME}" \
    --vnet "${VNET_NAME}"

  # Allow only incoming traffic from Event Grid
  # Only set rule if it does not exist, to avoid error
  exists=$(\
    az functionapp config access-restriction show \
      -n "${METRICS_COLLECT_APP_NAME}" \
      -g "${RESOURCE_GROUP}" \
      --query "ipSecurityRestrictions[?ip_address == 'AzureEventGrid'].ip_address" \
      --output tsv)
  if [ -z "$exists" ]; then
    az functionapp config access-restriction add \
      -n "${METRICS_COLLECT_APP_NAME}" \
      -g "${RESOURCE_GROUP}" \
      --priority 100 \
      --service-tag AzureEventGrid
  fi

  echo "Configure: ${METRICS_COLLECT_APP_NAME}"
  az functionapp config set \
    --name "${METRICS_COLLECT_APP_NAME}" \
    --resource-group "${RESOURCE_GROUP}" \
    --always-on true \
    --ftps-state "Disabled" \
    --min-tls-version "1.2" \
    --vnet-route-all-enabled true

  echo "Removing public access for ${METRICS_COLLECT_STORAGE_NAME}"
  az storage account update \
    --name "${METRICS_COLLECT_STORAGE_NAME}" \
    --resource-group "${RESOURCE_GROUP}" \
    --default-action "Deny"

  # Configure log streaming for function app
  metrics_collect_function_id=$(\
    az functionapp show \
      -n "${METRICS_COLLECT_APP_NAME}" \
      -g "${RESOURCE_GROUP}" \
      --output tsv \
      --query id)
  update_diagnostic_settings "${metrics_collect_function_id}" "${DIAGNOSTIC_SETTINGS_FUNC}"

  echo "Creating Application Insight for function: ${METRICS_COLLECT_APP_NAME}"
  appInsightConnectionString=$(\
    az monitor app-insights component create \
      --app "${METRICS_COLLECT_APP_NAME}" \
      --location "${LOCATION}" \
      --resource-group "${RESOURCE_GROUP}" \
      --tags "Project=${PROJECT_TAG}" \
      --application-type "${APPINSIGHTS_KIND}" \
      --kind "${APPINSIGHTS_KIND}" \
      --workspace "${LOG_ANALYTICS_WORKSPACE_ID}" \
      --query "${APPINSIGHTS_CONNECTION_STRING}" \
      --output tsv)

  echo "Integrating Application Insight with function: ${METRICS_COLLECT_APP_NAME}"
  az monitor app-insights component connect-function \
    --app "${METRICS_COLLECT_APP_NAME}" \
    --function "${METRICS_COLLECT_APP_NAME}" \
    --resource-group "${RESOURCE_GROUP}"

  echo "Configure appsettings for: ${METRICS_COLLECT_APP_NAME}"
  db_conn_str=$(pg_connection_string "$DB_SERVER_NAME" "$DB_NAME" "${METRICS_COLLECT_APP_NAME//-/_}")
  eventgrid_endpoint=$(eventgrid_endpoint "${RESOURCE_GROUP}" "${BU_METRICS_TOPIC_NAME}")
  eventgrid_key_str=$(eventgrid_key_string "${RESOURCE_GROUP}" "${BU_METRICS_TOPIC_NAME}")
  eventgrid_match_metrics_endpoint=$(eventgrid_endpoint "${RESOURCE_GROUP}" "${MATCH_METRICS_TOPIC_NAME}")
  eventgrid_match_metrics_key_string=$(eventgrid_key_string "${RESOURCE_GROUP}" "${MATCH_METRICS_TOPIC_NAME}")
  eventgrid_search_endpoint=$(eventgrid_endpoint "${RESOURCE_GROUP}" "${SEARCH_METRICS_TOPIC_NAME}")
  eventgrid_search_key_string=$(eventgrid_key_string "${RESOURCE_GROUP}" "${SEARCH_METRICS_TOPIC_NAME}")

  az functionapp config appsettings set \
    --resource-group "${RESOURCE_GROUP}" \
    --name "${METRICS_COLLECT_APP_NAME}" \
    --settings \
      ${APPLICATIONINSIGHTS_CONNECTION_STRING}="${appInsightConnectionString}" \
      ${CLOUD_NAME_STR_KEY}="${CLOUD_NAME}" \
      ${METRICS_DB_CONN_STR_KEY}="${db_conn_str}" \
      ${EVENTGRID_CONN_STR_ENDPOINT}="${eventgrid_endpoint}" \
      ${EVENTGRID_CONN_STR_KEY}="${eventgrid_key_str}" \
      ${EVENTGRID_CONN_METRICS_MATCH_STR_ENDPOINT}="${eventgrid_match_metrics_endpoint}" \
      ${EVENTGRID_CONN_METRICS_MATCH_STR_KEY}="${eventgrid_match_metrics_key_string}" \
      ${EVENTGRID_CONN_METRICS_SEARCH_STR_ENDPOINT}="${eventgrid_search_endpoint}" \
      ${EVENTGRID_CONN_METRICS_SEARCH_STR_KEY}="${eventgrid_search_key_string}" \
    --output none

  # Waiting before publishing the app, since publishing immediately after creation returns an App Not Found error
  # Waiting was the best solution I could find. More info in these GH issues:
  # https://github.com/Azure/azure-functions-core-tools/issues/1616
  # https://github.com/Azure/azure-functions-core-tools/issues/1766
  echo "Waiting to publish function app"
  sleep 60

  # publish the function app
  try_run "func azure functionapp publish ${METRICS_COLLECT_APP_NAME} --dotnet" 7 "../metrics/src/Piipan.Metrics/${METRICS_COLLECT_APP_FILEPATH}"

  # Create Event Grid Subscriptions for Metrics
  ./create-event-grid-subscriptions.bash "${azure_env}"

  # Create Metrics API Function Storage Account
  echo "Creating storage account: ${METRICS_API_APP_STORAGE_NAME}"
  az deployment group create \
    --name "${METRICS_API_APP_STORAGE_NAME}" \
    --resource-group "${RESOURCE_GROUP}" \
    --template-file ./arm-templates/function-storage.json \
    --parameters \
      storageAccountName="${METRICS_API_APP_STORAGE_NAME}" \
      resourceTags="${RESOURCE_TAGS}" \
      location="${LOCATION}" \
      diagnosticSettingName="${DIAGNOSTIC_SETTINGS_NAME}" \
      eventHubAuthorizationRuleId="${EH_RULE_ID}" \
      eventHubName="${EVENT_HUB_NAME}" \
      sku="${STORAGE_SKU}" \
      subnet="${FUNC_SUBNET_NAME}" \
      vnet="${VNET_ID}" \
      workspaceId="${LOG_ANALYTICS_WORKSPACE_ID}"

  # Create Metrics API Function App in Azure
  echo "Creating function app: ${METRICS_API_APP_NAME}"
  az functionapp create \
    --resource-group "${RESOURCE_GROUP}" \
    --plan "${APP_SERVICE_PLAN_FUNC_NAME}" \
    --tags "Project=${PROJECT_TAG}" \
    --runtime "dotnet" \
    --functions-version 4 \
    --name "${METRICS_API_APP_NAME}" \
    --storage-account "${METRICS_API_APP_STORAGE_NAME}" \
    --assign-identity "[system]" \
    --disable-app-insights true \
    --https-only true

  # Create an Active Directory app registration associated with the app.
  az ad app create \
    --display-name "${METRICS_API_APP_NAME}" \
    --sign-in-audience "AzureADMyOrg"

  # Integrate function app into Virtual Network
  echo "Integrating $METRICS_API_APP_NAME into virtual network"
  az functionapp vnet-integration add \
    --name "$METRICS_API_APP_NAME" \
    --resource-group "${RESOURCE_GROUP}" \
    --subnet "$FUNC_SUBNET_NAME" \
    --vnet "$VNET_NAME"

  echo "Configure: ${METRICS_API_APP_NAME}"
  az functionapp config set \
    --name "${METRICS_API_APP_NAME}" \
    --resource-group "${RESOURCE_GROUP}" \
    --always-on true \
    --ftps-state "Disabled" \
    --min-tls-version "1.2" \
    --vnet-route-all-enabled true

  echo "Removing public access for ${METRICS_API_APP_STORAGE_NAME}"
  az storage account update \
    --name "${METRICS_API_APP_STORAGE_NAME}" \
    --resource-group "${RESOURCE_GROUP}" \
    --default-action "Deny"

  echo "Creating Application Insight for function: ${METRICS_API_APP_NAME}"
  appInsightConnectionString=$(\
  az monitor app-insights component create \
    --app "${METRICS_API_APP_NAME}" \
    --location "${LOCATION}" \
    --resource-group "${RESOURCE_GROUP}" \
    --tags "Project=${PROJECT_TAG}" \
    --application-type "${APPINSIGHTS_KIND}" \
    --kind "${APPINSIGHTS_KIND}" \
    --workspace "${LOG_ANALYTICS_WORKSPACE_ID}" \
    --query "${APPINSIGHTS_CONNECTION_STRING}" \
    --output tsv)

  echo "Integrating Application Insight with function: ${METRICS_API_APP_NAME}"
  az monitor app-insights component connect-function \
    --app "${METRICS_API_APP_NAME}" \
    --function "${METRICS_API_APP_NAME}" \
    --resource-group "${RESOURCE_GROUP}"

  echo "Configure appsettings for: ${METRICS_API_APP_NAME}"
  db_conn_str=$(pg_connection_string "$DB_SERVER_NAME" "$DB_NAME" "${METRICS_API_APP_NAME//-/_}")
  az functionapp config appsettings set \
      --resource-group "${RESOURCE_GROUP}" \
      --name "$METRICS_API_APP_NAME" \
      --settings \
        ${APPLICATIONINSIGHTS_CONNECTION_STRING}="${appInsightConnectionString}" \
        $METRICS_DB_CONN_STR_KEY="$db_conn_str" \
        $CLOUD_NAME_STR_KEY="$CLOUD_NAME" \
      --output none

  # Configure log streaming for function app
  metrics_api_function_id=$(\
    az functionapp show \
      -n "$METRICS_API_APP_NAME" \
      -g "${RESOURCE_GROUP}" \
      --output tsv \
      --query id)
  update_diagnostic_settings "${metrics_api_function_id}" "${DIAGNOSTIC_SETTINGS_FUNC}"

  # publish metrics function app
  try_run "func azure functionapp publish ${METRICS_API_APP_NAME} --dotnet" 7 "../metrics/src/Piipan.Metrics/${METRICS_API_APP_FILEPATH}"

  script_completed
}
main "$@"
