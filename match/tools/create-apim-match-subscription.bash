#!/usr/bin/env bash
#
# Creates a subscription for the specified state for the Duplicate
# Participation ("match") API.
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# state-abbr is the two-letter postal abbreviation of the state.
#
# usage: create-apim-match-subscription.bash <azure-env> <state-abbr>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../../tools/common.bash || exit

SUBSCRIPTION_SUFFIX="-DupPart"
MATCH_API_NAME="match"

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  # shellcheck source=./iac/env/tts/dev.bash
  source "$(dirname "$0")"/../../iac/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/../../iac/iac-common.bash
  verify_cloud

  state_abbr=$2
  state_abbr=$(echo "$state_abbr" | tr '[:lower:]' '[:upper:]')
  subscription_name="${state_abbr}${SUBSCRIPTION_SUFFIX}"

  serviceName=$(get_resources "$DUP_PART_API_TAG" "$MATCH_RESOURCE_GROUP")
  mgmt_domain=$(resource_manager_host_suffix)
  mgmt_uri="https://management${mgmt_domain}/subscriptions/${SUBSCRIPTION_ID}/resourceGroups/${MATCH_RESOURCE_GROUP}/providers/Microsoft.ApiManagement/service/${serviceName}/subscriptions/${subscription_name}?api-version=2020-12-01"
  body='{
    "properties": {
      "displayName": "'"${subscription_name}"'",
      "scope": "/apis/'"${MATCH_API_NAME}"'"
    }
}'

  echo "Creating subscription for apis/${MATCH_API_NAME}"
  display_name=$(\
    az rest \
      --method PUT \
      --uri "$mgmt_uri" \
      --body "$body" \
      --query name \
      --output tsv)
  
  echo "Created subscription ${display_name}"

  script_completed
}

main "$@"
