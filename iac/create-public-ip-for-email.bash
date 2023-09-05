#!/usr/bin/env bash
#
# Creates a Public IP address in Azure for outbound emailing using the Agency Email Relay
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# usage: create-public-ip-for-email.bash <azure-env>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  source "$(dirname "$0")"/env/"${azure_env}".bash
  source "$(dirname "$0")"/iac-common.bash
  verify_cloud
  set_defaults

  echo "Creating Public IP for outbound emailing"
  # [Coming breaking change] In the coming release, the default behavior will be
  # changed as follows when sku is Standard and zone is not provided:
  # For zonal regions, you will get a zone-redundant
  # IP indicated by zones:["1","2","3"]; For non-zonal regions,
  # you will get a non zone-redundant IP indicated by zones:null.
  # shellcheck disable=SC2086
  az network public-ip create \
    --resource-group "${RESOURCE_GROUP}" \
    --name "${PUBLIC_IP_NAME}" \
    --version "IPv4" \
    --sku "Standard" \
    --zone ${EMAIL_ZONE_REDUNDANCY}
}

main "$@"
