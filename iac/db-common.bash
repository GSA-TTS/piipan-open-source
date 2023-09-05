#!/usr/bin/env bash

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit

CURRENT_USER_OBJID=$(az ad signed-in-user show --query id --output tsv)
PSQL_OPTS=(-v ON_ERROR_STOP=1 -X -q)
TEMPLATE_DB=template1

# Store AAD group membership status, default to false
db_aad_exists=false

# Environment setting
db_set_env () {
  local group=$1
  local server=$2
  local user=$3
  local secret=$4

  export PGOPTIONS='--client-min-messages=warning'
  PGHOST=$(az resource show \
    --resource-group "$group" \
    --name "$server" \
    --resource-type "Microsoft.DbForPostgreSQL/servers" \
    --query properties.fullyQualifiedDomainName -o tsv)
  export PGHOST
  export PGPASSWORD=$secret
  export PGUSER=${user}@${server}
  export PGSSLMODE=require
}

# Database configuration
db_init () {
  local db=$1
  local superuser=$2
  local owner=$db

  echo "Baseline $TEMPLATE_DB before creating new databases from it"
  db_config "$TEMPLATE_DB"

  db_create_role "$owner"
  db_config_role "$owner"

  db_create "$db"
  db_set_owner "$db" "$owner" "$superuser"
  db_config "$db"
}

db_create () {
  local db=$1
  psql "${PSQL_OPTS[@]}" -d "$TEMPLATE_DB" -f - <<EOF
    SELECT 'CREATE DATABASE $db TEMPLATE $TEMPLATE_DB'
      WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = '$db')\gexec
EOF
}

db_apply_ddl () {
  local db=$1
  local ddl=$2

  psql "${PSQL_OPTS[@]}" -d "$db" -f "$ddl"
}

db_config () {
  local db=$1
  psql "${PSQL_OPTS[@]}" -d "$db" -f - <<EOF
    REVOKE ALL ON DATABASE $db FROM public;
    REVOKE ALL ON SCHEMA public FROM public;
    CREATE EXTENSION IF NOT EXISTS plpgsql WITH SCHEMA pg_catalog;
EOF
}

db_set_owner () {
  local db=$1
  local owner=$2
  local superuser=$3

  psql "${PSQL_OPTS[@]}" -d "$db" -f - <<EOF
    -- "superuser" account under Azure is not so super; must be a member of the
    -- owner role before being able to create a database with it as owner
    GRANT $owner to $superuser;
    ALTER DATABASE $db OWNER TO $owner;
    REVOKE $owner from $superuser;
EOF
}

# Role configuration
db_create_role () {
  local role=$1
  psql "${PSQL_OPTS[@]}" -d "$TEMPLATE_DB" -f - <<EOF
    DO \$\$
    BEGIN
      CREATE ROLE $role;
      EXCEPTION WHEN DUPLICATE_OBJECT THEN
      RAISE NOTICE 'role "$role" already exists';
    END
    \$\$;
EOF
}

db_config_role () {
  local role=$1
  psql "${PSQL_OPTS[@]}" -d "$TEMPLATE_DB" -f - <<EOF
    ALTER ROLE $role PASSWORD NULL;
    ALTER ROLE $role NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT NOLOGIN;
EOF
}

db_create_managed_role () {
  local db=$1
  local func=$2
  local group=$3
  local role=${func//-/_}

  principal_id=$(\
    az webapp identity show \
      -n "$func" \
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
  psql "${PSQL_OPTS[@]}" -d "$db" -f - <<EOF
    SET aad_validate_oids_in_tenant = off;
    DO \$\$
    BEGIN
      IF EXISTS (
      SELECT rolname FROM pg_catalog.pg_roles
      WHERE  rolname = '$role') 
      THEN
        ALTER ROLE $role PASSWORD '$app_id';    
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

db_config_managed_role () {
  local db=$1
  local func=$2
  local role=${func//-/_}

  psql "${PSQL_OPTS[@]}" -d "$db" -f - <<EOF
    GRANT CONNECT,TEMPORARY ON DATABASE $db TO $role;
    GRANT USAGE ON SCHEMA public TO $role;
    ALTER ROLE $role NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT;
EOF
}

# Database access controls
# Example grant access to all tables
#   db_grant_read "DB_Name" "APP_Name"
# Example grant access to some tables
#   db_grant_read "DB_Name" "APP_Name" "table1, table2"
# Example grant access a table
#   db_grant_read "DB_Name" "APP_Name" "tableName"

db_grant_read () {
  local db=$1
  local func=$2
  local table=${3-'ALL TABLES IN SCHEMA public'}
  local role=${func//-/_}

  psql "${PSQL_OPTS[@]}" -d "$db" -f - <<EOF
    GRANT SELECT ON $table TO $role;
EOF
}

# Database readwrite access control
# Example grant access to all tables
#   db_grant_readwrite "DB_Name" "APP_Name"
# Example grant access to some tables
#   db_grant_readwrite "DB_Name" "APP_Name" "table1, table2"
# Example grant access a table
#   db_grant_readwrite "DB_Name" "APP_Name" "tableName"
db_grant_readwrite () {
  local db=$1
  local func=$2
  local table=${3-'ALL TABLES IN SCHEMA public'}
  local sequence=${3-'ALL SEQUENCES IN SCHEMA public'}
  role=${func//-/_}

  psql "${PSQL_OPTS[@]}" -d "${db}" -f - <<EOF
    GRANT SELECT, INSERT, UPDATE, DELETE, TRUNCATE ON ${table} TO ${role};
    GRANT USAGE, SELECT ON ${sequence} TO ${role};
EOF
}

# Azure Active Directory tools
db_config_aad () {
  local group=$1
  local server=$2
  local aad_admin=$3

  az ad group create --display-name "$aad_admin" --mail-nickname "$aad_admin"

  local aad_admin_objid
  aad_admin_objid=$(az ad group show --group "$aad_admin" --query id --output tsv)
  az postgres server ad-admin create \
    --resource-group "$group" \
    --server "$server" \
    --display-name "$aad_admin" \
    --object-id "$aad_admin_objid"
}

db_use_aad () {
  # Add current user to AAD group, allowing them to grant db access
  # to managed identities
  local server=$1
  local aad_admin=$2

  db_aad_exists=$(az ad group member check \
    --group "$aad_admin" \
    --member-id "$CURRENT_USER_OBJID" \
    --query value -o tsv)

  if [ "$db_aad_exists" = "true" ]; then
    echo "$CURRENT_USER_OBJID is already a member of $aad_admin"
  else
    # Temporarily add current user as a PostgreSQL AD admin
    # to allow provisioning of managed identity roles
    az ad group member add \
      --group "$aad_admin" \
      --member-id "$CURRENT_USER_OBJID"
  fi

  # Authenticate under the AD "superuser" group, in order to create managed
  # identities. Assumes the current user is a member of PG_AAD_ADMIN.
  local aad_pgpassword
  aad_pgpassword=$(az account get-access-token --resource-type oss-rdbms \
    --query accessToken --output tsv)
  export PGPASSWORD=$aad_pgpassword
  export PGUSER=${aad_admin}@${server}
}

db_leave_aad () {
  local aad_admin=$1

  if [ "$db_aad_exists" = "true" ]; then
    echo "Leaving $CURRENT_USER_OBJID as a member of $aad_admin"
  else
    # Revoke assignment of current user as a PostgreSQL AD admin
    az ad group member remove \
      --group "$aad_admin" \
      --member-id "$CURRENT_USER_OBJID"
  fi
}
