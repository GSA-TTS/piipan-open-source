#!/usr/bin/env bash
#
# Supporting functionality for subsystem build scripts.
# Purpose of build scripts is to have platform-agnostic scripts
# that can be used for local development, CI, and IAC.
# Each subsystem has a top-level build.bash file that may
# use and/or override any of these functions.
# Build scripts rely on a solutions file (sln) in subsystem root.

# runs the build process
run_build () {
  echo "Running build"
  dotnet build
}

# runs tests
run_tests () {
  echo "Running tests"
  dotnet test
}

# run tests in continuous integration mode
run_tests_ci () {
  echo "Running tests in CI mode"
  dotnet test \
    -p:ContinuousIntegrationBuild=true \
    --collect:"XPlat Code Coverage" \
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=lcov
}

# Builds project with optional testing and app deployment
# Relies on a solutions file (sln) in the subsystem root directory
#
# Arguments:
# [none]                Build project binaries
# test [-c]             Run tests
# deploy -e <azure_env> Deploy to specified Azure Environment (e.g. tts/dev)
#
# Description:
# When passed no arguments, script runs in build mode.
# When deploying, an environment flag [-e] must be passed.
# When testing, an optional flag [-c] can be passed to run in Continuous Integration mode.
#
# Usage:
# ./build.bash
# ./build.bash test
# ./build.bash test -c
# ./build.bash deploy
# ./build.bash deploy -e tts/test
main () {
  mode=${1:-build} # set default mode to "build"
  azure_env=""
  ci_mode="false"

  case "$mode" in
    deploy)
      shift # Remove `deploy` from the argument list
    while getopts ":e:" opt; do
      case ${opt} in
        e )
          azure_env=$OPTARG
          ;;
        \? )
          echo "Invalid Option: -$OPTARG" 1>&2
          exit 1
          ;;
        : )
          echo "Invalid Option: -$OPTARG requires an argument" 1>&2
          exit 1
          ;;
      esac
    done
    shift $((OPTIND -1))
    ;;

    test)
      shift # remove 'test' from argument list
    while getopts ":c" opt; do
      case ${opt} in
        c )
          ci_mode='true'
          ;;
        * )
          echo "usage: test [-c]"
          exit 1
          ;;
      esac
    done
    shift $((OPTIND -1))
    ;;
  esac

  if [ "$mode" = "build" ];   then run_build; fi
  if [[ "$mode" = "test" ]]; then
    if [[ "$ci_mode" = "true" ]]; then
      run_tests_ci
    else
      run_tests
    fi
  fi
  if [[ "$mode" = "deploy" ]]; then
    if [[ "$azure_env" = "" ]]; then
      echo "You must specify an azure environment using the -e flag"
      echo "Example: ./build.bash deploy -e tts/dev"
      exit 1
    else
      run_deploy "$azure_env"
    fi
  fi

  script_completed
}
