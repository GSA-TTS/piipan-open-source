# 8. Use xUnit.net for unit testing

Date: 2020-11-23

## Status

Accepted

## Context

We would like a default unit testing framework for our server-side components that enables and encourages common testing practices across our sub-systems and repos. Our [default programming framework is .NET Core](0003-programming-framework.md) and we considered the three most common .NET unit testing frameworks:

- [xUnit.net](https://xunit.net/) (xUnit)
- [NUnit](https://nunit.org/)
- [MSTest](https://en.wikipedia.org/wiki/Visual_Studio_Unit_Testing_Framework)

## Decision

We will use xUnit as our default unit testing framework.

## Consequences

- We will be able to leverage [prior 18F experience](https://gsa-tts.slack.com/archives/CCRJXQ74K/p1561050449005600) with xUnit.
- xUnit will encourage good test-writing habits through its [test isolation practices](https://xunit.net/docs/why-did-we-build-xunit-1.0#lessons-learned).
- xUnit is well documented and used frequently in [Microsoft's own docs](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-code-coverage?tabs=linux).
- MSTest's main advantage is tight integration with Visual Studio which 18F developers tend not to use.
- Given our current needs, the remaining differences between xUnit, NUnit, and MSTest are not large enough to supersede the prior points.

## References

- [Unit Test Frameworks for C#: The Pros and Cons of the Top 3](https://stackify.com/unit-test-frameworks-csharp/)
- [AlaskaDHSS / ProtoWebAPI](https://github.com/AlaskaDHSS/ProtoWebApi) — prior 18F project using xUnit.
- [Why Did we Build xUnit 1.0?](https://xunit.net/docs/why-did-we-build-xunit-1.0)


