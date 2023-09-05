#!/usr/bin/env bash
#
# Provisions and configures the infrastructure components for Query Tool.
# Assumes an Azure user with the Contributor role has signed in with the Azure CLI.
# Assumes base resource groups and resources have been created in the same
# environment.
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# usage: create-query-tool-resources.bash <azure-env>

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

  # Create resources for Query Tool
  # This needs to happen after the orchestrator is created
  if [ "${PROVISION_QUERY_TOOL_FD}" = "true" ] ; then
    echo "Create Front Door and WAF policy for Query Tool"
    suffix=$(web_app_host_suffix)
    query_tool_host=${QUERY_TOOL_APP_NAME}${suffix}
    ./add-front-door-to-app.bash \
      "${azure_env}" \
      "${RESOURCE_GROUP}" \
      "${QUERY_TOOL_FRONTDOOR_NAME}" \
      "${QUERY_TOOL_WAF_NAME}" \
      "${query_tool_host}"
  fi

  if [ "${PROVISION_QUERY_TOOL_APP_SERVICE}" = "true" ] ; then
    echo "Creating App Service resources for Query Tool"
    front_door_id=$(\
      az network front-door show \
        --name "${QUERY_TOOL_FRONTDOOR_NAME}" \
        --resource-group "${RESOURCE_GROUP}" \
        --query frontdoorId \
        --output tsv)
    echo "Front Door ID: ${front_door_id}"

    orch_api_uri=$(\
      az functionapp show \
        -g "${MATCH_RESOURCE_GROUP}" \
        -n "${ORCHESTRATOR_FUNC_APP_NAME}" \
        --query defaultHostName \
        --output tsv)
    orch_api_uri="https://${orch_api_uri}/api/v1/"
    orch_api_app_id=$(\
      az ad app list \
        --display-name "${ORCHESTRATOR_FUNC_APP_NAME}" \
        --filter "displayName eq '${ORCHESTRATOR_FUNC_APP_NAME}'" \
        --query "[0].appId" \
        --output tsv)

    match_res_api_uri=$(\
      az functionapp show \
        -g "${MATCH_RESOURCE_GROUP}" \
        -n "${MATCH_RES_FUNC_APP_NAME}" \
        --query defaultHostName \
        --output tsv)
    match_res_api_uri="https://${match_res_api_uri}/api/v1/"
    match_res_api_app_id=$(\
      az ad app list \
        --display-name "${MATCH_RES_FUNC_APP_NAME}" \
        --filter "displayName eq '${MATCH_RES_FUNC_APP_NAME}'" \
        --query "[0].appId" \
        --output tsv)

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

    echo "Deploying ${QUERY_TOOL_APP_NAME} resources"
    az deployment group create \
      --name "${QUERY_TOOL_APP_NAME}" \
      --resource-group "${RESOURCE_GROUP}" \
      --template-file ./arm-templates/query-tool-app.json \
      --parameters \
        location="${LOCATION}" \
        resourceTags="${RESOURCE_TAGS}" \
        appName="${QUERY_TOOL_APP_NAME}" \
        servicePlan="${APP_SERVICE_PLAN}" \
        orchApiUri="${orch_api_uri}" \
        orchApiAppId="${orch_api_app_id}" \
        matchResApiUri="${match_res_api_uri}" \
        matchResApiAppId="${match_res_api_app_id}" \
        statesApiUri="${states_api_uri}" \
        statesApiAppId="${states_api_app_id}" \
        idpOidcConfigUri="${QUERY_TOOL_APP_IDP_OIDC_CONFIG_URI}" \
        idpOidcScopes="${QUERY_TOOL_APP_IDP_OIDC_SCOPES}" \
        idpClientId="${QUERY_TOOL_APP_IDP_CLIENT_ID}" \
        aspNetCoreEnvironment="${QUERY_TOOL_NAME}" \
        frontDoorId="${front_door_id}" \
        frontDoorUri="${QUERY_TOOL_URL}" \
        diagnosticSettingName="${DIAGNOSTIC_SETTINGS_NAME}" \
        eventHubAuthorizationRuleId="${EH_RULE_ID}" \
        eventHubName="${EVENT_HUB_NAME}" \
        maintenanceSlotName="${MAINTENANCE_APP_NAME}" \
        workspaceId="${LOG_ANALYTICS_WORKSPACE_ID}"

    echo "Integrating ${QUERY_TOOL_APP_NAME} into virtual network"
    az webapp vnet-integration add \
      --name "${QUERY_TOOL_APP_NAME}" \
      --resource-group "${RESOURCE_GROUP}" \
      --subnet "${WEBAPP_SUBNET_NAME}" \
      --vnet "${VNET_ID}"

    # Create a placeholder OIDC IdP secret
    create_oidc_secret "${QUERY_TOOL_APP_NAME}"
  fi

  script_completed
}
main "$@"
