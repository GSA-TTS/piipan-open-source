#!/usr/bin/env bash
#
# Assigns the current CLI user the specified application role of the
# specified Azure Function. Used to enable the use of Azure CLI credentials
# (i.e., `Azure.Identity.AzureCliCredential`) when connecting to remote APIs
# which have been secured using App Service Authentication. Only intended
# for development environments.
#
# <azure-env> is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
# <func-app-name> is the name of the Function app resource.
# <role-name> is the name of a valid application role
#
# usage: assign-app-role.bash <azure-env> <func-app-name> <role-name>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/common.bash || exit

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  # shellcheck source=./iac/env/tts/dev.bash
  source "$(dirname "$0")"/../iac/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/../iac/iac-common.bash
  verify_cloud

  function=$2
  role=$3

  # Get function's application object
  application=$(\
    az ad app list \
      --display-name "${function}" \
      --query "[0].appId" \
      --output tsv)

  # Get application role ID from application object
  role_id=$(\
    az ad app show \
      --id "${application}" \
      --query "appRoles[?value == '${role}'].id" \
      --output tsv)

  # Get application object's service principal
  service_principal=$(\
    az ad sp list \
      --display-name "${function}" \
      --filter "appId eq '${application}'" \
      --query [0].id \
      --output tsv)

  # Get user's Azure AD object ID
  user=$(\
    az ad signed-in-user show \
      --query id \
      --output tsv)

  json="\
  {
    \"principalId\": \"${user}\",
    \"resourceId\": \"${service_principal}\",
    \"appRoleId\": \"${role_id}\"
  }"

  domain=$(graph_host_suffix)

  # Assign application role
  az rest \
    --method POST \
    --uri "https://graph${domain}/v1.0/users/${user}/appRoleAssignments" \
    --headers 'Content-Type=application/json' \
    --body "$json"

  script_completed
}

main "$@"
