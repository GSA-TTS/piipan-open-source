DO $$ BEGIN
    CREATE TYPE hash_type AS ENUM ('ldshash');
EXCEPTION
    WHEN duplicate_object THEN
        RAISE NOTICE 'hash_type ENUM already exists, skipping';
END $$;
