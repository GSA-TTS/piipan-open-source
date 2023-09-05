#!/usr/bin/env bash
#
# Update package dependencies for C# projects (.csproj), recursively
# from the indicated path.
# Only minor updates are considered, unless --highest-major is specified.
#
# usage: update-packages.bash <path> [--highest-major]

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/common.bash || exit

restore () {
  local options=(-v q)
  if [ -n "${1:-}" ]; then
    options+=("$1")
  fi

  dotnet restore "${options[@]}" > /dev/null
}

update () {
  local options=(--outdated)
  if [ "$1" = "--highest-major" ]; then
    : # noop as this is default behavior for `dotnet list package`
  else
    options+=("$1")
  fi

  # We've set pipefail, but don't want grep to result in error code for this
  # whole pipeline if no new packages are found; see https://unix.stackexchange.com/a/581991
  list=$(dotnet list package "${options[@]}" | tr -s ' ' | { grep '>' || test $? = 1; } | cut -f 3,6 -d ' ')
  while IFS=, read -r line; do
    trimmed=$(echo "$line" | tr -d '[:space:]')
    if [ "$trimmed" != "" ]; then
      package=$(echo "$line" | cut -f1 -d ' ')
      version=$(echo "$line" | cut -f2 -d ' ')

      dotnet add package --version "$version" "$package"
    fi
  done <<< "$list"
}

main () {
  start_path="$1"
  local options=("--highest-minor")
  if [ "$#" = "2" ]; then
    options=("$2")
  fi

  # Sort so `src` projects are updated before `tests` projects
  projects=$(find "$start_path" -name "*.csproj" | sort)

  # Different dependency chains will cause errors or missed updates if
  # individual projects are restored, updated, and force-restored one by
  # one. Avoid this issue by running each operation against all projects
  # before proceeding to the next.
  operations=("restore" "update ${options[@]}" "restore --force-evaluate")

  for op in "${operations[@]}"
  do
    while IFS=, read -r csproj; do
      dn=$(dirname "$csproj")

      pushd "$dn" > /dev/null
        $op
      popd > /dev/null
    done <<< "$projects"
  done

  script_completed
}

main "$@"
