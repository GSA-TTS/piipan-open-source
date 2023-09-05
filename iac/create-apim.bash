#!/usr/bin/env bash
#
# Creates the API Management instance for managing the external-facing
# duplicate participation API. Assumes an Azure user with the Global
# Administrator role has signed in with the Azure CLI.
# See install-extensions.bash for prerequisite Azure CLI extensions.
# Deployment can take ~45 minutes for new instances.
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# admin-email is the email address to use for the required "publisher
# email" property. A notification will be sent to the email when the
# instance has been created.
#
# usage: create-apim.bash <azure-env> <admin-email>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit

clean_defaults () {
  local group=$1
  local apim=$2

  # Delete "echo API" example API
  az apim api delete \
    --api-id echo-api \
    -g "${group}" \
    -n "${apim}" \
    -y

  # Delete default "Starter" and "Unlimited" products and their associated
  # product subscriptions
  az apim product delete \
    --product-id starter \
    --delete-subscriptions true \
    -g "${group}" \
    -n "${apim}" \
    -y

  az apim product delete \
    --product-id unlimited \
    --delete-subscriptions true \
    -g "${group}" \
    -n "${apim}" \
    -y
}

generate_policy () {
  local path
  path=$(dirname "$0")/$1
  local uri=$2
  local APP_URI_PLACEHOLDER="{applicationUri}"
  local xml
  xml=$(< "$path")

  xml=${xml/$APP_URI_PLACEHOLDER/$uri}

  echo "$xml"
}

grant_blob () {
  local assignee=$1
  local storage_account=$2
  local DEFAULT_PROVIDERS="/subscriptions/${SUBSCRIPTION_ID}/resourceGroups/${RESOURCE_GROUP}/providers"

  #Only create the role assignment if it doesn't exist
  assignment_id=$(\
    az role assignment list \
       --role "Storage Blob Data Contributor" \
       --assignee "${assignee}" \
       --scope "${DEFAULT_PROVIDERS}/Microsoft.Storage/storageAccounts/${storage_account}" \
       --query "[0].id" \
       --output tsv)

  if [ -z "${assignment_id}" ]; then
    az role assignment create \
      --role "Storage Blob Data Contributor" \
      --assignee "${assignee}" \
      --scope "${DEFAULT_PROVIDERS}/Microsoft.Storage/storageAccounts/${storage_account}" \
      --output none
  fi
}

get_state_abbrs () {
  local state_abbrs=()

  while IFS=, read -r abbr _; do
    abbr=$(echo "$abbr" | tr '[:upper:]' '[:lower:]')
    state_abbrs+=("${abbr}")
  done < env/"${azure_env}"/states.csv

  echo "${state_abbrs[*]}"
}

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  source "$(dirname "$0")"/env/"${azure_env}".bash
  source "$(dirname "$0")"/iac-common.bash
  verify_cloud
  set_defaults

  APIM_NAME=${PREFIX}-apim-duppartapi-${ENV}
  PUBLISHER_NAME='API Administrator'
  publisher_email=$2
  orch_name=$(get_resources "$ORCHESTRATOR_API_TAG" "$MATCH_RESOURCE_GROUP")
  orch_hostname=$(\
    az functionapp show \
      -g "$MATCH_RESOURCE_GROUP" \
      -n "$orch_name" \
      --query defaultHostName \
      --output tsv)
  orch_api_url="https://${orch_hostname}/api/v1"
  orch_aad_client_id=$(\
    az ad app list \
      --display-name "${orch_name}" \
      --filter "displayName eq '${orch_name}'" \
      --query "[0].appId" \
      --output tsv)

  duppart_policy_xml=$(generate_policy apim-duppart-policy.xml "api://${orch_aad_client_id}")

  uploadallparticipants_policy_path=$(dirname "$0")/apim-uploadallparticipants-policy.xml
  uploadallparticipants_policy_xml=$(< "$uploadallparticipants_policy_path")

  get_upload_by_id_policy_path=$(dirname "$0")/apim-getuploadbyid-policy.xml
  get_upload_by_id_policy_xml=$(< "$get_upload_by_id_policy_path")

  all_upload_operations_policy_path=$(dirname "$0")/apim-all-bulkupload-operations-policy.xml
  all_upload_operations_policy_xml=$(< "$all_upload_operations_policy_path")

  apim_policy_path=$(dirname "$0")/apim-policy.xml
  apim_policy_xml=$(< "$apim_policy_path")

  duplicate_participation_openapi_path=$(dirname "$0")/../docs/openapi/generated/duplicate-participation-api/openapi.yaml
  duplicate_participation_openapi_yaml=$(< "$duplicate_participation_openapi_path")

  bulk_openapi_path=$(dirname "$0")/../docs/openapi/generated/bulk-api/openapi.yaml
  bulk_openapi_yaml=$(< "$bulk_openapi_path")

  local state_abbrs
  state_abbrs=$(get_state_abbrs)

  local state_app_id_list=()
  for i in $state_abbrs
    do
    single_state_etl_app_id=$(\
    az ad app list \
      --display-name "${PREFIX}-func-${i}etl-${ENV}" \
      --filter "displayName eq '${PREFIX}-func-${i}etl-${ENV}'" \
      --query "[0].appId" \
      --output tsv)
    state_app_id_list+=("${single_state_etl_app_id}") 
    done    

  echo "Creating APIM instance"
  apim_identity=$(\
    az deployment group create \
      --name apim-dev \
      --resource-group "$MATCH_RESOURCE_GROUP" \
      --template-file ./arm-templates/apim.json \
      --query properties.outputs.identity.value.principalId \
      --output tsv \
      --parameters \
        env="$ENV" \
        prefix="$PREFIX" \
        cloudName="$CLOUD_NAME" \
        apiName="$APIM_NAME" \
        publisherEmail="$publisher_email" \
        publisherName="$PUBLISHER_NAME" \
        orchestratorUrl="$orch_api_url" \
        dupPartPolicyXml="$duppart_policy_xml" \
        uploadStates="$state_abbrs" \
        location="$LOCATION" \
        resourceTags="$RESOURCE_TAGS" \
        coreResourceGroup="$RESOURCE_GROUP" \
        apimPolicyXml="$apim_policy_xml" \
        diagnosticSettingName="${DIAGNOSTIC_SETTINGS_NAME}" \
        eventHubAuthorizationRuleId="${EH_RULE_ID}" \
        eventHubName="${EVENT_HUB_NAME}" \
        workspaceId="${LOG_ANALYTICS_WORKSPACE_ID}" \
        sku="${APIM_SKU}" \
        openApiYamlFileContent="${duplicate_participation_openapi_yaml}")

  apim_id=$(\
    az apim show \
      --name "$APIM_NAME" \
      --resource-group "$MATCH_RESOURCE_GROUP" \
      --query id \
      --output tsv)

  # Update Key Vault to allow APIM access
  echo "Granting Key Vault access to APIM"
  az deployment group create \
    --name "${VAULT_NAME}-access-policy-for-apim" \
    --resource-group "${RESOURCE_GROUP}" \
    --template-file ./arm-templates/key-vault-access-policy.json \
    --parameters \
      keyVaultName="${VAULT_NAME}" \
      objectId="${apim_identity}" \
      permissionsSecrets="['get', 'list']"

  # Create APIM Named Value
  echo "Creating APIM Named Value for ${UPLOAD_ENCRYPT_KEY_KV}"
  az deployment group create \
   --name "named-value-${UPLOAD_ENCRYPT_KEY_KV}" \
   --resource-group "${MATCH_RESOURCE_GROUP}" \
   --template-file ./arm-templates/apim-named-value.json \
   --parameters \
     apimName="${APIM_NAME}" \
     keyVaultName="${VAULT_NAME}" \
     keyVaultResourceGroup="${RESOURCE_GROUP}" \
     secretName="${UPLOAD_ENCRYPT_KEY_KV}"

  # Create APIM Named Value
  echo "Creating APIM Named Value for ${UPLOAD_ENCRYPT_KEY_SHA_KV}"
  az deployment group create \
  --name "named-value-${UPLOAD_ENCRYPT_KEY_SHA_KV}" \
  --resource-group "${MATCH_RESOURCE_GROUP}" \
  --template-file ./arm-templates/apim-named-value.json \
  --parameters \
    apimName="${APIM_NAME}" \
    keyVaultName="${VAULT_NAME}" \
    keyVaultResourceGroup="${RESOURCE_GROUP}" \
    secretName="${UPLOAD_ENCRYPT_KEY_SHA_KV}"

  echo "Creating APIM - Bulk Upload"
  az deployment group create \
    --name apim-dev \
    --resource-group "$MATCH_RESOURCE_GROUP" \
    --template-file ./arm-templates/apim-bulkupload.json \
    --parameters \
      apiName="${APIM_NAME}" \
      cloudName="${CLOUD_NAME}" \
      env="${ENV}" \
      prefix="${PREFIX}" \
      uploadAllParticipantsPolicyXml="${uploadallparticipants_policy_xml}" \
      uploadStates="${state_abbrs}" \
      state_app_id_list="${state_app_id_list[*]}" \
      uploadByIdPolicyXml="${get_upload_by_id_policy_xml}" \
      allUploadOperationsPolicyXml="${all_upload_operations_policy_xml}" \
      openApiYamlFileContent="${bulk_openapi_yaml}"

  echo "Granting APIM identity contributor access to per-state storage accounts"
  upload_accounts=($(get_resources "$PER_STATE_STORAGE_TAG" "$RESOURCE_GROUP"))

  tenant_id=$(az account show --query homeTenantId -o tsv)
  for account in "${upload_accounts[@]}"
  do
    # \r sometimes gets added to the account. Remove it
    account="${account/$'\r'/}"
    grant_blob "$apim_identity" "$account"

    echo "Allowing APIM to access $account"
    az storage account network-rule add \
      --account-name "$account" \
      --resource-group "$RESOURCE_GROUP" \
      --resource-id "$apim_id" \
      --tenant-id "$tenant_id"
  done

  # Clear out default example resources
  # See: https://stackoverflow.com/a/64297708
  clean_defaults "$MATCH_RESOURCE_GROUP" "$APIM_NAME"

  script_completed
}

main "$@"
