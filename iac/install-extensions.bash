#!/usr/bin/env bash
#
# Installs Azure CLI extensions required for create-resources.bash.
#
# usage: install-extensions.bash

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit

# Array of extension names
declare -a extensions=("db-up" "front-door" "application-insights")

for name in "${extensions[@]}"; do
  echo "Installing '$name'..."
  az extension add --name "$name"
done
