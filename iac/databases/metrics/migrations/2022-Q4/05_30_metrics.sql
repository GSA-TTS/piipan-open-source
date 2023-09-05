BEGIN;

CREATE TABLE IF NOT EXISTS participant_uploads(
    id serial PRIMARY KEY,
    state VARCHAR(50) NOT NULL,
    uploaded_at timestamptz NOT NULL,
    completed_at  timestamptz,
    status text NOT NULL,
    upload_identifier text,
    participants_uploaded bigint,
    error_message text
);

COMMENT ON TABLE participant_uploads IS 'Participant bulk upload event record';

CREATE TABLE IF NOT EXISTS participant_searches(
    id serial PRIMARY KEY,
    state VARCHAR(50) NOT NULL,
    search_reason VARCHAR(100) NOT NULL,
    search_from VARCHAR(50) NULL,
    match_creation VARCHAR(250) NULL,
    match_count int,
    searched_at timestamptz NOT NULL
);

COMMENT ON TABLE participant_searches IS 'Participant search event record';
CREATE TABLE IF NOT EXISTS participant_matches(
    id serial PRIMARY KEY,
    match_id text UNIQUE NOT NULL,
    match_created_at timestamptz NOT NULL,
    init_state VARCHAR(50) NOT NULL,
    init_state_invalid_match BOOLEAN NULL,
    init_state_invalid_match_reason VARCHAR(250) NULL,
    init_state_initial_action_taken  VARCHAR(250) NULL,
    init_state_initial_action_at  timestamptz NULL,
    init_state_final_disposition VARCHAR(250) NULL,
    init_state_final_disposition_date timestamptz NULL,
    init_state_vulnerable_individual BOOLEAN NULL,
    matching_state VARCHAR(50) NOT NULL,
    matching_state_invalid_match BOOLEAN NULL,
    matching_state_invalid_match_reason VARCHAR(250) NULL,
    matching_state_initial_action_taken  VARCHAR(250) NULL,
    matching_state_initial_action_at  timestamptz NULL,
    matching_state_final_disposition VARCHAR(250) NULL,
    matching_state_final_disposition_date timestamptz NULL,
    matching_state_vulnerable_individual BOOLEAN NULL,
    match_status VARCHAR(50) NOT NULL
    );
COMMENT ON TABLE participant_matches IS 'Participant match event record for reporting purpose';

COMMIT;
