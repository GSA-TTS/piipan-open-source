#!/usr/bin/env bash
#
# Stores the shared client secrets from the OIDC identity provider (IdP)
# for each OIDC-enabled App Service instance. The core key vault is used
# and at a later stage, the values will be read by configure-oidc-apps.bash
# to udpate each apps' environment.
#
# Assumes an Azure user with the Global Administrator role has signed in
# with the Azure CL, and the infrastructure, established by create-resources.bash
# is in place.
#
# usage: store-oidc-secrets.bash <azure-env>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit

main () {
  # Load agency/subscription/deployment-specific settings
  local azure_env=$1
  source "$(dirname "$0")"/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/iac-common.bash
  verify_cloud

  unset -v password # make sure it's not exported
  set +o allexport  # make sure variables are not automatically exported

  for app in "${OIDC_APPS[@]}"
  do
    IFS= read -rsp "Enter client secret for $app:" password < /dev/tty
    echo
    echo "Storing secret..."
    set_oidc_secret "$app" "$password"
  done

  script_completed
}

main "$@"
