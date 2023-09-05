# Shared Test Fixtures

This project is for keeping all database connection configuration, database setup, and database teardown in one place for the various Integration tsst suites throughout the system.

There are multiple databases throughout the system. Except for the Match Orchestrator, subsystems typically touch one database. Therefore these fixtures are organized per database. A test suite may extend one of these base fixtures for its own querying and data conversion purposes.

It's up to each test suite to provide its own environment variables for database connection. Variables are specified in the setup of each fixture class.
