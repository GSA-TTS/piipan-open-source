#!/usr/bin/env bash
#
# Basic integration test tool for the Metrics API. Sends a curl request to the
# API's GetParticipantUploads endpoint and writes result to stdout.
# Requires the caller is on an allowed Network.
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# usage: test-metricsapi.bash <azure-env>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../../tools/common.bash || exit

main () {
  azure_env=$1
  # shellcheck source=./iac/env/tts/dev.bash
  source "$(dirname "$0")"/../../iac/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/../../iac/iac-common.bash
  verify_cloud

  app_id=$(\
    az ad app list \
      --display-name "${METRICS_API_APP_NAME}" \
      --filter "displayName eq '${METRICS_API_APP_NAME}'" \
      --query "[0].appId" \
      --output tsv)
  token=$(az account get-access-token --resource "api://${app_id}" --query accessToken -o tsv)

  # grab url for metrics api
  function_uri=$(az functionapp function show \
    --name "$METRICS_API_APP_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --function-name $METRICS_API_FUNCTION_NAME \
    --query invokeUrlTemplate \
    --output tsv)

  echo "Submitting request to ${function_uri}"
  curl -X GET -i \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer ${token}" \
    "${function_uri}"

  printf "\n"

  # grab url for metrics api
  function_uri_lastupload=$(az functionapp function show \
    --name "$METRICS_API_APP_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --function-name $METRICS_API_FUNCTION_NAME_LASTUPLOAD \
    --query invokeUrlTemplate \
    --output tsv)

  echo "Submitting request to ${function_uri_lastupload}"
  curl -X GET -i \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer ${token}" \
    "${function_uri_lastupload}"

  printf "\n"

    # grab url for metrics statistics api
  function_uri_statistics=$(az functionapp function show \
    --name "$METRICS_API_APP_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --function-name $METRICS_API_FUNCTION_NAME_STATISTICS \
    --query invokeUrlTemplate \
    --output tsv)

  echo "Submitting request to ${function_uri_statistics}"
  curl -X GET -i \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer ${token}" \
    "${function_uri_statistics}"

  printf "\n"

  script_completed
}

main "$@"
