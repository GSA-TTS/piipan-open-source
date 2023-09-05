#!/usr/bin/env bash
#
# Basic remote test tool for the APIM Bulk Upload API. Sends a curl request
# with de-identified data to the API's upload endpoint for the configured
# state.
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# usage: test-apim-upload-api.bash <azure-env> <state>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../../tools/common.bash || exit

STATE=$2
STATE_LC=$(echo "$STATE" | tr '[:upper:]' '[:lower:]')
SUBSCRIPTION_NAME="${STATE}-BulkUpload"
DATA_RELPATH=../docs/csv/
DATA_BASENAME=example.csv
UPLOAD_API_PATH="/bulk/${STATE_LC}/v2/upload_all_participants/${DATA_BASENAME}"

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
    --request PUT "${endpoint_uri}" \
    --header 'Content-Type: text/plain' \
    --header "Ocp-Apim-Subscription-Key: ${api_key}" \
    --data-binary "@$(dirname "$0")/${DATA_RELPATH}${DATA_BASENAME}" \
    --include
}
main "$@"
