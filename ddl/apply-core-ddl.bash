#!/usr/bin/env bash
#
# Creates collaboration and metrics records tables and their access controls. 
#
# usage: apply-core-ddl.bash <azure-env>

set -e

export PGOPTIONS='--client-min-messages=warning'

set_constants () {
  DB_ADMIN_NAME=piipanadmin
  PG_SECRET_NAME=core-pg-admin
  METRICS_DB_NAME=metrics
  COLLAB_DB_NAME=collaboration
  set_defaults
}

main () {
  azure_env=$1

  # shellcheck source=./iac/env/tts/dev.bash
  source "$(dirname "$0")"/../iac/env/"${azure_env}".bash  
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/../iac/iac-common.bash
  # shellcheck source=./iac/db-common.bash
  source "$(dirname "$0")"/../iac/db-common.bash
  verify_cloud

  set_constants

  secret=$(\
    az keyvault secret show \
      --name "${PG_SECRET_NAME}" \
      --vault-name "${VAULT_NAME}" \
      --query value \
      --output tsv \
      || echo "")

  db_set_env "$RESOURCE_GROUP" "$CORE_DB_SERVER_NAME" "$DB_ADMIN_NAME" "$secret"

  # Create copy of liquibase.properties template file and replace user/pw tokens with actual values
  cp ../iac/databases/sample.liquibase.properties ./liquibase.properties
  sed -i -e "s/<NAC-USERNAME>/${PGUSER}/" -e "s/<NAC-PASSWORD>/${secret}/" ./liquibase.properties

  echo "Creating $METRICS_DB_NAME database and applying DDL"
  db_init "${METRICS_DB_NAME}" "${DB_ADMIN_NAME}"
  liquibase --changeLogFile=../iac/databases/metrics/master-changelog.xml \
    --url=jdbc:postgresql://"${PGHOST}":5432/"${METRICS_DB_NAME}" update

  # echo "Creating $COLLAB_DB_NAME database and applying DDL"
  db_init "${COLLAB_DB_NAME}" "${DB_ADMIN_NAME}"
  liquibase --changeLogFile=../iac/databases/collaboration/master-changelog.xml \
      --url=jdbc:postgresql://"${PGHOST}":5432/"${COLLAB_DB_NAME}" update 

  rm ./liquibase.properties
}

main "$@"
