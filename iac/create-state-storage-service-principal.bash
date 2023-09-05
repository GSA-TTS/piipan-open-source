#!/usr/bin/env bash
# Creates the named service principal (SP) with the built-in Azure role of
# Storage Blob Data Contributor:
# https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#storage-blob-data-contributor
#
# If the SP already exists, the password is reset to a new random
# password.
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# storage-account-name is the name of the storage account you wish to grant access to
#
# usage: create-service-principal.bash <azure-env> <storage-account-name>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  storage_acct_name=$2
  # shellcheck source=./iac/env/tts/dev.bash
  source "$(dirname "$0")"/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/iac-common.bash
  verify_cloud

  # Required service principal name
  name="${storage_acct_name}-external"

  # Role ID for Storage Blob Data Contributor
  ROLE_ID=ba92f5b4-2d11-453d-a403-e96b0029c9fe

  # Get resource id of storage account
  scope=$(az resource show -g "$RESOURCE_GROUP" -n "$storage_acct_name" --resource-type "Microsoft.Storage/storageAccounts" --query id --output tsv)

  # If the service principal does not exist, the `create-for-rbac` command will
  # create it. If the service principal does exist, `create-for-rbac` will "patch"
  # it, creating new credentials.
  echo "Creating/resetting service principal $name"
  az ad sp create-for-rbac \
    --name "$name" \
    --role "$ROLE_ID" \
    --scopes "$scope"

  script_completed
}

main "$@"
