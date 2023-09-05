# 38. Use Polly for retry

Date: 2023-01-12

## Status

Proposed

## Context

Network services can fail or become temporarily unreachable unexpectedly. This is especially true when running code on cloud providers and more often than not these things are out of our control. 

The Wait and Retry policy lets you pause before retrying, a great feature for scenarios where all you need is a little time for the problem to resolve. A well-designed system should support some reasonable level of resiliency in communication links between components. One of many ways to support reliable connections and commands via configurable retry policies.

using Polly, a library which allows code to be more resilient to failure via retry, circuit breaker and other fault-handling policies. The neat thing about Polly is that you can intertwine multiple policies together to support just about any scenario you may have. 

## Decision

 Polly can retry failed connections/requests, cache previous responses, protect your resources, prevent you from making requests to broken services, terminate requests that are taking too long and return a default value when all else fails. It’s also thread safe and works on sync and async calls. 
 
 Implement Polly pattern to enable retries as needed.

## Consequences

-  The retry policy should be tuned to match the business requirements of the application and the nature of the failure.
 

## References
* [Retry Pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/retry/)
* [Polly](https://sergeyakopov.com/reliable-database-connections-and-commands-with-polly/) 