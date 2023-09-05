#!/usr/bin/env bash
#
# Basic remote test tool for the APIM Bulk Upload API. Sends a curl request
# with an upload identifier to the uploads endpoint to get an uploads status.
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# usage: test-apim-get-upload-by-id.bash <azure-env> <state> <upload identifier>

# $0 == environment part 1
# $1 == environment part 2
# $2 == state abbreviation
# $3 == upload identifier

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../../tools/common.bash || exit

SUBSCRIPTION_NAME="${2}-BulkUpload"
upload_identifier=${3}
UPLOAD_API_PATH="/bulk/${2}/v2/uploads/${upload_identifier}"

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

  endpoint_uri="https://${serviceName}${apim_domain}${UPLOAD_API_PATH}"
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
    --request GET "${endpoint_uri}" \
    --header 'Content-Type: text/plain' \
    --header "Ocp-Apim-Subscription-Key: ${api_key}" \
    --include
}
main "$@"