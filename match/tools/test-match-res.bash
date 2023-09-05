#!/usr/bin/env bash
#
# Basic remote test tool for the match resolution API. Sends a curl request
# with de-identified data to the API's AddEvent endpoint and writes result to
# stdout. Requires that the user has been added to the necessary application role
# for the function app (see Remote Testing in match/docs/orchestrator-match.md).
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# usage: test-mast-res.bash <azure-env> <match id>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../../tools/common.bash || exit

MATCH_RES_API_FUNC_NAME="AddEvent"
DUP_PART_API_FUNC_NAME="GetMatch"


# The data being added to the event
JSON='{
    "data": {
      "vulnerable_individual": true
    }
}'

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  # shellcheck source=./iac/env/tts/dev.bash
  source "$(dirname "$0")"/../../iac/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/../../iac/iac-common.bash
  verify_cloud
  #match id
  match_id_param=$2
  name=$(get_resources "$MATCH_RES_API_TAG" "$MATCH_RESOURCE_GROUP")
  
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

  match_endpoint_uri=$(\
    az functionapp function show \
      -g "$MATCH_RESOURCE_GROUP" \
      -n "$name" \
      --function-name "$DUP_PART_API_FUNC_NAME" \
      --query invokeUrlTemplate \
      -o tsv)

  endpoint_uri=$(\
    az functionapp function show \
      -g "$MATCH_RESOURCE_GROUP" \
      -n "$name" \
      --function-name "$MATCH_RES_API_FUNC_NAME" \
      --query invokeUrlTemplate \
      -o tsv)

  text_to_replace="{matchid}"
  endpoint_uri=${endpoint_uri/"$text_to_replace"/$match_id_param}
  match_endpoint_uri=${match_endpoint_uri/"$text_to_replace"/$match_id_param}

  echo "Submitting pre add event request to ${match_endpoint_uri}"
  curl \
    --request GET "${match_endpoint_uri}" \
    --header "Authorization: Bearer ${token}" \
    --header 'Accept: application/json' \
    --header 'Content-Type: application/json' \
    --header 'X-Initiating-State: ea' \
    --header 'X-Request-Location: ea' \
    --include

  printf "\n"

  echo "Submitting AddEvent request to ${endpoint_uri}"
  curl \
    --request PATCH "${endpoint_uri}" \
    --header "Authorization: Bearer ${token}" \
    --header 'Accept: application/json' \
    --header 'Content-Type: application/json' \
    --header 'X-Initiating-State: ea' \
    --data-raw "$JSON" \
    --include

    echo "Submitting post add event request to ${match_endpoint_uri}"
  curl \
    --request GET "${match_endpoint_uri}" \
    --header "Authorization: Bearer ${token}" \
    --header 'Accept: application/json' \
    --header 'Content-Type: application/json' \
    --header 'X-Initiating-State: ea' \
    --header 'X-Request-Location: ea' \
    --include

  printf "\n"

  script_completed

}

main "$@"
