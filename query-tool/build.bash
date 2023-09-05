#!/usr/bin/env bash
#
# Builds project with optional testing and app deployment
# Relies on a solutions file (sln) in the subsystem root directory
# See build-common.bash for usage details

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit
# shellcheck source=./tools/build-common.bash
source "$(dirname "$0")"/../tools/build-common.bash || exit

set_constants () {
  QUERY_TOOL_APP_NAME=$PREFIX-app-querytool-$ENV # TODO: make this DRY
}

run_deploy () {
  azure_env=$1
  # shellcheck source=./iac/env/tts/dev.bash
  source "$(dirname "$0")"/../iac/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/../iac/iac-common.bash
  verify_cloud
  set_constants

  # Don't publish at solution level as it will publish both src and tests
  # to the same directory resulting in a failed deployment:
  # https://github.com/dotnet/sdk/issues/7238
  pushd "$(dirname "$0")"/src/Piipan.QueryTool

  echo "Publishing project"
  dotnet publish -o ./artifacts
  echo "Deploying to Azure Environment ${azure_env}"
  pushd ./artifacts
    zip -r querytool.zip .
  popd
  az webapp deployment \
    source config-zip \
    -g "$RESOURCE_GROUP" \
    -n "$QUERY_TOOL_APP_NAME" \
    --src ./artifacts/querytool.zip

  popd
}

main "$@"
