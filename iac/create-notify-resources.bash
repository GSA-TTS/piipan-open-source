#!/usr/bin/env bash
#
# Provisions and configures the infrastructure components for Notify(Notifications).
#
# Assumes the script is executed by an Azure user with the Owner/Contributor
# role for the subscription.
#
# azure-env is the name of the deployment environment (e.g., "fns/dev").
# See iac/env for available environments.
#
# usage: create-notify-resources.bash <azure-env>

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

  # Several CLI commands use the vnet resource ID
  VNET_ID=$(get_vnet)

  # Create Public IP to use with Email NAT Gateway
  ./create-public-ip-for-email.bash "$azure_env"

  # Retrieve Public IP to use with Email NAT Gateway
  PUBLIC_IP_EMAIL=$(\
    az network public-ip show \
      --name "${PUBLIC_IP_NAME}" \
      --resource-group "${RESOURCE_GROUP}" \
      --query id \
      -o tsv)

  # Create NAT Gateway for Email
  echo "Creating NAT Gateway for Email"
  az network nat gateway create \
    --name "${NAT_GW_EMAIL}" \
    --resource-group "${RESOURCE_GROUP}" \
    --location "${LOCATION}" \
    --public-ip-addresses "${PUBLIC_IP_EMAIL}" \
    --idle-timeout 4

  # Attach NAT Gateway to email subnet
  echo "Attach NAT Gateway '${NAT_GW_EMAIL}' to subnet '${EMAIL_SUBNET_NAME}'"
  az network vnet subnet update \
    --resource-group "${RESOURCE_GROUP}" \
    --vnet-name "${VNET_NAME}" \
    --name "${EMAIL_SUBNET_NAME}" \
    --nat-gateway "${NAT_GW_EMAIL}"

  # Create Notify API Function App
  echo "Create Notify API Function App"
  az deployment group create \
    --name notify-api \
    --resource-group "${RESOURCE_GROUP}" \
    --template-file  ./arm-templates/function-notify.json \
    --parameters \
      resourceTags="$RESOURCE_TAGS" \
      location="$LOCATION" \
      functionAppName="$NOTIFY_FUNC_APP_NAME" \
      appServicePlanName="${EMAIL_APP_SERVICE_PLAN_NAME}" \
      storageAccountName="$NOTIFY_FUNC_APP_STORAGE_NAME" \
      cloudName="${CLOUD_NAME}" \
      coreResourceGroup="${RESOURCE_GROUP}" \
      diagnosticSettingName="${DIAGNOSTIC_SETTINGS_NAME}" \
      eventHubAuthorizationRuleId="${EH_RULE_ID}" \
      eventHubName="${EVENT_HUB_NAME}" \
      sku="${STORAGE_SKU}" \
      queueName="${NOTIFY_FUNC_APP_STORAGE_QUEUE}" \
      enableEmails="${ENABLE_EMAILS}" \
      smtpServer="${SMTP_SERVER}" \
      smtpFromEmail="${SMTP_FROM_EMAIL}" \
      smtpCcEmail="${SMTP_CC_EMAIL}" \
      smtpBccEmail="${SMTP_BCC_EMAIL}" \
      subnet="${EMAIL_SUBNET_NAME}" \
      systemTypeTag="${NOTIFY_API_SYSTEM_TAG}" \
      vnet="${VNET_ID}" \
      workspaceId="${LOG_ANALYTICS_WORKSPACE_ID}"

  # Create an Active Directory app registration associated with the app.
  # Used by subsequent resources to configure auth
  az ad app create \
    --display-name "${NOTIFY_FUNC_APP_NAME}" \
    --sign-in-audience "AzureADMyOrg"

  echo "Integrating ${NOTIFY_FUNC_APP_NAME} into virtual network"
  az functionapp vnet-integration add \
    --name "${NOTIFY_FUNC_APP_NAME}" \
    --resource-group "${RESOURCE_GROUP}" \
    --subnet "${EMAIL_SUBNET_NAME}" \
    --vnet "${VNET_ID}"

  echo "Removing public access for ${NOTIFY_FUNC_APP_NAME}"
  az storage account update \
    --name "${NOTIFY_FUNC_APP_STORAGE_NAME}" \
    --resource-group "${RESOURCE_GROUP}" \
    --default-action "Deny"

  echo "Update ${NOTIFY_FUNC_APP_NAME} settings"
  az functionapp config appsettings set \
    --name "${NOTIFY_FUNC_APP_NAME}" \
    --resource-group "${RESOURCE_GROUP}" \
    --settings \
      WEBSITE_CONTENTOVERVNET=1 \
      WEBSITE_VNET_ROUTE_ALL=1

  echo "Publish Notify API Function App"
  try_run "func azure functionapp publish ${NOTIFY_FUNC_APP_NAME} --dotnet" 7 "../notifications/src/Piipan.Notifications.Func.Api"

  script_completed
}

main "$@"

