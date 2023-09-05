#!/usr/bin/env bash

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit

# Require PGHOST be set by the caller
: "$PGHOST"

TEMPLATE_DB=template1
APP_SCHEMA=piipan

export PGOPTIONS='--client-min-messages=warning'
PSQL_OPTS=(-v ON_ERROR_STOP=1 -X -q)

create_managed_role () {
  app=$1
  group=$2
  role=${app//-/_}

  principal_id=$(\
    az webapp identity show \
      -n "$app" \
      -g "$group" \
      --query principalId \
      -o tsv)
  app_id=$(\
    az ad sp show \
      --id "$principal_id" \
      --query appId \
      -o tsv)

  # Establish a managed identity role for an application's
  # system-assigned identity.
  psql "${PSQL_OPTS[@]}" -d "$TEMPLATE_DB" -f - <<EOF
    SET aad_validate_oids_in_tenant = off;
    DO \$\$
    BEGIN
      IF EXISTS (
      SELECT rolname FROM pg_catalog.pg_roles
      WHERE  rolname = '$role') 
      THEN
         ALTER ROLE $role WITH PASSWORD '$app_id';  
      ELSE
        BEGIN
          CREATE ROLE $role LOGIN PASSWORD '$app_id' IN ROLE azure_ad_user;
          EXCEPTION WHEN DUPLICATE_OBJECT THEN
          RAISE NOTICE 'role "$role" already exists';
        END;
      END IF;
    END
    \$\$;
EOF
}

config_readonly_role () {
  app=$1
  role=${app//-/_}
  readonly="readonly"

  psql "${PSQL_OPTS[@]}" -d "$TEMPLATE_DB" -f - <<EOF
    GRANT $readonly to $role;
    ALTER ROLE $role NOSUPERUSER NOCREATEDB NOCREATEROLE INHERIT;
    ALTER ROLE $role SET search_path = $APP_SCHEMA,public;
    
EOF
}

main () {
  APP=$1
  RESOURCE_GROUP=$2
  PG_AAD_USER=$3
  source "$(dirname "$0")"/iac-common.bash

  PGPASSWORD=$(az account get-access-token --resource-type oss-rdbms \
    --query accessToken --output tsv)
  export PGPASSWORD
  export PGUSER=$PG_AAD_USER

  create_managed_role "$APP" "$RESOURCE_GROUP"
  config_readonly_role "$APP"

  script_completed
}

main "$@"
