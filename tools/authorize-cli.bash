#!/usr/bin/env bash
#
# Add the Azure CLI as a "pre-authorized application" for the specified
# application object. Used to allow users to obtain an access token via
# the CLI (e.g., `az account get-access-token <app-uri>`). Access tokens
# can be used to call internal APIs which are protected by Easy Auth.
#
# Prior to Azure CLI 2.37, the 'az ad app create' command added a
# user_impersonation scope to expose the application as an API.
# With 2.37 Microsoft Graph migration, we must create the user_impersonation
# scope, along with the preAuthorizedApplications. This is ONLY needed
# local development.
#
# <azure-env> is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
# <func-app-name> is the name of the Function app resource.
#
# usage: assign-app-role.bash <azure-env> <func-app-name>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/common.bash || exit

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  # Name of the Function app resource
  func=$2
  # shellcheck source=./iac/env/tts/dev.bash
  source "$(dirname "$0")"/../iac/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/../iac/iac-common.bash
  verify_cloud

  # For references to hard-coded ID see:
  # - https://docs.microsoft.com/en-us/azure-stack/user/azure-stack-rest-api-use?view=azs-2008#example-1
  # - https://github.com/Azure/azure-cli/blob/24e0b9ef8716e16b9e38c9bb123a734a6cf550eb/src/azure-cli-core/azure/cli/core/_profile.py#L65
  azure_cli_client_id="04b07795-8ddb-461a-bbee-02f9e1bf7b46"
  visual_studio_client_id="872cd9fa-d31f-45e0-9eab-6e460a02d1f1"
  #powershell_client_id="1950a258-227b-4e31-a9cf-717495945fc2"
  
  domain=$(graph_host_suffix)
  object_id=$(\
    az ad app list \
      --display-name "${func}" \
      --query "[0].id" \
      -o tsv)

  # Use the Azure Cli Client Id as the unique identifier for the oauth2PermissionScope applications require
  # - https://docs.microsoft.com/en-us/graph/api/resources/preauthorizedapplication?view=graph-rest-1.0#properties
  permission_scope_id=$azure_cli_client_id

  echo "Adding user_impersonation scope"
  # shellcheck disable=SC2016
  json="{
    \"api\": {
      \"oauth2PermissionScopes\": [{
        \"adminConsentDescription\":\"Allow the application to access ${func} on behalf of the signed-in user.\",
        \"adminConsentDisplayName\":\"Access ${func}\",
        \"id\":\"${permission_scope_id}\",
        \"isEnabled\":true,
        \"type\":\"User\",
        \"userConsentDescription\":\"Allow the application to access ${func} on your behalf.\",
        \"userConsentDisplayName\":\"Access ${func}\",
        \"value\":\"user_impersonation\"
      }]
    }
  }"

  az rest \
    -m PATCH \
    -u "https://graph${domain}/v1.0/applications/${object_id}" \
    --headers 'Content-Type=application/json' \
    --body "${json}"

  echo "Register Azure CLI and Visual Studio as Pre-Authorized Applications"
    
  # shellcheck disable=SC2016
  json="{
      \"api\": {
        \"preAuthorizedApplications\": [{
          \"appId\":\"${azure_cli_client_id}\",
          \"delegatedPermissionIds\":[\"${permission_scope_id}\"]
        },
        {
          \"appId\":\"${visual_studio_client_id}\",
          \"delegatedPermissionIds\":[\"${permission_scope_id}\"]
        }]
      }
    }"

  az rest \
    -m PATCH \
    -u "https://graph${domain}/v1.0/applications/${object_id}" \
    --headers 'Content-Type=application/json' \
    --body "${json}"

  script_completed
}

main "$@"
