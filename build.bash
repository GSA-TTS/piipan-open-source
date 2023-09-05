#!/usr/bin/env bash
#
# Builds all projects with optional testing and app deployment
# Relies on a solutions file (sln) in each subsystem root directory
# See build-common.bash for usage details
# All flags/arguments are passed to subprocesses

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/tools/common.bash || exit

run_all() {
  subsystems=(components dashboard etl match metrics notifications participants query-tool shared states support-tools maintenance)

  for s in "${subsystems[@]}"
  do
    pushd "$(dirname "$0")"/"$s"/
      echo "${s}"
      ./build.bash "$@"
    popd
  done

  script_completed
}

run_all "$@"
