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

  states_function_apps=($(get_resources $STATES_API_TAG "$RESOURCE_GROUP"))

  for app in "${states_function_apps[@]}"
  do
    echo "Publish ${app} to Azure Environment ${azure_env}"
    pushd ./src/Piipan.States.Func.Api
      func azure functionapp publish "$app" --dotnet
    popd
  done

}

main "$@"
