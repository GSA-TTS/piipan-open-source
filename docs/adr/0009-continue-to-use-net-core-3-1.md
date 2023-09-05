# 9. Continue to use .NET Core 3.1

Date: 2021-01-19

## Status

Accepted

## Context

Our initial work on Piipan began in October 2020, using .NET Core 3.1. .NET Core 3.1 is Microsoft's most recent Long Term Support (LTS) version of the .NET platform, and is [supported through December 2022](https://dotnet.microsoft.com/platform/support/policy/dotnet-core).

.NET 5 was [released in November 2020](https://devblogs.microsoft.com/dotnet/announcing-net-5-0/) – it drops the "Core" moniker and skips a major version for branding reasons – but is now, in general, [Microsoft's recommended version of the .NET platform](https://dotnet.microsoft.com/download). However, it is not an LTS release; that will come with .NET 6, which is anticipated in February 2022. [.NET 5 support ends 3 months after that](https://dotnet.microsoft.com/platform/support/policy/dotnet-core).

Further, Azure Functions does not yet support .NET 5. [A .NET 5 worker is anticipated in early 2021](https://techcommunity.microsoft.com/t5/apps-on-azure/net-5-support-on-azure-functions/ba-p/1973055).

## Decision

Continue to use .NET Core 3.1 for all Piipan sub-systems.

## Consequences

* There will be a single .NET platform version for the 18F team to understand and manage.
* For the near- to medium-term, it will be easier to find relevant documentation and tutorials.
* There will not be a support-motivated sense of urgency to update .NET shortly after our product launch.
* We will miss out on technical advantages of .NET 5 (C# 9 language features, performance enhancements, etc.).
