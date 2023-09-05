#!/usr/bin/env bash
#
# Basic remote test tool for the orchestrator match API. Sends a curl request
# with de-identified data to the API's find_matches endpoint and writes result to
# stdout. Requires that the user has been added to the necessary application role
# for the function app (see Remote Testing in match/docs/orchestrator-match.md).
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# usage: test-orch-match-api.bash <azure-env>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../../tools/common.bash || exit

MATCH_API_FUNC_NAME="find_matches"

# Hash digest for farrington,10/13/31,425-46-5417
JSON='{
    "data": [{
      "lds_hash": "a3cab51dd68da2ac3e5508c8b0ee514ada03b9f166f7035b4ac26d9c56aa7bf9d6271e44c0064337a01b558ff63fd282de14eead7e8d5a613898b700589bcdec"
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

  name=$(get_resources "$ORCHESTRATOR_API_TAG" "$MATCH_RESOURCE_GROUP")

  aad_app_id=$(\
    az ad app list \
      --display-name "${name}" \
      --filter "displayName eq '${name}'" \
      --query "[0].appId" \
      --output tsv)

  echo "Retrieving access token from ${name}"
  token=$(\
    az account get-access-token \
      --resource "api://${aad_app_id}" \
      --query accessToken \
      -o tsv
  )

  endpoint_uri=$(\
    az functionapp function show \
      -g "$MATCH_RESOURCE_GROUP" \
      -n "$name" \
      --function-name "$MATCH_API_FUNC_NAME" \
      --query invokeUrlTemplate \
      -o tsv)

  echo "Submitting request to ${endpoint_uri}"
  curl \
    --request POST "${endpoint_uri}" \
    --header "Authorization: Bearer ${token}" \
    --header 'Accept: application/json' \
    --header 'Content-Type: application/json' \
    --header 'X-Initiating-State: ea' \
    --data-raw "$JSON" \
    --include

  printf "\n"

  script_completed

}

main "$@"
