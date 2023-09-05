# 30. Use a Blazor Component Library

Date: 2022-04-05

## Status

Proposed

Related to [29. Use Blazor WebAssembly For Client Applications](0029-use-blazor-web-assembly.md)

## Context

Blazor Components are reusable by design. Simply passing in different parameters to the components allows different behaviors without changing the component at all. We can utilize this behavior and create a component library that will allow developers to see what components are available, how to pull them into their project, as well as what parameters they can pass into the component. The components themselves are being created anyway, so it's not much more work to put them into a component library.

## Decision

A component library project, under a new "components" folder in the main solution should be created. There are three parts to this project: 
1. The component library itself, which will get included into the other web applications. Named: Piipan.Components
2. A demo project of the component library, where the developers can go and see all of the components and their functionality. Named: Piipan.Components.Demo
3. A test project of the component library, written in [bUnit](https://bunit.dev/), where all of the components can be thoroughly tested. Named: Piipan.Components.Tests

## Consequences

* This should speed up development time for the client applications, as well as having a higher level of consistency for all of the UI components. The test suite will also provide reassurance that the components work as expected.
* As more applications and pages within those applications start using these components, the more coupled they become to the UI components. If there ever was a severe issue with the components, all of the applications and pages that reference the bad components would need to change.
* We may want to think about putting these components into their own repository and have the client-side components reference a version of the repository, so that if a change happens to a component that is a breaking change it doesn't break the applications.

## References
* [Blazor WebAssembly Basics](https://blog.stevensanderson.com/2018/02/06/blazor-intro/)
* [bUnit](https://bunit.dev/)
