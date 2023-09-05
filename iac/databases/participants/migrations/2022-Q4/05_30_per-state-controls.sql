BEGIN;

-- Creates access controls for participant records tables
-- Assumes 4 database roles, to be set via the psql -v option:
--  * cluster `superuser`
--  * database `owner`, which owns the tables, sequences
--  * database `admin`, which gets read/write access
--  * database `reader`, which gets read-only access

SET search_path=piipan,public;

-- "superuser" account under Azure is not so super
GRANT ${owner} to ${superuser};

ALTER TABLE uploads OWNER TO ${owner};
ALTER TABLE participants OWNER TO ${owner};

ALTER SEQUENCE participants_id_seq OWNER TO ${owner};
ALTER SEQUENCE uploads_id_seq OWNER TO ${owner};

GRANT SELECT, INSERT, UPDATE, DELETE ON participants TO ${admin};
GRANT SELECT, INSERT, UPDATE, DELETE ON uploads TO ${admin};
GRANT USAGE, SELECT, UPDATE ON participants_id_seq TO ${admin};
GRANT USAGE, SELECT, UPDATE ON uploads_id_seq TO ${admin};

GRANT SELECT ON participants TO ${reader};
GRANT SELECT ON uploads TO ${reader};

-- restore privileges
REVOKE ${owner} FROM ${superuser};

COMMIT;
