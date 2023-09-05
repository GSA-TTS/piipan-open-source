# 20. Storing and Converting Time

Date: 2021-09-16

## Status

Superceded by [26. Use Timestamptz (with time zone) for PostgreSQL DateTime storage](0026-use-timestamptz.md)

## Context

We use Postgresql for our databases. Postgres has two ways of storing times and timestamps: `without time zone` (default) and `with time zone`. These types [convert time zones differently](https://www.postgresql.org/docs/11/datatype-datetime.html) to and from the database.

Being consistent with how we store times throughout our databases should reduce complexity and cognitive overhead when working with times. Our users will be spread across all states and territories, so it's important to derive local time zones as easily and unambiguously as possible when displaying to users. So we should pick one data type or the other, but not use both.

One way to decide is going with what Postgres prefers, but what it prefers is unclear. While `timestamp` (without time zone) is the default data type, the `typeispreferred` setting for date and time data types is `timestampz` (with time zone), which means that time-related data conversions will default to this type unless specified otherwise. The Postgres "Don't Do This" [Wiki](https://wiki.postgresql.org/wiki/Don't_Do_This#Don.27t_use_timestamp_.28without_time_zone.29) states not to use `timestamp`, athough claims it's permittable in cases of simple app retrieval, which is our use case. The Wiki also states not to use `timestamp` to store UTC values as there are consequences when converting time zones within sql (details in our [Consequences](#consequences) section).

In addition, Daylight Savings Time complicates the use of `timestampz`, as UTC offset can change depending on what time of year the data was stored.


### Converting to local time in Dotnet Apps

A `DateTime` in Dotnet has a `Kind` [property](https://docs.microsoft.com/en-us/dotnet/api/system.datetime.kind?view=netcore-3.1) that indicates whether the time is based on local time, UTC, or neither. When a new `DateTime` instance doesn't explicitly specify a `Kind`, the default is `Unspecified` (neither).

Datetimes with unspecified Kinds can cause ambiguity when trying to convert times elsewhere, so it's best to specify Kind whenever possible.

## Decision

In Postgres, we will store `time` and `timestamp` values in the default manner `without time zone`. All times stored are assumed to be UTC. This means times should be converted to UTC before they are saved.

Over API's, all times will be in UTC.

When it's converted to a local time zone, a timestamp should travel as little as possible through an application to avoid possible downstream reconversion. For web apps, this usually means keeping the timestamp in UTC and converting it to local time only when rendering it as html.

When creating new DateTime instances in Dotnet, specify the Kind property.

## Consequences

While we've documented this convention, as this [Wiki](https://wiki.postgresql.org/wiki/Don't_Do_This#Don.27t_use_timestamp_.28without_time_zone.29_to_store_UTC_times) states, there's no way for the database to be aware of this convention.

Also from the [Wiki](https://wiki.postgresql.org/wiki/Don't_Do_This#Don.27t_use_timestamp_.28without_time_zone.29_to_store_UTC_times), this adds an extra step whenever time zones are being converted in sql itself. For example, with `timestampz` one could do:
```sql
SELECT my_timestamptz AT TIME ZONE 'US/Hawaii';
```
But with `timestamp` one would need to convert to UTC before re-converting to the intended time zone:
```sql
SELECT my_timestamp AT TIME ZONE 'UTC' AT TIME ZONE 'US/Hawaii'
```

We don't anticipate ever having to perform a query like this, since we're deciding to hold off on time zone conversion until much later in the stack.
