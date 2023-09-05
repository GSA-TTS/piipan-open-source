#!/usr/bin/env bash
#
# Provisions and configures the infrastructure components for all Piipan
# subsystems. Assumes an Azure user with the Global Administrator role
# has signed in with the Azure CLI. Must be run from a trusted network.
# See install-extensions.bash for prerequisite Azure CLI extensions.
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# usage: create-resources.bash <azure-env>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit

set_constants () {
  # TODO: Move to create-databases.bash, along with related database code
  # Name of secret used to store the PostgreSQL server admin password
  PG_SECRET_NAME=particpants-records-admin
  # Name of administrator login for PostgreSQL server
  PG_SUPERUSER=postgres
  # Name of PostgreSQL server
  PG_SERVER_NAME=${PREFIX}-psql-participants-${ENV}

  set_defaults
}

# Generate the storage account connection string for the corresponding
# blob storage account.
# XXX Uses the secondary access key (aka `key2`) for internal access, reserving
#     the primary access key (aka `key1`) for state access. Improve by replacing
#     with share access signatures (SAS URLs) via managed identities at runtime.
blob_connection_string () {
  resource_group=$1
  name=$2

  az storage account show-connection-string \
    --key secondary \
    --resource-group "$resource_group" \
    --name "$name" \
    --query connectionString \
    --output tsv
}

# From a managed identity name, generate the value for
# AzureServicesAuthConnectionString
az_connection_string () {
  resource_group=$1
  identity=$2

  configure_azure_profile
  client_id=$(\
    az identity show \
      --resource-group "$resource_group" \
      --name "$identity" \
      --query clientId \
      --output tsv)

  echo "RunAs=App;AppId=${client_id}"
  configure_azure_profile
}

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1

  # Configure Azure CLI warnings
  # Note: If warnings are disabled, az login will not show output
  az config set core.only_show_errors=false
  read -p "Disable Azure CLI warnings? (Yes or No) " -r -t 10 &&
  if [[ ${REPLY} =~ ^[yY]es$ ]]; then
    echo "Disabling Azure CLI warnings"
    az config set core.only_show_errors=true
  else
    echo "Showing Azure CLI warnings"
  fi

  # shellcheck source=./iac/env/tts/dev.bash
  source "$(dirname "$0")"/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/iac-common.bash
  verify_cloud
  setup_enviroment "$(dirname "$0")"/env/"${azure_env}"
  verify_file "env/${azure_env}/" "states.csv"
  set_constants

  echo "Creating Resource Groups"
  ./create-resource-groups.bash "$azure_env"

  # Create Log Analytics workspace
  echo "Creating Log Analytics workspace"
  az deployment group create \
   --name "${LOG_ANALYTICS_WORKSPACE_NAME}" \
   --resource-group "${RESOURCE_GROUP}" \
   --template-file ./arm-templates/log-analytics-workspace.json \
   --parameters \
     diagnosticSettingName="${DIAGNOSTIC_SETTINGS_NAME}" \
     eventHubAuthorizationRuleId="${EH_RULE_ID}" \
     eventHubName="${EVENT_HUB_NAME}" \
     resourceTags="${RESOURCE_TAGS}" \
     retentionInDays="${LOG_ANALYTICS_RETENTION_DAYS}" \
     workspaceId="${LOG_ANALYTICS_WORKSPACE_ID}" \
     workspaceName="${LOG_ANALYTICS_WORKSPACE_NAME}"

  # Create Event Hub
  ./create-event-hub.bash "${azure_env}"

  # Virtual network is used to secure connections between
  # participant records database and all apps that communicate with it.
  # Apps will be integrated with VNet as they're created.
  echo "Creating Virtual Network and Subnets"
  az deployment group create \
    --name "$VNET_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --template-file ./arm-templates/virtual-network.json \
    --parameters \
      location="$LOCATION" \
      vnetName="$VNET_NAME" \
      peParticipantsSubnetName="$DB_SUBNET_NAME" \
      peCoreSubnetName="$DB_2_SUBNET_NAME" \
      funcAppServicePlanSubnetName="$FUNC_SUBNET_NAME" \
      funcAppServicePlanNsgName="$FUNC_NSG_NAME" \
      webAppServicePlanSubnetName="$WEBAPP_SUBNET_NAME" \
      webAppServicePlanNsgName="$WEBAPP_NSG_NAME" \
      emailSubnetName="${EMAIL_SUBNET_NAME}" \
      emailAppServicePlanNsgName="${EMAIL_APP_NSG_NAME}" \
      idpOidcIpRanges="$IDP_OIDC_IP_RANGES" \
      diagnosticSettingName="${DIAGNOSTIC_SETTINGS_NAME}" \
      eventHubAuthorizationRuleId="${EH_RULE_ID}" \
      eventHubName="${EVENT_HUB_NAME}" \
      resourceTags="${RESOURCE_TAGS}" \
      workspaceId="${LOG_ANALYTICS_WORKSPACE_ID}"

  # Several CLI commands use the vnet resource ID
  VNET_ID=$(get_vnet)

  # Create a key vault which will store credentials for use in other templates
  az deployment group create \
    --name "${VAULT_NAME}" \
    --resource-group "${RESOURCE_GROUP}" \
    --template-file ./arm-templates/key-vault.json \
    --parameters \
      name="${VAULT_NAME}" \
      location="${LOCATION}" \
      diagnosticSettingName="${DIAGNOSTIC_SETTINGS_NAME}" \
      eventHubAuthorizationRuleId="${EH_RULE_ID}" \
      eventHubName="${EVENT_HUB_NAME}" \
      objectId="${CURRENT_USER_OBJID}" \
      resourceTags="${RESOURCE_TAGS}" \
      workspaceId="${LOG_ANALYTICS_WORKSPACE_ID}"

  # Configure Activity Logs and Alerts
  # Activity Log is subscription-wide, so it's only needed for one environment,
  # not every environment that is nested in the subscription.
  if [ "${CONFIGURE_ACTIVITY_LOG}" = "true" ] ; then
    ./security/configure-activity-log.bash "$azure_env"
  fi

  # For each participating state, create a separate storage account.
  # Each account has a blob storage container named `upload`.
  while IFS=$',\t\r\n' read -r abbr name enable_matches; do
      abbr=$(echo "$abbr" | tr '[:upper:]' '[:lower:]')
      func_stor_name=${PREFIX}st${abbr}upload${ENV}
      echo "Creating storage for $name ($func_stor_name)"
      az deployment group create \
      --name "${func_stor_name}" \
      --resource-group "${RESOURCE_GROUP}" \
      --template-file ./arm-templates/blob-storage.json \
      --parameters \
        storageAccountName="${func_stor_name}" \
        resourceTags="${RESOURCE_TAGS}" \
        location="${LOCATION}" \
        diagnosticSettingName="${DIAGNOSTIC_SETTINGS_NAME}" \
        eventHubAuthorizationRuleId="${EH_RULE_ID}" \
        eventHubName="${EVENT_HUB_NAME}" \
        sku="${STORAGE_SKU}" \
        subnet="${FUNC_SUBNET_NAME}" \
        vnet="${VNET_ID}" \
        workspaceId="${LOG_ANALYTICS_WORKSPACE_ID}"

  done < env/"${azure_env}"/states.csv

  # Avoid echoing passwords in a manner that may show up in process listing,
  # or storing it in a temp file that may be read, or appearing in a CI/CD log.
  #
  # By default, Azure CLI will print the password set in Key Vault; instead
  # just extract and print the secret id from the JSON response.
  PG_SECRET=$(random_password)
  export PG_SECRET
  printenv PG_SECRET | tr -d '\n' | az keyvault secret set \
    --vault-name "$VAULT_NAME" \
    --name "$PG_SECRET_NAME" \
    --file /dev/stdin \
    --query id
    #--value "$PG_SECRET"

  echo "Creating PostgreSQL server"
  az deployment group create \
    --name participant-records \
    --resource-group "$RESOURCE_GROUP" \
    --template-file ./arm-templates/participant-records.json \
    --parameters \
      administratorLogin=$PG_SUPERUSER \
      serverName="$PG_SERVER_NAME" \
      secretName="$PG_SECRET_NAME" \
      vaultName="$VAULT_NAME" \
      resourceTags="$RESOURCE_TAGS" \
      vnetName="$VNET_NAME" \
      subnetName="$DB_SUBNET_NAME" \
      privateEndpointName="$PRIVATE_ENDPOINT_NAME" \
      privateDnsZoneName="$PRIVATE_DNS_ZONE" \
      diagnosticSettingName="${DIAGNOSTIC_SETTINGS_NAME}" \
      eventHubAuthorizationRuleId="${EH_RULE_ID}" \
      eventHubName="${EVENT_HUB_NAME}" \
      workspaceId="${LOG_ANALYTICS_WORKSPACE_ID}"

  # The AD admin can't be specified in the PostgreSQL ARM template,
  # unlike in Azure SQL
  PG_AAD_ADMIN_OBJID=$(\
    az ad group list \
      --display-name "${PG_AAD_ADMIN}" \
      --query "id" \
      --output tsv)
  if [ -z "${PG_AAD_ADMIN_OBJID}" ]; then
    PG_AAD_ADMIN_OBJID=$(\
      az ad group create \
        --display-name "${PG_AAD_ADMIN}" \
        --mail-nickname "${PG_AAD_ADMIN}" \
        --query "id" \
        --output tsv)
    sleep 60
  fi

  az postgres server ad-admin create \
    --resource-group "$RESOURCE_GROUP" \
    --server "$PG_SERVER_NAME" \
    --display-name "$PG_AAD_ADMIN" \
    --object-id "$PG_AAD_ADMIN_OBJID"

  # Configure Payload Keys
  ./configure-encryption-secrets.bash "$azure_env"

  # Create managed identities to admin each state's database
  configure_azure_profile
  while IFS=$',\t\r\n' read -r abbr name enable_matches; do
      echo "Creating managed identity for $name ($abbr)"
      abbr=$(echo "$abbr" | tr '[:upper:]' '[:lower:]')
      identity=$(state_managed_id_name "$abbr" "$ENV")
      az identity create -g "$RESOURCE_GROUP" -n "$identity"
  done < env/"${azure_env}"/states.csv
  configure_azure_profile

  exists=$(az ad group member check \
    --group "$PG_AAD_ADMIN" \
    --member-id "$CURRENT_USER_OBJID" \
    --query value --output tsv)

  if [ "$exists" = "true" ]; then
    echo "$CURRENT_USER_OBJID is already a member of $PG_AAD_ADMIN"
  else
    # Temporarily add current user as a PostgreSQL AD admin
    # to allow provisioning of managed identity roles
    az ad group member add \
      --group "$PG_AAD_ADMIN" \
      --member-id "$CURRENT_USER_OBJID"
  fi

  PGPASSWORD=$PG_SECRET
  export PGPASSWORD
  PGUSER=${PG_SUPERUSER}@${PG_SERVER_NAME}
  export PGUSER
  PGHOST=$(az resource show \
    --resource-group "$RESOURCE_GROUP" \
    --name "$PG_SERVER_NAME" \
    --resource-type "Microsoft.DbForPostgreSQL/servers" \
    --query properties.fullyQualifiedDomainName --output tsv)
  export PGHOST
  export ENV
  export PREFIX
  # Multiple PostgreSQL databases cannot be created with an ARM template;
  # detailed database/schema/role configuration can't be done with an ARM
  # template either. Instead, we access the PostgreSQL server from a trusted
  # network (as established by its ARM template firewall variable), and apply
  # various Data Definition (DDL) scripts for each state.
  ./create-databases.bash "$RESOURCE_GROUP" "$azure_env"

  # Apply DDL shared between the ETL and match API subsystems.
  # XXX This should be moved out of IaC, which is not run in CI/CD,
  #     to a continuously deployable workflow that accomodates schema
  #     changes over time.
  pushd ../ddl
  ./apply-ddl.bash "$azure_env"
  popd

  # This is a subscription-level resource provider
  az provider register --wait --namespace Microsoft.EventGrid

  # Function apps need an app service plan with private endpoint abilities
  echo "Creating app service plan ${APP_SERVICE_PLAN_FUNC_NAME}"
  az deployment group create \
    --name "$APP_SERVICE_PLAN_FUNC_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --template-file ./arm-templates/app-service-plan.json \
    --parameters \
      name="$APP_SERVICE_PLAN_FUNC_NAME" \
      location="$LOCATION" \
      kind="$APP_SERVICE_PLAN_FUNC_KIND" \
      sku="$APP_SERVICE_PLAN_FUNC_SKU" \
      resourceTags="$RESOURCE_TAGS"

    # Function apps need an app service plan with private endpoint abilities
  echo "Creating app service plan ${EMAIL_APP_SERVICE_PLAN_NAME}"
  az deployment group create \
    --name "${EMAIL_APP_SERVICE_PLAN_NAME}" \
    --resource-group "${RESOURCE_GROUP}" \
    --template-file ./arm-templates/app-service-plan.json \
    --parameters \
      name="${EMAIL_APP_SERVICE_PLAN_NAME}" \
      location="${LOCATION}" \
      kind="${APP_SERVICE_PLAN_FUNC_KIND}" \
      sku="${EMAIL_APP_SERVICE_PLAN_FUNC_SKU}" \
      resourceTags="${RESOURCE_TAGS}"

  # Create the list of state abbreviations, and which states should be disabled from
  # returning matches from the orchestrator API.
  state_abbrs=""
  state_enabled_matches=""
  while IFS=$',\t\r\n' read -r abbr name enable_matches; do
    abbr=$(echo "$abbr" | tr '[:upper:]' '[:lower:]')
    state_abbrs+=",${abbr}"
    if [ "$enable_matches" = $STATE_ENABLED_KEY ]; then
        state_enabled_matches+=",${abbr}"
    fi
  done < env/"${azure_env}"/states.csv
  state_abbrs=${state_abbrs:1}
  if [[ -n "$state_enabled_matches" ]]; then
    state_enabled_matches=${state_enabled_matches:1}
  fi
  echo "Enabled States: ${state_enabled_matches}"

  # Create Notify resources
  ./create-notify-resources.bash "${azure_env}"

  # Create orchestrator-level Function app using ARM template and
  # deploy project code using functions core tools. Networking
  # restrictions for the function app and storage account are added
  # in a separate step to avoid deployment and publishing issues.
  db_conn_str=$(pg_connection_string "$PG_SERVER_NAME" "$DATABASE_PLACEHOLDER" "$ORCHESTRATOR_FUNC_APP_NAME")
  # Given that the entire max connections for the Participants database is around 150, we need to divide this up between all databases.
  # Going with 15 for now as that seems to be about right, but we can always adjust as needed.
  db_conn_str="${db_conn_str};MaxPoolSize=15;"
  collab_db_conn_str=$(pg_connection_string "$CORE_DB_SERVER_NAME" "$COLLAB_DB_NAME" "$ORCHESTRATOR_FUNC_APP_NAME")
  az deployment group create \
    --name orch-api \
    --resource-group "$MATCH_RESOURCE_GROUP" \
    --template-file  ./arm-templates/function-orch-match.json \
    --parameters \
      resourceTags="$RESOURCE_TAGS" \
      location="$LOCATION" \
      functionAppName="$ORCHESTRATOR_FUNC_APP_NAME" \
      appServicePlanName="$APP_SERVICE_PLAN_FUNC_NAME" \
      storageAccountName="$ORCHESTRATOR_FUNC_APP_STORAGE_NAME" \
      participantsDatabaseConnectionString="$db_conn_str" \
      collabDatabaseConnectionString="$collab_db_conn_str" \
      cloudName="$CLOUD_NAME" \
      states="$state_abbrs" \
      coreResourceGroup="$RESOURCE_GROUP" \
      statesToEnableMatches="$state_enabled_matches" \
      diagnosticSettingName="${DIAGNOSTIC_SETTINGS_NAME}" \
      eventHubAuthorizationRuleId="${EH_RULE_ID}" \
      eventHubName="${EVENT_HUB_NAME}" \
      sku="${STORAGE_SKU}" \
      subnet="${FUNC_SUBNET_NAME}" \
      vnet="${VNET_ID}" \
      workspaceId="${LOG_ANALYTICS_WORKSPACE_ID}"

  echo "Integrating ${ORCHESTRATOR_FUNC_APP_NAME} into virtual network"
  az functionapp vnet-integration add \
    --name "$ORCHESTRATOR_FUNC_APP_NAME" \
    --resource-group "$MATCH_RESOURCE_GROUP" \
    --subnet "$FUNC_SUBNET_NAME" \
    --vnet "$VNET_ID"

  echo "Removing public access for ${ORCHESTRATOR_FUNC_APP_NAME}"
  az storage account update \
    --name "${ORCHESTRATOR_FUNC_APP_STORAGE_NAME}" \
    --resource-group "${MATCH_RESOURCE_GROUP}" \
    --default-action "Deny"

  # Update Key Vault to allow function access
  echo "Granting Key Vault access for ${ORCHESTRATOR_FUNC_APP_NAME}"
  funcIdentityPrincipalId=$(\
    az functionapp identity show \
    --name "${ORCHESTRATOR_FUNC_APP_NAME}" \
    --resource-group "${MATCH_RESOURCE_GROUP}" \
    --query principalId \
    --output tsv)

  az deployment group create \
    --name "${VAULT_NAME}-access-for-${ORCHESTRATOR_FUNC_APP_NAME}" \
    --resource-group "${RESOURCE_GROUP}" \
    --template-file ./arm-templates/key-vault-access-policy.json \
    --parameters \
      keyVaultName="${VAULT_NAME}" \
      objectId="${funcIdentityPrincipalId}" \
      permissionsSecrets="['get', 'list']"

  # Create Event Grid Topics (does not include System Topics)
  ./create-event-grid-topics.bash "${azure_env}"

  # Match - Event Grid properties
  eventgrid_match_metrics_endpoint=$(eventgrid_endpoint "${RESOURCE_GROUP}" "${MATCH_METRICS_TOPIC_NAME}")
  eventgrid_match_metrics_key_string=$(eventgrid_key_string "${RESOURCE_GROUP}" "${MATCH_METRICS_TOPIC_NAME}")
  # Search - Event Grid properties
  eventgrid_search_endpoint=$(eventgrid_endpoint "${RESOURCE_GROUP}" "${SEARCH_METRICS_TOPIC_NAME}")
  eventgrid_search_key_string=$(eventgrid_key_string "${RESOURCE_GROUP}" "${SEARCH_METRICS_TOPIC_NAME}")
  # Notificaction - Event Grid properties
  eventgrid_notify_endpoint=$(eventgrid_endpoint "${RESOURCE_GROUP}" "${NOTIFY_TOPIC_NAME}")
  eventgrid_notify_key_string=$(eventgrid_key_string "${RESOURCE_GROUP}" "${NOTIFY_TOPIC_NAME}")

  echo "Update ${ORCHESTRATOR_FUNC_APP_NAME} settings"
  az functionapp config appsettings set \
    --name "$ORCHESTRATOR_FUNC_APP_NAME" \
    --resource-group "$MATCH_RESOURCE_GROUP" \
    --settings \
      ${COLUMN_ENCRYPT_KEY}="@Microsoft.KeyVault(VaultName=${VAULT_NAME};SecretName=${COLUMN_ENCRYPT_KEY_KV})" \
      ${EVENTGRID_CONN_METRICS_MATCH_STR_ENDPOINT}="${eventgrid_match_metrics_endpoint}" \
      ${EVENTGRID_CONN_METRICS_MATCH_STR_KEY}="${eventgrid_match_metrics_key_string}" \
      ${EVENTGRID_CONN_METRICS_SEARCH_STR_ENDPOINT}="${eventgrid_search_endpoint}" \
      ${EVENTGRID_CONN_METRICS_SEARCH_STR_KEY}="${eventgrid_search_key_string}" \
      ${EVENTGRID_CONN_NOTIFY_STR_ENDPOINT}="${eventgrid_notify_endpoint}" \
      ${EVENTGRID_CONN_NOTIFY_STR_KEY}="${eventgrid_notify_key_string}" \
      QueryToolUrl="${QUERY_TOOL_DISPLAY_URL}" \
      SmtpReplyToEmail="${SMTP_REPLYTO_EMAIL}" \
      WEBSITE_CONTENTOVERVNET=1 \
      WEBSITE_VNET_ROUTE_ALL=1

  # Publish function app
  try_run "func azure functionapp publish ${ORCHESTRATOR_FUNC_APP_NAME} --dotnet" 7 "../match/src/Piipan.Match/Piipan.Match.Func.Api"

  # Create an Active Directory app registration associated with the app.
  # Used by subsequent resources to configure auth
  az ad app create \
    --display-name "$ORCHESTRATOR_FUNC_APP_NAME" \
    --sign-in-audience "AzureADMyOrg"

  ./config-managed-role.bash "$ORCHESTRATOR_FUNC_APP_NAME" "$MATCH_RESOURCE_GROUP" "${PG_AAD_ADMIN}@${PG_SERVER_NAME}"

  # Create Match Resolution API Function App
  echo "Create Match Resolution API Function App"
  collab_db_conn_str=$(pg_connection_string "$CORE_DB_SERVER_NAME" "$COLLAB_DB_NAME" "$MATCH_RES_FUNC_APP_NAME")
  az deployment group create \
    --name match-res-api \
    --resource-group "$MATCH_RESOURCE_GROUP" \
    --template-file  ./arm-templates/function.json \
    --parameters \
      resourceTags="$RESOURCE_TAGS" \
      location="$LOCATION" \
      functionAppName="$MATCH_RES_FUNC_APP_NAME" \
      appServicePlanName="$APP_SERVICE_PLAN_FUNC_NAME" \
      storageAccountName="$MATCH_RES_FUNC_APP_STORAGE_NAME" \
      collabDatabaseConnectionString="$collab_db_conn_str" \
      cloudName="$CLOUD_NAME" \
      states="$state_abbrs" \
      coreResourceGroup="$RESOURCE_GROUP" \
      diagnosticSettingName="${DIAGNOSTIC_SETTINGS_NAME}" \
      enabledStates="$state_enabled_matches" \
      eventHubAuthorizationRuleId="${EH_RULE_ID}" \
      eventHubName="${EVENT_HUB_NAME}" \
      sku="${STORAGE_SKU}" \
      subnet="${FUNC_SUBNET_NAME}" \
      systemTypeTag="${MATCH_RES_API_SYSTEM_TAG}" \
      vnet="${VNET_ID}" \
      workspaceId="${LOG_ANALYTICS_WORKSPACE_ID}"

  echo "Integrating ${MATCH_RES_FUNC_APP_NAME} into virtual network"
  az functionapp vnet-integration add \
    --name "$MATCH_RES_FUNC_APP_NAME" \
    --resource-group "$MATCH_RESOURCE_GROUP" \
    --subnet "$FUNC_SUBNET_NAME" \
    --vnet "$VNET_ID"

  echo "Removing public access for ${MATCH_RES_FUNC_APP_NAME}"
  az storage account update \
    --name "${MATCH_RES_FUNC_APP_STORAGE_NAME}" \
    --resource-group "${MATCH_RESOURCE_GROUP}" \
    --default-action "Deny"

  # Update Key Vault to allow function access
  echo "Granting Key Vault access for ${MATCH_RES_FUNC_APP_NAME}"
  funcIdentityPrincipalId=$(\
    az functionapp identity show \
    --name "${MATCH_RES_FUNC_APP_NAME}" \
    --resource-group "${MATCH_RESOURCE_GROUP}" \
    --query principalId \
    --output tsv)

  az deployment group create \
    --name "${VAULT_NAME}-access-for-${MATCH_RES_FUNC_APP_NAME}" \
    --resource-group "${RESOURCE_GROUP}" \
    --template-file ./arm-templates/key-vault-access-policy.json \
    --parameters \
      keyVaultName="${VAULT_NAME}" \
      objectId="${funcIdentityPrincipalId}" \
      permissionsSecrets="['get', 'list']"

  echo "Update ${MATCH_RES_FUNC_APP_NAME} settings"
  az functionapp config appsettings set \
    --name "$MATCH_RES_FUNC_APP_NAME" \
    --resource-group "$MATCH_RESOURCE_GROUP" \
    --settings \
      ${COLUMN_ENCRYPT_KEY}="@Microsoft.KeyVault(VaultName=${VAULT_NAME};SecretName=${COLUMN_ENCRYPT_KEY_KV})" \
      QueryToolUrl="${QUERY_TOOL_DISPLAY_URL}" \
      SmtpReplyToEmail="${SMTP_REPLYTO_EMAIL}" \
      WEBSITE_CONTENTOVERVNET=1 \
      WEBSITE_VNET_ROUTE_ALL=1 \
      ${EVENTGRID_CONN_METRICS_MATCH_STR_ENDPOINT}="${eventgrid_match_metrics_endpoint}" \
      ${EVENTGRID_CONN_METRICS_MATCH_STR_KEY}="${eventgrid_match_metrics_key_string}" \
      ${EVENTGRID_CONN_NOTIFY_STR_ENDPOINT}="${eventgrid_notify_endpoint}" \
      ${EVENTGRID_CONN_NOTIFY_STR_KEY}="${eventgrid_notify_key_string}" \

  echo "Publish Match Resolution API Function App"
  try_run "func azure functionapp publish ${MATCH_RES_FUNC_APP_NAME} --dotnet" 7 "../match/src/Piipan.Match/Piipan.Match.Func.ResolutionApi"

  # Create an Active Directory app registration associated with the app.
  # Used by subsequent resources to configure auth
  az ad app create \
    --display-name "$MATCH_RES_FUNC_APP_NAME" \
    --sign-in-audience "AzureADMyOrg"

  # Create States API Function App
  echo "Create States API Function App"
  collab_db_conn_str=$(pg_connection_string "$CORE_DB_SERVER_NAME" "$COLLAB_DB_NAME" "$STATES_FUNC_APP_NAME")
  az deployment group create \
    --name state-api \
    --resource-group "$RESOURCE_GROUP" \
    --template-file  ./arm-templates/function.json \
    --parameters \
      resourceTags="$RESOURCE_TAGS" \
      location="$LOCATION" \
      functionAppName="$STATES_FUNC_APP_NAME" \
      appServicePlanName="$APP_SERVICE_PLAN_FUNC_NAME" \
      storageAccountName="$STATES_FUNC_APP_STORAGE_NAME" \
      collabDatabaseConnectionString="$collab_db_conn_str" \
      cloudName="$CLOUD_NAME" \
      coreResourceGroup="$RESOURCE_GROUP" \
      diagnosticSettingName="${DIAGNOSTIC_SETTINGS_NAME}" \
      eventHubAuthorizationRuleId="${EH_RULE_ID}" \
      eventHubName="${EVENT_HUB_NAME}" \
      sku="${STORAGE_SKU}" \
      states="" \
      enabledStates="" \
      subnet="${FUNC_SUBNET_NAME}" \
      systemTypeTag="${STATES_API_SYSTEM_TAG}" \
      vnet="${VNET_ID}" \
      workspaceId="${LOG_ANALYTICS_WORKSPACE_ID}"

  echo "Integrating ${STATES_FUNC_APP_NAME} into virtual network"
  az functionapp vnet-integration add \
    --name "$STATES_FUNC_APP_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --subnet "$FUNC_SUBNET_NAME" \
    --vnet "$VNET_ID"

  echo "Removing public access for ${STATES_FUNC_APP_NAME}"
  az storage account update \
    --name "${STATES_FUNC_APP_STORAGE_NAME}" \
    --resource-group "${RESOURCE_GROUP}" \
    --default-action "Deny"

  # Update Key Vault to allow function access
  echo "Granting Key Vault access for ${STATES_FUNC_APP_NAME}"
  funcIdentityPrincipalId=$(\
    az functionapp identity show \
    --name "${STATES_FUNC_APP_NAME}" \
    --resource-group "${RESOURCE_GROUP}" \
    --query principalId \
    --output tsv)

  az deployment group create \
    --name "${VAULT_NAME}-access-for-${STATES_FUNC_APP_NAME}" \
    --resource-group "${RESOURCE_GROUP}" \
    --template-file ./arm-templates/key-vault-access-policy.json \
    --parameters \
      keyVaultName="${VAULT_NAME}" \
      objectId="${funcIdentityPrincipalId}" \
      permissionsSecrets="['get', 'list']"

  echo "Update ${STATES_FUNC_APP_NAME} settings"
  az functionapp config appsettings set \
    --name "$STATES_FUNC_APP_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --settings \
      ${COLUMN_ENCRYPT_KEY}="@Microsoft.KeyVault(VaultName=${VAULT_NAME};SecretName=${COLUMN_ENCRYPT_KEY_KV})" \
      WEBSITE_CONTENTOVERVNET=1 \
      WEBSITE_VNET_ROUTE_ALL=1

  echo "Publish States API Function App"
  try_run "func azure functionapp publish ${STATES_FUNC_APP_NAME} --dotnet" 7 "../states/src/Piipan.States.Func.Api"

  # Create an Active Directory app registration associated with the app.
  # Used by subsequent resources to configure auth
  az ad app create \
    --display-name "$STATES_FUNC_APP_NAME" \
    --sign-in-audience "AzureADMyOrg"

  # Create Support Tools API Function App
  echo "Create Support Tools API Function App"
  
  az deployment group create \
    --name support-tools-api \
    --resource-group "$RESOURCE_GROUP" \
    --template-file  ./arm-templates/function.json \
    --parameters \
      resourceTags="$RESOURCE_TAGS" \
      location="$LOCATION" \
      functionAppName="${SUPPORT_TOOLS_FUNC_APP_NAME}" \
      appServicePlanName="$APP_SERVICE_PLAN_FUNC_NAME" \
      storageAccountName="${SUPPORT_TOOLS_FUNC_APP_STORAGE_NAME}" \
      collabDatabaseConnectionString="" \
      cloudName="$CLOUD_NAME" \
      coreResourceGroup="$RESOURCE_GROUP" \
      diagnosticSettingName="${DIAGNOSTIC_SETTINGS_NAME}" \
      eventHubAuthorizationRuleId="${EH_RULE_ID}" \
      eventHubName="${EVENT_HUB_NAME}" \
      sku="${STORAGE_SKU}" \
      states="" \
      enabledStates="" \
      subnet="${FUNC_SUBNET_NAME}" \
      systemTypeTag="${SUPPORT_TOOLS_API_SYSTEM_TAG}" \
      vnet="${VNET_ID}" \
      workspaceId="${LOG_ANALYTICS_WORKSPACE_ID}"

  echo "Integrating ${SUPPORT_TOOLS_FUNC_APP_NAME} into virtual network"
  az functionapp vnet-integration add \
    --name "${SUPPORT_TOOLS_FUNC_APP_NAME}" \
    --resource-group "$RESOURCE_GROUP" \
    --subnet "$FUNC_SUBNET_NAME" \
    --vnet "$VNET_ID"

  echo "Removing public access for ${SUPPORT_TOOLS_FUNC_APP_NAME}"
  az storage account update \
    --name "${SUPPORT_TOOLS_FUNC_APP_STORAGE_NAME}" \
    --resource-group "${RESOURCE_GROUP}" \
    --default-action "Deny"

  echo "Update ${SUPPORT_TOOLS_FUNC_APP_NAME} settings"
  az functionapp config appsettings set \
    --name "${SUPPORT_TOOLS_FUNC_APP_NAME}" \
    --resource-group "$RESOURCE_GROUP" \
    --settings \
      WEBSITE_CONTENTOVERVNET=1 \
      WEBSITE_VNET_ROUTE_ALL=1

  echo "Publish Support Tools API Function App"
  try_run "func azure functionapp publish ${SUPPORT_TOOLS_FUNC_APP_NAME} --dotnet" 7 "../support-tools/src/Piipan.SupportTools.Func.Api"

  # Create an Active Directory app registration associated with the app.
  # Used by subsequent resources to configure auth
  az ad app create \
    --display-name "${SUPPORT_TOOLS_FUNC_APP_NAME}" \
    --sign-in-audience "AzureADMyOrg"

  if [ "$exists" = "true" ]; then
    echo "Leaving $CURRENT_USER_OBJID as a member of $PG_AAD_ADMIN"
  else
    # Revoke temporary assignment of current user as a PostgreSQL AD admin
    az ad group member remove \
      --group "$PG_AAD_ADMIN" \
      --member-id "$CURRENT_USER_OBJID"
  fi

  # Create per-state Function apps and assign corresponding managed identity for
  # access to the per-state blob-storage and database, set up system topics and
  # event subscription to bulk upload (blob creation) events
  while IFS=$',\t\r\n' read -r abbr name enable_matches; do
    echo "Creating function app for $name ($abbr)"
    abbr=$(echo "$abbr" | tr '[:upper:]' '[:lower:]')

    # Per-state Function App
    func_app=$PREFIX-func-${abbr}etl-$ENV

    # Storage account for the Function app for its own use;
    func_stor=${PREFIX}st${abbr}etl${ENV}

    # Managed identity to access database
    identity=$(state_managed_id_name "$abbr" "$ENV")

    # Per-state database
    db_name=${abbr}

    #TODO: Move the following variables to a common file
    # Per-state storage account for bulk upload;
    # matches name passed to blob-storage.json
    stor_name="${PREFIX}st${abbr}upload${ENV}"

    # System topic for per-state upload (create blob) events
    # same as topic name in create-metrics-resources.bash
    topic_name="evgt-${abbr}-upload-${ENV}"

    # Event Grid System Topic Subscription, used to upload events that
    # get routed to a storage queue
    sub_name="evgs-${abbr}-upload"

    # Every Function app needs a storage account for its own internal use;
    # e.g., bindings state, keys, function code. Keep this separate from
    # the storage account used to upload data for better isolation.
    az deployment group create \
      --name "${func_stor}" \
      --resource-group "${RESOURCE_GROUP}" \
      --template-file ./arm-templates/function-storage.json \
      --parameters \
        storageAccountName="${func_stor}" \
        location="${LOCATION}" \
        resourceTags="${RESOURCE_TAGS}" \
        diagnosticSettingName="${DIAGNOSTIC_SETTINGS_NAME}" \
        eventHubAuthorizationRuleId="${EH_RULE_ID}" \
        eventHubName="${EVENT_HUB_NAME}" \
        sku="${STORAGE_SKU}" \
        subnet="${FUNC_SUBNET_NAME}" \
        vnet="${VNET_ID}" \
        workspaceId="${LOG_ANALYTICS_WORKSPACE_ID}"

    # Even though the OS *should* be abstracted away at the Function level, Azure
    # portal has oddities/limitations when using Linux -- lets just get it
    # working with Windows as underlying OS
    #
    # TODO: The following functionapp code should be migrated to ARM
    az functionapp create \
      --resource-group "$RESOURCE_GROUP" \
      --plan "$APP_SERVICE_PLAN_FUNC_NAME" \
      --tags Project="$PROJECT_TAG" "$PER_STATE_ETL_TAG" \
      --runtime dotnet \
      --functions-version 4 \
      --os-type Windows \
      --name "$func_app" \
      --storage-account "$func_stor" \
      --disable-app-insights true \
      --https-only true

    # Integrate function app into Virtual Network
    echo "Integrating ${func_app} into virtual network"
    az functionapp vnet-integration add \
      --name "$func_app" \
      --resource-group "$RESOURCE_GROUP" \
      --subnet "$FUNC_SUBNET_NAME" \
      --vnet "$VNET_NAME"

    echo "Configure: ${func_app}"
    az functionapp config set \
      --name "${func_app}" \
      --resource-group "${RESOURCE_GROUP}" \
      --always-on true \
      --ftps-state "Disabled" \
      --min-tls-version "1.2" \
      --vnet-route-all-enabled true

    echo "Removing public access for ${func_stor}"
    az storage account update \
      --name "${func_stor}" \
      --resource-group "${RESOURCE_GROUP}" \
      --default-action "Deny"

    # Stream Event Grid topic diagnostic logs to Event Hub / Log Analytisc workspace
    func_id=$(\
      az functionapp show \
        -n "$func_app" \
        -g "$RESOURCE_GROUP" \
        --output tsv \
        --query id)
    update_diagnostic_settings "${func_id}" "${DIAGNOSTIC_SETTINGS_FUNC}"

    # XXX Assumes if any identity is set, it is the one we are specifying below
    exists=$(az functionapp identity show \
      --resource-group "$RESOURCE_GROUP" \
      --name "$func_app")

    stateManagedIdentity="${DEFAULT_PROVIDERS}/Microsoft.ManagedIdentity/userAssignedIdentities/${identity}"
    if [ -z "$exists" ]; then
      # Conditionally execute otherwise we will get an error if it is already
      # assigned this managed identity
      az functionapp identity assign \
        --resource-group "${RESOURCE_GROUP}" \
        --name "${func_app}" \
        --identities "${stateManagedIdentity}"
    fi

    # Update function to use state managed identity when referencing key vault
    echo "Configuring ${func_app} Key Vault Reference Identity"
    az rest --method PATCH --uri "${func_id}?api-version=2021-01-01" --body "{'properties':{'keyVaultReferenceIdentity':'${stateManagedIdentity}'}}"

    # Update Key Vault to allow function access
    echo "Granting Key Vault access to ${func_app}"
    configure_azure_profile
    stateManagedIdentityPrincipalId=$(\
      az identity show \
        --resource-group "${RESOURCE_GROUP}" \
        --name "${identity}" \
        --query principalId \
        --output tsv)
    configure_azure_profile

    az deployment group create \
      --name "${VAULT_NAME}-access-for-${func_app}" \
      --resource-group "${RESOURCE_GROUP}" \
      --template-file ./arm-templates/key-vault-access-policy.json \
      --parameters \
        keyVaultName="${VAULT_NAME}" \
        objectId="${stateManagedIdentityPrincipalId}" \
        permissionsSecrets="['get', 'list']"

    echo "Creating Application Insight for function: ${func_app}"
    appInsightConnectionString=$(\
      az monitor app-insights component create \
        --app "${func_app}" \
        --location "${LOCATION}" \
        --resource-group "${RESOURCE_GROUP}" \
        --tags "${RESOURCE_TAGS}" \
        --application-type "${APPINSIGHTS_KIND}" \
        --kind "${APPINSIGHTS_KIND}" \
        --workspace "${LOG_ANALYTICS_WORKSPACE_ID}" \
        --query "${APPINSIGHTS_CONNECTION_STRING}" \
        --output tsv)

    echo "Integrating Application Insight with function: ${func_app}"
    az monitor app-insights component connect-function \
      --app "${func_app}" \
      --function "${func_app}" \
      --resource-group "${RESOURCE_GROUP}"

    echo "Configure appsettings for: ${func_app}"
    az_serv_str=$(az_connection_string "${RESOURCE_GROUP}" "${identity}")
    blob_conn_str=$(blob_connection_string "${RESOURCE_GROUP}" "${stor_name}")
    # Long-running bulk upload queries require some specific connection
    # details that are not part of the default connection string
    db_conn_str=$(pg_connection_string "$PG_SERVER_NAME" "$db_name" "$identity")
	# Given that the entire max connections for the Participants database is around 150, we need to divide this up between all databases.
	# Going with 15 for now as that seems to be about right, but we can always adjust as needed.
    db_conn_str="${db_conn_str};Tcp Keepalive=true;Tcp Keepalive Time=30000;Command Timeout=300;MaxPoolSize=15;"
    eventgrid_endpoint=$(eventgrid_endpoint "${RESOURCE_GROUP}" "${BU_METRICS_TOPIC_NAME}")
    eventgrid_key_str=$(eventgrid_key_string "${RESOURCE_GROUP}" "${BU_METRICS_TOPIC_NAME}")
    az functionapp config appsettings set \
      --resource-group "$RESOURCE_GROUP" \
      --name "$func_app" \
      --settings \
        ${APPLICATIONINSIGHTS_CONNECTION_STRING}="${appInsightConnectionString}" \
        ${AZ_SERV_STR_KEY}="${az_serv_str}" \
        ${BLOB_CONN_STR_KEY}="${blob_conn_str}" \
        ${CLOUD_NAME_STR_KEY}="${CLOUD_NAME}" \
        ${COLUMN_ENCRYPT_KEY}="@Microsoft.KeyVault(VaultName=${VAULT_NAME};SecretName=${COLUMN_ENCRYPT_KEY_KV})" \
        ${DB_CONN_STR_KEY}="${db_conn_str}" \
        ${EVENTGRID_CONN_STR_ENDPOINT}="${eventgrid_endpoint}" \
        ${EVENTGRID_CONN_STR_KEY}="${eventgrid_key_str}" \
        ${STATE_STR_KEY}="${abbr}" \
        ${UPLOAD_ENCRYPT_KEY}="@Microsoft.KeyVault(VaultName=${VAULT_NAME};SecretName=${UPLOAD_ENCRYPT_KEY_KV})" \
        ${UPLOAD_ENCRYPT_KEY_SHA}="@Microsoft.KeyVault(VaultName=${VAULT_NAME};SecretName=${UPLOAD_ENCRYPT_KEY_SHA_KV})" \
      --output none

    # Create an Active Directory app registration associated with the app.
    # Used by subsequent resources to configure auth
    az ad app create \
      --display-name "${func_app}" \
      --sign-in-audience "AzureADMyOrg"

    # Create Function endpoint before setting up event subscription
    try_run "func azure functionapp publish ${func_app} --dotnet" 7 "../etl/src/Piipan.Etl/Piipan.Etl.Func.BulkUpload"

    # Create Event Grid System Topic for State Uploads - Create blob events
    echo "Create Event Grid System Topic for ${abbr} Uploads - Create blob events"
    event_grid_system_topic_id=$(\
      az eventgrid system-topic create \
        --location "${LOCATION}" \
        --name "${topic_name}" \
        --topic-type Microsoft.Storage.storageAccounts \
        --resource-group "${RESOURCE_GROUP}" \
        --source "${DEFAULT_PROVIDERS}/Microsoft.Storage/storageAccounts/${stor_name}" \
        --output tsv \
        --query id)
    # Stream Event Grid topic diagnostic logs to Event Hub / Log Analytisc workspace
    update_diagnostic_settings "${event_grid_system_topic_id}" "${DIAGNOSTIC_SETTINGS_EVGT_DELIVERY_FAIL}"

    echo "Create Event Grid System Topic Subscription for ${abbr} Uploads - Create blob events"
    storageid=$(get_storage_account_id "${stor_name}" "${RESOURCE_GROUP}")
    az eventgrid system-topic event-subscription create \
      --name "${sub_name}" \
      --system-topic-name "${topic_name}" \
      --resource-group "${RESOURCE_GROUP}" \
      --endpoint "${storageid}${STORAGE_QUEUE}${STATE_STORAGE_UPLOAD_QUEUE}" \
      --endpoint-type storagequeue \
      --included-event-types Microsoft.Storage.BlobCreated \
      --subject-begins-with /blobServices/default/containers/upload/blobs/

  done < env/"${azure_env}"/states.csv

  # Create Query Tool app resources
  ./create-query-tool-resources.bash "${azure_env}"

  # Establish metrics sub-system
  if [ "${PROVISION_METRICS_RESOURCES}" = "true" ] ; then
    ./create-metrics-resources.bash "${azure_env}"
  fi

  # Create Dashboard app resources
  ./create-dashboard-resources.bash "${azure_env}"

  # Core database server and schemas
  if [ "${PROVISION_CORE_DATABASE_RESOURCES}" = "true" ] ; then
     ./create-core-databases.bash "${azure_env}"
  fi

  # Create API Management instance
  if [ "${PROVISION_APIM_RESOURCES}" = "true" ] ; then
    ./create-apim.bash "${azure_env}" "${APIM_EMAIL}"
  fi

  # Configure App Service Authentication
  ./configure-easy-auth.bash "${azure_env}"

  # Configure Microsoft Defender for Cloud, assign Azure CIS 1.3.0 benchmark
  # policy, and policies to export data to Event Hub and Log Analytics workspace.
  ./security/configure-defender-and-policies.bash "${azure_env}"
  # Configure exemptions for Azure CIS 1.3.0 policy
  ./security/configure-policy-exemptions.bash "${azure_env}"

  echo "Secure database connection"
  ./security/remove-external-network.bash \
    "${azure_env}" \
    "${RESOURCE_GROUP}" \
    "${PG_SERVER_NAME}"

  script_completed
}

main "$@"

