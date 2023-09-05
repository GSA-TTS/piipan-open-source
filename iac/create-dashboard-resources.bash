#!/usr/bin/env bash
#
# Provisions and configures the infrastructure components for Dashboard.
# Assumes an Azure user with the Contributor role has signed in with the Azure CLI.
# Assumes base resource groups and resources have been created in the same
# environment.
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# usage: create-dashboard-resources.bash <azure-env>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit

set_constants () {
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

  # Create resources for Dashboard
  if [ "${PROVISION_DASHBOARD_FD}" = "true" ] ; then
    echo "Create Front Door and WAF policy for Dashboard"
    suffix=$(web_app_host_suffix)
    dashboard_host=${DASHBOARD_APP_NAME}${suffix}
    ./add-front-door-to-app.bash \
      "${azure_env}" \
      "${RESOURCE_GROUP}" \
      "${DASHBOARD_FRONTDOOR_NAME}" \
      "${DASHBOARD_WAF_NAME}" \
      "${dashboard_host}"
  fi

  if [ "${PROVISION_DASHBOARD_APP_SERVICE}" = "true" ] ; then
    front_door_id=$(\
    az network front-door show \
      --name "${DASHBOARD_FRONTDOOR_NAME}" \
      --resource-group "${RESOURCE_GROUP}" \
      --query frontdoorId \
      --output tsv)
    echo "Front Door ID: ${front_door_id}"

    metrics_api_hostname=$(\
      az functionapp show \
        -n "${METRICS_API_APP_NAME}" \
        -g "${RESOURCE_GROUP}" \
        --query "defaultHostName" \
        --output tsv)
    metrics_api_uri="https://${metrics_api_hostname}/api/"
    metrics_api_app_id=$(\
      az ad app list \
        --display-name "${METRICS_API_APP_NAME}" \
        --filter "displayName eq '${METRICS_API_APP_NAME}'" \
        --query "[0].appId" \
        --output tsv)

    # Grab states API URI & App ID for use with the Dashboard app
    states_api_uri=$(\
      az functionapp show \
        -g "${RESOURCE_GROUP}" \
        -n "${STATES_FUNC_APP_NAME}" \
        --query defaultHostName \
        --output tsv)
    states_api_uri="https://${states_api_uri}/api/v1/"
    states_api_app_id=$(\
      az ad app list \
        --display-name "${STATES_FUNC_APP_NAME}" \
        --filter "displayName eq '${STATES_FUNC_APP_NAME}'" \
        --query "[0].appId" \
        --output tsv)

    # Create App Service resources for dashboard app
    echo "Creating App Service resources for Dashboard"
    az deployment group create \
      --name "${DASHBOARD_APP_NAME}" \
      --resource-group "${RESOURCE_GROUP}" \
      --template-file ./arm-templates/dashboard-app.json \
      --parameters \
        location="${LOCATION}" \
        resourceTags="${RESOURCE_TAGS}" \
        appName="${DASHBOARD_APP_NAME}" \
        servicePlan="${APP_SERVICE_PLAN}" \
        metricsApiUri="${metrics_api_uri}" \
        metricsApiAppId="${metrics_api_app_id}" \
        idpOidcConfigUri="${DASHBOARD_APP_IDP_OIDC_CONFIG_URI}" \
        idpOidcScopes="${DASHBOARD_APP_IDP_OIDC_SCOPES}" \
        idpClientId="${DASHBOARD_APP_IDP_CLIENT_ID}" \
        aspNetCoreEnvironment="${DASHBOARD_NAME}" \
        frontDoorId="${front_door_id}" \
        frontDoorUri="${DASHBOARD_TOOL_URL}" \
        diagnosticSettingName="${DIAGNOSTIC_SETTINGS_NAME}" \
        eventHubAuthorizationRuleId="${EH_RULE_ID}" \
        eventHubName="${EVENT_HUB_NAME}" \
        maintenanceSlotName="${MAINTENANCE_APP_NAME}" \
        workspaceId="${LOG_ANALYTICS_WORKSPACE_ID}" \
        statesApiUri="${states_api_uri}" \
        statesApiAppId="${states_api_app_id}"

    echo "Integrating ${DASHBOARD_APP_NAME} into virtual network"
    az webapp vnet-integration add \
      --name "${DASHBOARD_APP_NAME}" \
      --resource-group "${RESOURCE_GROUP}" \
      --subnet "${WEBAPP_SUBNET_NAME}" \
      --vnet "${VNET_ID}"

    create_oidc_secret "$DASHBOARD_APP_NAME"
  fi

  script_completed
}
main "$@"
