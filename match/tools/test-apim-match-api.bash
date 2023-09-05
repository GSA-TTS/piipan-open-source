#!/usr/bin/env bash
#
# Basic remote test tool for the APIM match API. Sends a curl request
# with de-identified data to the API's find_matches endpoint and writes result to
# stdout.
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# usage: test-apim-match-api.bash <azure-env>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../../tools/common.bash || exit

SUBSCRIPTION_NAME="EB-DupPart"
MATCH_API_PATH="/match/v2/find_matches"

# Hash digest for farrington,10/13/31,425-46-5417
JSON='{
    "data": [{
      "lds_hash": "a3cab51dd68da2ac3e5508c8b0ee514ada03b9f166f7035b4ac26d9c56aa7bf9d6271e44c0064337a01b558ff63fd282de14eead7e8d5a613898b700589bcdec",
      "participant_id": "participant1",
      "search_reason": "application"
    }]
}'

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  # shellcheck source=./iac/env/tts/dev.bash
  source "$(dirname "$0")"/../../iac/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/../../iac/iac-common.bash
  verify_cloud

  serviceName=$(get_resources "$DUP_PART_API_TAG" "$MATCH_RESOURCE_GROUP")
  apim_domain=$(apim_host_suffix)
  endpoint_uri="https://${serviceName}${apim_domain}${MATCH_API_PATH}"
  mgmt_domain=$(resource_manager_host_suffix)
  mgmt_uri="https://management${mgmt_domain}/subscriptions/${SUBSCRIPTION_ID}/resourceGroups/${MATCH_RESOURCE_GROUP}/providers/Microsoft.ApiManagement/service/${serviceName}/subscriptions/${SUBSCRIPTION_NAME}/listSecrets?api-version=2020-12-01"

  api_key=$(\
    az rest \
    --method POST \
    --uri "$mgmt_uri" \
    --query primaryKey \
    --output tsv)

  echo "Submitting request to ${endpoint_uri}"
  curl \
    --request POST "${endpoint_uri}" \
    --header 'Content-Type: application/json' \
    --header 'Accept: application/json' \
    --header "Ocp-Apim-Subscription-Key: ${api_key}" \
    --data-raw "$JSON" \
    --include
}
main "$@"
