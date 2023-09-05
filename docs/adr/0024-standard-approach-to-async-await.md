# 24. Standard approach to async await

Date: 2022-01-05

## Status

Accepted

## Context

When wriitng asynchronous code in .NET, there are often multiple ways to achieve the same or similar result, sometimes with competing benefits and drawbacks. In an effort to follow best practices and to maintain consistency across our code base, we would like adopt a standard approach for asynchronous programming.

## Decision

We will adopt the standards outlined in [this guide](https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md#asynchronous-programming). We will avoid reiterating the specific practices in this decision record, but upon discovering this guide, we found the following two suggestions especially applicable to our code base:
- [Never use `async void`](https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md#async-void)
- [Prefer `async/await` over directly returning `Task`](https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md#prefer-asyncawait-over-directly-returning-task)

## Consequences

Adopting these best practices for asynchronous programming will make our code base more consistent and easier for developers to understand. We will also benefit from alignment with the broader .NET community, making documentation and code examples more directly applicable to ours. Lastly, we expect to avoid certain pitfalls associated with the to-be-avoided patterns described in this guide.