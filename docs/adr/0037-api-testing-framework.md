# 3. API Testing framework

Date: 2022-10-10

## Status

Proposed

## Context

We would like to have a default programming language/base framework for our automated API testing so we can have common tooling and practices across sub-systems. While our partner agency has expressed that they have a strong preference for the Microsoft ecosystem, that technology stack does not offer a lot of options for API testing. Instead, we explored two alternative options 1) writing tests with Postman (and Javascript) and 2) writing tests with RestAssured and Java.

## Decision

We will use Postman and Javascript for automating our API tests. Postman is already used by our partner agency and they have a license for it. The NAC team itself already uses it for manual API testing and the team is already familiar with Javascript as some of the front-end is written with Javascript.

RestAssured was too heavy-handed. None of the current NAC development is in Java so this would introduce an entirely new language our partner agency would need to learn and support. Developing these tests would require developers to write Java source code to implement tests. Developing a custom testing framework would additionally require code for a common set of reusable elements. Developers would need to familiarize themselves with Java, the RestAssured libraries, and the tools and IDEs necessary for Java development. They would need to work with agency technical support to download and install these tools. 

## Consequences

-  The dev team is already familiar with Javascript which reduces the need for any training or onboarding. This familiarity allows the entire team of developers to contribute to writing & maintaining tests, not just a QA automation team.
- Postman doesn't require the team to learn/use Java and a different suite of tools. The team is already very familiar with Postman while using it for manual API tests.
- Less overhead is necessary for writing & maintaining test suites in Postman than writing a custom framework for tests with RestAssured and Java.
- Postman is already licensed for use by our partner agency.
- Postman provides a low code option that makes it easier for even non-developers to write tests.
- Postman allows tests to be easily exteneded for use in Performance and Load testing with K6.
- We can utilize Newman for running Postman test collections from command line. Newman also easily integrates with CI & Build systems.
- We can import our OpenAPI specs into Postman for API testing.
- This should allow other technology teams at our partner agency to more easily understand and maintain the system after the Ventera/18F engagement ends.
- The team will develop conventions and standard designs/patterns for structuring, storing, running Postman test scenarios.

## References
* [RestAssured](https://rest-assured.io/)
* [Postman](https://www.postman.com/api-platform/api-testing/)
* [Newman](https://learning.postman.com/docs/running-collections/using-newman-cli/command-line-integration-with-newman/)
* [K6 Load Testing](https://k6.io/blog/load-testing-with-postman-collections/)