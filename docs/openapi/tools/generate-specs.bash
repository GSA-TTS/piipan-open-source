#!/usr/bin/env bash
#
# Validates a decomposed YAML OpenAPI spec and bundles it into a
# single openapi.yaml file.
#
# Requires swagger-cli (https://github.com/APIDevTools/swagger-cli)
#
# usage: generate-spec.bash

set -e
set -u

main () {
    specs=(bulk-api duplicate-participation-api)

    for s in "${specs[@]}"
    do
        spec_path="./${s}.yaml"
        generated_path="./generated/${s}/openapi.yaml"

        echo "Validating ${spec_path}"
        swagger-cli validate "$spec_path"

        echo "Generating ${generated_path}"
        swagger-cli bundle "$spec_path" -o "$generated_path" -t yaml
    done
}

main
