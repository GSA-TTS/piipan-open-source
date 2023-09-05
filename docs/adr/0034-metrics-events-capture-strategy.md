# 34. Metrics Events Capture Strategy

Date: 2022-08-23

## Status
 
Proposed

## Context

As the NAC continues to evolve and more states begin onboarding, there will be more interest in system metrics. We need to apply a consistent approach to capturing, persisting, and providing these metrics. Metrics and reporting have not been discussed in great detail yet but growing levels of usage of the NAC will drive discussions about metrics and the types of data stakeholders want to query and review.

## Decision

Until metrics reporting requirements can be better fleshed out, we will continue to summarize metrics data in the database rather than collecting a running log of events. We should revisit this decision when we have a better understanding of expected query patterns, the types of reports stakeholders are interested in, BI tools we utilize, etc. But for now, this approach allows us to provide simple summary reports to users, prioritizing read performance over write performance, without creating much overhead work for developers.

## Consequences

We will continue generating custom events that better support simple, user-facing summaries or reports, rather than system events that may require code for aggregating, custom query/filter support, data manipulation, or data visualization prior to user presentation. We will store summarized metrics instead of a running log of events. This means we will be issuing a combination of insert & update queries to maintain summary records rather than ONLY issuing a series of insert queries.

We will continue with a 1-1 relationship between a specific metric needing to be captured and a function within the Metrics Collect Function App. The logic for this work will consist of constructing the custom event, publishing the event, collecting it, and persisting it.

We will also continue with a 1-1 relationship between an API endpoint and a function within the Metrics API Function App. This logic will retrieve metrics data and provide it within an API response.

