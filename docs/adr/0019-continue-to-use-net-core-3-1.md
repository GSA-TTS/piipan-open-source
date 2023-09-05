# 19. Continue to use .NET Core 3.1

Date: 2021-09-15

## Status

Accepted

## Context

We [considered upgrading to .NET 5 from .NET Core 3.1](0009-continue-to-use-net-core-3-1.md) in January 2021. At the time, we decided to continue with .NET Core 3.1 due to .NET 5 not being a Long Term Support version and a lack of Azure Functions support.

We revisited the decision of whether or not to upgrade to .NET 5 for two main reasons. The first is that in March 2021, Microsoft announced the general availabilty of .NET 5 in Azure Functions in their commercial cloud. Six months later, it was reasonable to think that .NET 5 may also be supported in the government cloud. Second, we expect that the eventual upgrade to .NET 6 will require fewer changes if upgrading from .NET 5 as opposed to .NET Core 3.1.

However, our research and experimentation revealed a bug in the current version of the Azure Functions Core Tools which prevents the publishing of .NET 5 Function Apps in both the commercial and government clouds.

## Decision

We will continue to use .NET Core 3.1 for all Piipan subsystems. 

## Consequences

* There will be a single .NET platform version for the 18F team to understand and manage.
* For the near- to medium-term, it will be easier to find relevant documentation and tutorials.
* There will not be a support-motivated sense of urgency to update .NET shortly after our product launch.
* We will miss out on technical advantages of .NET 5 (C# 9 language features, performance enhancements, etc.).

## Resources
* [GitHub issue in Azure Functions Core Tools](https://github.com/Azure/azure-functions-core-tools/issues/2615)
* [Fix for above issue, as yet unreleased](https://github.com/Azure/azure-functions-core-tools/pull/2616)