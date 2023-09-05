BEGIN;

CREATE TABLE IF NOT EXISTS state_info(
    id text UNIQUE PRIMARY KEY,
    state text UNIQUE NOT NULL,
    state_abbreviation text NOT NULL,
    email text NOT NULL,
    phone text,
    region text NOT NULL,
    email_cc text NULL
);

COMMENT ON COLUMN state_info.id IS 'Unique number for each state. Will be alphabetical - Alabama = 1, Alaska = 2 etc';
COMMENT ON COLUMN state_info.phone IS 'The phone number contact';
COMMENT ON COLUMN state_info.email IS 'The email to contact that state';
COMMENT ON COLUMN state_info.state IS 'State/territory full name';
COMMENT ON COLUMN state_info.state_abbreviation IS 'State/territorys two letter abbreviation';
COMMENT ON COLUMN state_info.region IS 'The region the specified state belongs to';
COMMENT ON COLUMN state_info.email_cc IS 'The email to cc for notifications';

COMMIT;
