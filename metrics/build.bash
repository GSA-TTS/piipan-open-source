#!/usr/bin/env bash
#
# Builds project with optional testing and app deployment
# Relies on a solutions file (sln) in the subsystem root directory
# See build-common.bash for usage details

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit
# shellcheck source=./tools/build-common.bash
source "$(dirname "$0")"/../tools/build-common.bash || exit

run_deploy () {
  azure_env=$1
  # shellcheck source=./iac/env/tts/dev.bash
  source "$(dirname "$0")"/../iac/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/../iac/iac-common.bash

  verify_cloud

  echo "Publish ${METRICS_COLLECT_APP_NAME} to Azure Environment ${azure_env}"
  pushd ./src/Piipan.Metrics/Piipan.Metrics.Func.Collect
    func azure functionapp publish "$METRICS_COLLECT_APP_NAME" --dotnet
  popd

  echo "Publish ${METRICS_API_APP_NAME} to Azure Environment ${azure_env}"
  pushd ./src/Piipan.Metrics/Piipan.Metrics.Func.Api
    func azure functionapp publish "$METRICS_API_APP_NAME" --dotnet
  popd
}

main "$@"
