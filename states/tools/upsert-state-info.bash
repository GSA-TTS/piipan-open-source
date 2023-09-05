#!/usr/bin/env bash
#
# Basic tool for calling upsert endpoint on State Info API. Sends a curl request
# with state information to API's Upsert endpoint and writes result to
# stdout. Requires that the user has been added to the necessary application role
# for the function app (see Remote Testing in match/docs/orchestrator-match.md).
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# usage: upsert-state-info.bash <azure-env> <state> <state abbreviation> <email> <phone> <region> <email cc>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../../tools/common.bash || exit

UPSERT_STATE_API_FUNC_NAME="upsert_state"
GET_STATES_API_FUNC_NAME="states"

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  # shellcheck source=./iac/env/tts/dev.bash
  source "$(dirname "$0")"/../../iac/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/../../iac/iac-common.bash
  verify_cloud

  id=$2
  state_param=$3
  state_abbr_param=$4
  email_param=$5
  phone_param=$6
  region_param=$7

  # The state data being upserted into the database
  JSON='{
      "data": {
        "id": '${id}',
        "state": '"\"${state_param}\""',
        "state_abbreviation": '"\"${state_abbr_param}\""',
        "email": '"\"${email_param}\""',
        "phone": '"\"${phone_param}\""',
        "region": '"\"${region_param}\""'
      }
  }'

  if [ $# -eq 8 ]  #email_cc is optional. If provided as an argument, construct payload to include it
  then
      email_cc_param=$8

        # The state data being upserted into the database
      JSON='{
          "data": {
            "id": '${id}',
            "state": '"\"${state_param}\""',
            "state_abbreviation": '"\"${state_abbr_param}\""',
            "email": '"\"${email_param}\""',
            "phone": '"\"${phone_param}\""',
            "region": '"\"${region_param}\""',
            "email_cc": '"\"${email_cc_param}\""',
          }
      }'
  fi





  name=$(get_resources "$STATES_API_TAG" "$RESOURCE_GROUP")
  
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

  states_endpoint_uri=$(\
    az functionapp function show \
      -g "$RESOURCE_GROUP" \
      -n "$name" \
      --function-name "$GET_STATES_API_FUNC_NAME" \
      --query invokeUrlTemplate \
      -o tsv)

  upsert_endpoint_uri=$(\
    az functionapp function show \
      -g "$RESOURCE_GROUP" \
      -n "$name" \
      --function-name "$UPSERT_STATE_API_FUNC_NAME" \
      --query invokeUrlTemplate \
      -o tsv)

  upsert_endpoint_uri=${upsert_endpoint_uri}
  states_endpoint_uri=${states_endpoint_uri}

  echo "Submitting pre request to retrieve current states from ${states_endpoint_uri}"
  curl \
    --request GET "${states_endpoint_uri}" \
    --header "Authorization: Bearer ${token}" \
    --header 'Accept: application/json' \
    --header 'Content-Type: application/json' \
    --include

  printf "\n"

  echo "Submitting AddEvent request to ${upsert_endpoint_uri}"
  curl \
    --request POST "${upsert_endpoint_uri}" \
    --header "Authorization: Bearer ${token}" \
    --header 'Accept: application/json' \
    --header 'Content-Type: application/json' \
    --data-raw "$JSON" \
    --include

    echo "Submitting post request to retrieve new list of states from ${states_endpoint_uri}"
  curl \
    --request GET "${states_endpoint_uri}" \
    --header "Authorization: Bearer ${token}" \
    --header 'Accept: application/json' \
    --header 'Content-Type: application/json' \
    --include

  printf "\n"

  script_completed

}

main "$@"
