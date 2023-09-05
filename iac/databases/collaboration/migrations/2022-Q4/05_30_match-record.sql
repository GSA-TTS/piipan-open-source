BEGIN;

DO $$ BEGIN
    CREATE TYPE hash_type AS ENUM ('ldshash');
EXCEPTION
    WHEN duplicate_object THEN
        RAISE NOTICE 'hash_type ENUM already exists, skipping';
END $$;

CREATE TABLE IF NOT EXISTS matches(
    id serial PRIMARY KEY,
    match_id text UNIQUE NOT NULL,
    created_at timestamptz NOT NULL,
    initiator text NOT NULL,
    states text[2] NOT NULL,
    hash text NOT NULL,
    hash_type hash_type NOT NULL default 'ldshash',
    input jsonb,
    data jsonb NOT NULL
);

COMMENT ON TABLE matches IS 'Match records';
COMMENT ON COLUMN matches.match_id IS 'Match record''s human-readable unique identifier.';
COMMENT ON COLUMN matches.created_at IS 'Match record''s creation date/time.';
COMMENT ON COLUMN matches.initiator IS 'Match record''s initiating entity.';
COMMENT ON COLUMN matches.states IS 'State/territory pair involved in match.';
COMMENT ON COLUMN matches.hash IS 'Value of hash used to identify match.';
COMMENT ON COLUMN matches.hash_type IS 'Type of hash used to identify match.';
COMMENT ON COLUMN matches.input IS 'Incoming data from real-time match request.';
COMMENT ON COLUMN matches.data IS 'Response data from match request.';

CREATE TABLE IF NOT EXISTS match_res_events(
    id serial PRIMARY KEY,
    match_id text NOT NULL REFERENCES matches (match_id),
    inserted_at timestamptz NOT NULL DEFAULT (NOW() AT TIME ZONE 'utc'),
    actor text NOT NULL,
    actor_state text,
    delta jsonb NOT NULL,
    notified_at timestamptz NULL DEFAULT(NOW() AT TIME ZONE 'utc')
);

CREATE INDEX IF NOT EXISTS index_match_id_on_match_res_events on match_res_events (match_id);
CREATE INDEX IF NOT EXISTS index_notified_at_on_match_res_events on match_res_events (notified_at);


COMMENT ON TABLE match_res_events IS 'Match resolution events';
COMMENT ON COLUMN match_res_events.match_id IS 'References match ID of original match';
COMMENT ON COLUMN match_res_events.actor IS 'the person or automated system performing this change';
COMMENT ON COLUMN match_res_events.actor_state IS 'indicates if the actor is associated with a state involved in the match';
COMMENT ON COLUMN match_res_events.delta IS 'json object representing data changes submitted by states, as well as stateful domain data like match status';
COMMENT ON COLUMN match_res_events.notified_at IS 'The date/time that the notification was created for the match update';

COMMIT;
