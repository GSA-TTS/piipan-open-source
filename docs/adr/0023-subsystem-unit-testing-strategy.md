# 23. Subsystem unit testing strategy

Date: 2022-01-03

## Status

Accepted

## Context

Piipan is made up of six primary subsystems (`dashboard`, `etl`, `match`, `metrics`, `participants`, `query-tool`), each of which are modeled on our [standard subsystem software architecture](0018-standardize-subsystem-software-architecture.md). In an effort to fully and effectively test these subsystems, we would like to similarly adopt a standard unit stesting approach.

## Decision

Each project within a subsystem (with the exception of `*.Api`, if present) will have a corresponding `xUnit` test project located in the top-level `tests` directory (the `tests` directory structure should match that of `src`).

Each source file (with the exception of `interfaces` and `POCOs`) will have a corresponding test file. A given test file (e.g. `ParticipantServiceTests.cs`) should strive to fully test the executable code of the target file (e.g. `ParticipantService.cs`), while minimizing the execution of executable code that lives outside the target file. This is best achieved through a combination of dependency injection and mocking.

There will be special cases where mocking all external functionality is not possible. This most commonly occurs when there are dependencies on extension methods and/or third-party or system classes which contain non-virtual methods (in both cases, these components cannot be mocked because they are non-overridable).

## Consequences

We have seen that mocking all external dependencies when unit testing greatly reduces the effort required to achieve maximal test coverage. 

Additionally, mocking external dependencies makes the tests we write more robust -- changes to the internal behavior of dependencies does not necessitate updates to the tests. 

This approach to unit testing validates the internal functionality of the individual components, but does not test the relationships between components. Therefore, a separate integration test suite is necessary in order to verify that the system as a whole satisfies functional requirements. 