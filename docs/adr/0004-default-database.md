# 4. Default database

Date: 2020-10-22

## Status

Accepted

## Context

We will likely need several relational database cluster instances (and likely per-state logical databases) in order to provide good data isolation within the system. However, usage within an individual database is expected to be fairly basic and undemanding.

## Decision

We have decided to use Azure Database for PostgreSQL across the system and maximize the use of database-agnostic SQL.

## Consequences

- Azure Database for PostgreSQL, being based on an open source platform, is about 1/3 the hourly price of Azure SQL Database.
- While it is part of the Microsoft ecosystem, we do not have data on whether or not Azure Database for PostgreSQL is commonly used at the partner agency. By using database-agnostic SQL, we aim to mitigate the risk of introducing a new tool into the partner agency's environment.
- [PostgreSQL is 18F's default datastore](https://engineering.18f.gov/datastore-selection/) – this eases engineering onboarding during the engagement.
