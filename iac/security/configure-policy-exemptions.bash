#!/usr/bin/env bash
#
# Configures policy exemptions for CIS Microsoft Azure
# https://docs.microsoft.com/en-us/azure/governance/policy/samples/cis-azure-1-3-0
# https://docs.microsoft.com/en-us/azure/governance/policy/samples/gov-cis-azure-1-3-0
#
# Assumes the script is executed by an Azure user with the Owner/Contributor role for the subscription.
#
# usage: configure-policy-exemptions.bash <azure-env>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../../tools/common.bash || exit

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  source "$(dirname "$0")"/../env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/../iac-common.bash
  verify_cloud
  set_defaults

  echo "Configure policy exemptions for ${AZURE_CIS_POLICY}"
  az deployment sub create \
    --name "azure-policy-exemptions" \
    --location "${LOCATION}" \
    --template-file "$(dirname "$0")"/../arm-templates/security/policy-exemptions.json \
    --parameters policyName="${AZURE_CIS_POLICY}" \

  script_completed
}

main "$@"
