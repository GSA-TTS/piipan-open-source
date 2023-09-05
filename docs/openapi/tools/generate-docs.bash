#!/usr/bin/env bash
#
# Validates and bundles Open Api spec for duplicate participation API
# into a single openapi markdown file to serve as API documentation.
#
# Requires swagger-cli (https://github.com/APIDevTools/swagger-cli)
# Requires widdershins cli (https://github.com/Mermade/widdershins/tree/master)
#
# You can modify markdown templates as specified by the --user_templates flag
# of the widdershins command:
# https://github.com/Mermade/widdershins#templates
#
# usage (from project root):
# cd docs/openapi
# ./tools/generate-docs.bash

set -e
set -u

main () {
  specs=(bulk-api duplicate-participation-api)

  ./tools/generate-specs.bash

  for s in "${specs[@]}"
  do
    pushd ./generated/"${s}"/
    widdershins \
        --language_tabs 'shell:curl:request' \
        --omitBody \
        --omitHeader \
        --resolve \
        --user_templates "../../tools/widdershins_templates/openapi3" \
        openapi.yaml \
        -o openapi.md
    popd
  done
}

main
