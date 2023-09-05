#!/usr/bin/env bash
#
# Creates participant records tables and their access controls for
# each configured state. PGHOST, PGUSER, PGPASSWORD must be set.
#
# usage: apply-ddl.bash

# shellcheck source=./iac/iac-common.bash
source "$(dirname "$0")"/../iac/iac-common.bash || exit

set -e

# PGUSER and PGPASSWORD should correspond to the out-of-the-box,
# non-AD "superuser" administrtor login
set -u
: "$PGHOST"
: "$PGUSER"
: "$PGPASSWORD"
: "$ENV"

# Azure user connection string will be of the form:
# administatorLogin@serverName
SUPERUSER=${PGUSER%@*}

export PGOPTIONS='--client-min-messages=warning'

main () {
  azure_env=$1

  # Create copy of liquibase.properties template file and replace user/pw tokens with actual values
  cp ../iac/databases/sample.liquibase.properties ./liquibase.properties
  sed -i -e "s/<NAC-USERNAME>/${PGUSER}/" -e "s/<NAC-PASSWORD>/${PGPASSWORD}/" ./liquibase.properties

  while IFS=, read -r abbr _; do
    db=$(echo "$abbr" | tr '[:upper:]' '[:lower:]')
    owner=$db
    admin=$(state_managed_id_name "$db" "$ENV")
    admin=${admin//-/_}

    liquibase --changeLogFile=../iac/databases/participants/master-changelog.xml \
    --url=jdbc:postgresql://"${PGHOST}":5432/"${db}"\
    --liquibase-schema-name=piipan \
    update -Downer="${owner}" -Dadmin="${admin}" -Dreader=readonly -Dsuperuser="${SUPERUSER}"

  done < ../iac/env/"${azure_env}"/states.csv

  rm ./liquibase.properties
}

main "$@"
