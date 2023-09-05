# 7. Blob Storage for bulk upload

Date: 2020-11-03

## Status

Accepted

## Context

One of our early goals in the system is to permit bulk PII to be uploaded, processed, and stored by the system. Authenticated access, restriction by IP network, message-level encryption, and fine-grained, per-state access control are anticipated needs. We would like to maximize our re-use of platform-provided, FedRAMP'd services.

## Decision

We will use Azure Blob Storage and its REST API for our initial bulk PII upload mechanism. 

## Consequences

- Blob Storage has multiple client libraries/tools and good documentation, which will ease integration.
- Azure Functions can easily be triggered by an upload event.
- It offers quite a bit of functionality in excess of what we need; it will be critical to disable those features and access rights.
- As we are making this decision ahead of our discussions with the states and territories, we may have to revisit and implement a more tailored solution.