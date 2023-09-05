#!/usr/bin/env bash
#
# Common preamble for bash scripts.

# Exit immediately if a command fails
set -e

# Unset variables are an error
set -u

# Conform more closely to POSIX standard, specifically so command substitutions
# inherit the value of the -e option. The more targeted inherit_errexit option
# would be preferred, but it is not available in bash 3.2, which ships with macOS.
# TODO This choice can be revisited now that we require bash 4.1 or above.
set -o posix

# Exit immediately if any command in a pipeline errors
set -o pipefail

# Inherit trap on ERR in shell functions, command substitutions, etc.
set -o errtrace

_script=$(basename "$0")

_err_report () {
  # If the error occurred in a while/done loop or in a function, the trap can
  # only report the line number of the loop or the function, not the offending
  # command itself.
  echo "$_script: error on (or around) line $1"
}

script_completed () {
  echo "$_script: completed successfully"
}

# In bash versions prior to 4.1, error line numbers are not reported correctly.
check_version () {
  # Default to zero for really old bash versions
  local full=${BASH_VERSION:-0}
  # Delete version dot separator and anything after it
  local major=${full%%.*}
  # Delete major version
  local minor="${full#*.}"
  # Delete any patch version
  minor="${minor%%.*}"

  if [ "$major" -gt 4 ] || { [ "$major" -eq 4 ] && [ "$minor" -ge 1 ]; } then
    return 0
  fi

  echo "$_script: error: bash 4.1 or greater required, version $full detected"
  exit 1
}

trap '_err_report $LINENO' ERR
check_version
