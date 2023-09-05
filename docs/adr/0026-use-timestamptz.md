# Use Timestamptz (with time zone) for PostgreSQL DateTime storage

## Status

Accepted/Implemented

Supercedes [20. Storing and Converting Time](0020-storing-and-converting-time.md)

## Context

Npgsql is our chosen ADO.NET data provider and used to handle datetimes coming to and from the PostgreSQL databases throughout the system. The Version 6.0 release contained [major changes to timestamp mapping](https://www.npgsql.org/doc/release-notes/6.0.html#major-changes-to-timestamp-mapping), as well as recommendations for using [PostgreSQL's](https://www.postgresql.org/docs/11/datatype-datetime.html) `timestamptz` (with time zone) data type over `timestamp` for datetimes in order to work more effectively with this mapping.

This recommendation conflicted with a [previous decision](./0020-storing-and-converting-time.md) to prefer `timestamp` without time zone when storing DateTimes.

## Decision

Based on this major version upgrade, we have converted all `timestamp` PostgreSQL fields to `timestamptz`.

## Consequences

Stored DateTimes are still intended to be UTC only, so we will ensure that we always use `DateTime with Kind=Utc` or `or DateTimeOffset with offset 0` when interacting with Npgsql, in accordance with Npgsql's recommendations.

We often use PostgreSQL's `now()` function to insert timestamps, which by default is based on the database server's time zone setting. We assume the default time zone setting for Microsoft Azure servers is UTC, but this hasn't been confirmed. As an extra precaution, we add a time zone setting to our instances of `now()` to be `now() at time zone 'utc'`, although this may end up being excessive.

Other instances of timestamp database insertion are from `EventGridEvent.EventTime` which are confirmed to be in UTC, so there was no need to update any application code for this upgrade.

### Potential future scenario

As it stands, since we do not set a `DateTimeKind` for ParticipantUpload `uploadedAt` fields, it is set to `Unspecified`. Per the [Npgsql release notes](https://www.npgsql.org/doc/release-notes/6.0.html#detailed-notes), Unspecified is sent to PostgreSQL as `timestamp` without any time zone information. When that time is selected out of PostgreSQL, it comes back as `DateTimeKind.Utc`.

The theoretical issue is that if the tests or applications were run on a machine with a time zone set to something other than UTC, `uploadedAt` would represent the local time but PostgreSQL would handle it (both when reading and writing) like it's UTC.

Since the "real world" use case for the metrics collector involves `EventGridEvent.EventTime` (which as noted is UTC), we suspect there are no bugs currently missing at the time of this writing.
