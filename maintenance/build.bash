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
  #MAINTENANCE_APP_NAME=maintenance # TODO: make this DRY
  QUERY_TOOL_APP_NAME=$PREFIX-app-querytool-$ENV
  DASHBOARD_APP_NAME=$PREFIX-app-dashboard-$ENV
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
  pushd "$(dirname "$0")"/src/Piipan.Maintenance

  echo "Publishing query tool project"
  dotnet publish -o ./artifacts
  echo "Deploying to Query Tool Azure Environment ${azure_env}"
  pushd ./artifacts
    zip -r maintenance.zip .
  popd
  az webapp deployment \
    source config-zip \
    -g "$RESOURCE_GROUP" \
    -n "$QUERY_TOOL_APP_NAME" \
    --slot maintenance \
    --src ./artifacts/maintenance.zip


  echo "Deploying to Dashboard Azure Environment ${azure_env}"
  az webapp deployment \
    source config-zip \
    -g "$RESOURCE_GROUP" \
    -n "$DASHBOARD_APP_NAME" \
    --slot maintenance \
    --src ./artifacts/maintenance.zip

  popd
}

main "$@"
