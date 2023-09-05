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

  orch_function_apps=($(get_resources $ORCHESTRATOR_API_TAG "$MATCH_RESOURCE_GROUP"))

  for app in "${orch_function_apps[@]}"
  do
    echo "Publish ${app} to Azure Environment ${azure_env}"
    pushd ./src/Piipan.Match/Piipan.Match.Func.Api
      func azure functionapp publish "$app" --dotnet
    popd
  done

  match_res_function_apps=($(get_resources $MATCH_RES_API_TAG "$MATCH_RESOURCE_GROUP"))

  for app in "${match_res_function_apps[@]}"
  do
    echo "Publish ${app} to Azure Environment ${azure_env}"
    pushd ./src/Piipan.Match/Piipan.Match.Func.ResolutionApi
      func azure functionapp publish "$app" --dotnet
    popd
  done
}

main "$@"
