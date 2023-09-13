# Quickstart Integration Guide for Tenants

This guide is to help development teams integrate their systems with piipan. 

## Overview



### Uploading De-identified data to piipan
Follow these steps to provide the daily de-identified data information that piipan will need to detect possible duplicate data in your tenant:

1. Understand the piipan material in the [system overview](/README.md#overview) and our [introduction to our Privacy-Preserving Record Linkage approach](/docs/pprl-plain.md).
2. Understand which de-identified data are considered [`active e-identified data`](/etl/docs/bulk-import.md#definition-of-active-participants).
3. Export active data from your system to a plain text CSV file.
    1. Exclude entries that are [missing key data fields](/etl/docs/bulk-import.md#participant-records-to-exclude) or are not considered [active e-identified data](/etl/docs/bulk-import.md#definition-of-active-participants)
4. Transform the plain text CSV to the [Bulk Upload CSV format](../etl/docs/bulk-import.md), in accordance with the [Personal Identifiable Information (PII) de-identification](./pprl.md) specification.
5. Integrate with the [Bulk Upload API](./openapi/generated/bulk-api/openapi.md) to submit the CSV to piipan using the `/upload` operation.

### Conduct searches against piipan
As a part of each certification, recertification, and addition of a household member, take the following steps:

1. Determine which individuals need to be included in the piipan search:
    1. For applications and recertifications, all applicants in the household should be included in piipan searches.
    2. For additions of household members, only the individuals being added to the household should be included in piipan searches.  Members of the household who have already been certified should not have new piipan searches performed.
2. De-identify PII of the individuals using the [PII de-identification specification](./pprl.md)
3. Integrate with the Duplicate Participation API's `/find_matches` call to conduct searches using the de-identified PII from the previous step.

### Resolve matches identified by piipan
At this time, no integration steps are needed to resolve matches. Case workers can log in to piipan website to learn about matches and record resolutions.

## Introduction to piipan APIs
The piipan provides 2 web service APIs for tenant integrations:

1. [Bulk Upload API](./openapi/generated/bulk-api/openapi.md)
1. [Duplicate Participation API](./openapi/generated/duplicate-participation-api/openapi.md)

Each API has one or more RPC or REST operations and uses JSON in the operation request and/or response bodies. All operations must be made over HTTPS and authenticated by an API key. Each tenant will be issued a key for the Bulk Upload API and a separate key for the Duplicate 

## Usage notes

### De-identification testing
- Correct de-identification in accordance with our defined process is critical for cross-tenant matching. We strongly recommend unit testing your de-identification code, [covering the specific normalization and validation scenarios we describe](./pprl.md). The piipan team is exploring strategies to verify tenant-performed de-identification in an automated, ongoing fashion.

### Record retention
- Save API responses received from the duplicate participation API for 3 years.

### Failed requests
- Bulk uploads can be resubmitted as required; the most recent upload will overwrite any pre-existing de-identified data snapshot.


## Feedback

Need to report a defect? We track API issues through GitHub.

Have a question, or want to work through a technical issue? Start a thread in our Microsoft Teams channel or reach out to us by email.
