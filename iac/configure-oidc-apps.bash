#!/usr/bin/env bash
#
# Sets the IDP_CLIENT_SECRET application setting for the OIDC-enabled
# App Service instances in the system. This will allow each app to
# delegate user authentication to the configured OIDC identity provider.
#
# Assumes an Azure user with the Global Administrator role has signed in
# with the Azure CLI, the infrastructure, established by create-resources.bash
# is in place, and the shared client secret with each app's IdP has been
# correctly set using store-oidc-secrets.bash.
#
# usage: configure-oidc-apps.bash <azure-env>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit

main () {
  # Load agency/subscription/deployment-specific settings
  local azure_env=$1
  source "$(dirname "$0")"/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/iac-common.bash
  verify_cloud

  for app in "${OIDC_APPS[@]}"
  do
    local secret
    secret=$(get_oidc_secret "$app")

    echo "Set IDP_CLIENT_SECRET for $app"

    # Filter, redirect output to doubly ensure secret is not sent to terminal
    az webapp config appsettings set \
      --name "$app" \
      --resource-group "$RESOURCE_GROUP" \
      --settings IDP_CLIENT_SECRET="$secret" \
      --query "[].name" > /dev/null
  done

  script_completed
}

main "$@"
