#!/usr/bin/env bash

# Removes residual Lookup API resources from provided Azure environment.

# usage: ./remove-lookup-resources.bash <azure-env>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../common.bash || exit

set_constants () {
  LOOKUP_STORAGE_NAME=${PREFIX}stlookupapi${ENV}
  MATCH_API_ID=match
  LOOKUP_OPERATION_ID=get-lookup
  APIM_RESOURCE_NAME=${PREFIX}-apim-duppartapi-${ENV}
}

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  # shellcheck source=./iac/env/tts/dev.bash
  source "$(dirname "$0")"/../../iac/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/../../iac/iac-common.bash
  verify_cloud

  set_constants

  echo "Deleting Lookup storage account"
  az storage account delete \
    -n "$LOOKUP_STORAGE_NAME" \
    -g "$MATCH_RESOURCE_GROUP"

  echo "Deleting APIM Lookup operation"
  az apim api operation delete \
    --api-id $MATCH_API_ID \
    --operation-id $LOOKUP_OPERATION_ID \
    -g "$MATCH_RESOURCE_GROUP" \
    -n "$APIM_RESOURCE_NAME"

  script_completed
}

main "$@"
