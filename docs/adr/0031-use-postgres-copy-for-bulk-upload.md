# Use Postgres Copy For Bulk Upload

## Status

Accepted / implemented

## Context

Initially for bulk upload, we took the simplest approach of committing all participants in an upload using per-row inserts within a single database transaction. This works fine for small uploads but we anticipated this may not be the appropriate approach for a production-ready system. The expected downsides to this approach are:
1. Performance issues. What is the write performance of a transaction containing millions of records? What is the affect on record retrieval when a million or more records are being committed?
1. Any single bad record/participant would cause an entire state upload (likely millions of participants) to fail without clear indication as to the bad record.
1. Difficulty communicating detailed upload progress to end-users.

### Testing
To gauge the performance, we ran a series of simple tests. We ran uploads with 100k fake participants into an empty database. Because of the distributed nature of our architecture, we did not collect exact timings. We periodically ran a query to get the count of participants in Postgres.

#### Observations
- When Event Grid delivers a message to a Function App, it expects a response within 30 seconds else it will retry message delivery.
- The time it took to insert all records exceeded the 30 second response expectation. This resulted in repeated retries and associated failures following an [exponential backoff retry pattern](https://docs.microsoft.com/en-us/azure/event-grid/delivery-and-retry).
- The Azure Function showed corresponding HTTP server errors for each retry, likely the result of timeout issues related to the Function runtime, database IO, and/or network connections.
- As a result, attempted database transactions were rolled back and records were never created in the database, in neither the Uploads table nor the Participants table.

## Decision

Because of the performance test results, in addition to the other concerns listed above, we plan to enhance the performance of the bulk upload process. We will make the following changes to improve this process:
1. Handle the initial Event Grid upload events by writing a message to an Azure Queue Storage queue
1. Update the Function responsible for loading participants to be triggered by messages from the Queue Storage queue
1. Insert participants into the database using the PostgreSQL `COPY` command
1. Increase the Azure Database for PostgreSQL cluster's storage size, thus increasing it's overall storage and IOPs capacity

Alternative approaches that were also considered:
- Event-based approach using Azure Event Hubs. Broadly speaking, this approach would entail using an Event Hub as the backend pipeline for the bulk upload API and streaming events to multiple Azure function instances (consumers) that handle database transactions. We think this is still a good candidate as the system matures, but have deferred these larger architectural decisions in favor of more immediate progress to MVP.
- Switching the ETL pipeline to use Azure Data Factory. We found the UI-based tooling inconsistent with our desire for simplicity, testability, and developer insight into critical functionality.

## Consequences

- We will limit the number of infrastructure and application changes necessary to meet our MVP requirements for the bulk upload process
- Event Grid event response times should become a non-issue as writing to a Queue Storage queue should be nearly instantaneous
- Sending large amounts of data via `COPY` requires some fine-tuning of database connection details and resources. This may become impractical from a technical and/or pricing perspective as more states are onboarded to the system.
- Using `COPY` functionality gives a significant performance boost over per-row inserts. Larger uploads required us to increase the Connection & Command KeepAlive & Timeout settings but testing demonstrated it was orders of magnitude faster (17s for 100k records vs 30minutes, a couple minutes for 1.5 million records). 
- This solution does not directly address the issue with one bad record causing an entire upload to fail.
- We are coupling our approach to PostgreSQL in a way that past decisions have attempted to avoid. Other database options like Azure SQL support similar bulk insert mechanisms, but our initial approach will be specific to PostgreSQL.
    - Consequently, changing database systems will require a new implementation of the [IParticipantBulkInsertHandler](../../participants/src/Piipan.Participants/Piipan.Participants.Core/DataAccessObjects/IParticipantBulkInsertHandler.cs) interface. The new implementation would need to be [injected into applications](../../participants/src/Piipan.Participants/Piipan.Participants.Core/Extensions/ServiceCollectionExtensions.cs) in place of the [PostgreSQL-specific implementation](../../participants/src/Piipan.Participants/Piipan.Participants.Core/DataAccessObjects/ParticipantBulkInserHandler.cs).
- This solution does not help with setting up future analysis streams. As a result, we are not confident this is a long term solution for bulk imports but it will allow the NAC to handle import sizes of 1.5 million records which are expected with the MVP rollout. We will revisit other options in the future.

## Resources

- [Azure Storage](https://docs.microsoft.com/en-us/azure/postgresql/flexible-server/concepts-compute-storage)
- [Azure Function Timeouts](https://docs.microsoft.com/en-us/azure/azure-functions/functions-scale#timeout)
- [Default Database ADR](./0004-default-database.md)

