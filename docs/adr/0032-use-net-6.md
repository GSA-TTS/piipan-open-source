# 32. Upgrade to .NET 6.0 and Azure function runtime 4.0

Date: 2022-04-11

## Status
 
Accepted

Supercedes [19. Continue to use .NET Core 3.1](0019-continue-to-use-net-core-3-1.md)
Supercedes [9. Continue to use .NET Core 3.1](0009-continue-to-use-net-core-3-1.md)
 
## Context

We [considered upgrading to .NET 5 from .NET Core 3.1](./0019-continue-to-use-net-core-3-1.md) in September 2021. At the time, we decided to continue with .NET Core 3.1 due to .NET 5 not being a Long Term Support version and also, lack of Azure Functions support.

.NET 6  is an LTS release and was [released in November 2021](https://devblogs.microsoft.com/dotnet/announcing-net-6/). It's supported for 3 years, till November, 2024.  .NET 6 delivers the final parts of the .NET unification plan that started with .NET 5. .NET 6 unifies the SDK, base libraries, and runtime across mobile, desktop, IoT, and cloud apps. In addition to this unification, the .NET 6 ecosystem offers: Simplified development, Better performance and Ultimate productivity. Cloud diagnostics have been improved with dotnet monitor and OpenTelemetry. WebAssembly support is more capable and performant. New APIs have been added, for HTTP/3, processing JSON, mathematics, and directly manipulating memory. 

Further, [Azure Functions now support .NET 6](https://techcommunity.microsoft.com/t5/apps-on-azure-blog/azure-functions-4-0-and-net-6-support-are-now-generally/ba-p/2933245). For .NET 6, Azure Functions 4.0 supports both in-process and isolated (out-of-process) execution models. Microsoft now [recommends](https://docs.microsoft.com/en-us/azure/azure-functions/functions-versions?tabs=in-process%2Cv4&pivots=programming-language-csharp) the 4.x runtime for functions in all languages.

We revisited the decision of whether to upgrade or not to .NET 6 for two main reasons. The first is that in November 2021, Microsoft launched the LTS of .NET 6 with Azure Functions 4.0  which supports both commercial and Government cloud, .NET Core 3.1 is nearing its end of support, which is December 2022.

Our research and experimentation revealed a smooth transition for upgrading to .NET 6 with Azure Functions Core Tools and publishing Function Apps 4.0 in both the commercial and government clouds.

## Decision

We will upgrade to use .NET 6.0 for all Piipan subsystems. 

We will evaluate switching to new features provided by .NET 6 and Azure Functions 4.x—for example, switching to isolated functions or migrating from Json.NET to System.Text.Json—in future ADRs.

## Consequences

* For the near-to long-term, it will be easier to find relevant documentation and tutorials.
* Developers need to upgrade to all new Prerequisites.





## Resources
* [Announcing .NET 6 Preview 6](https://devblogs.microsoft.com/dotnet/announcing-net-6-preview-6/)
