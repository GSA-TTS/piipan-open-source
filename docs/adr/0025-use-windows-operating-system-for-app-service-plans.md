# 25. Use Windows operating system for App Service plans

Date: 2022-01-11

## Status

Accepted

## Context

We use Azure App Service plans to back our Function Apps and App Service web apps. App Service plans can be configured to use Windows or Linux under the hood.

In general, our development team works in Unix-like environments and prefers Linux as a default selection.

However, Azure App Service plans that use Windows under the hood often have a larger feature set than those using Linux. For instance, Windows plans have [several more logging types](https://docs.microsoft.com/en-us/azure/app-service/troubleshoot-diagnostic-logs#overview) available compared to Linux plans.

## Decision

In the interest of establishing a default OS across all of our Azure resources and in an attempt to gain access to more useful logging/networking features, we will use Windows under the hood on all App Service plans.

## Consequences

- App Service instances using Windows plans require more manual wrangling to produce useful log streams. In contrast, when using Linux logging (while more limited) "just works."
- Developers working locally in Unix-like environments will need to vigilantly avoid using environment-specific features (e.g., [Time Zone names](https://devblogs.microsoft.com/dotnet/cross-platform-time-zones-with-net-core/)).
- Similarly, end-to-end testing increases in importance as a means to identify environment-specific issues.
