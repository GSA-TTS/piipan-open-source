# 11. Use Razor pages for web-apps

Date: 2021-01-26

## Status

Accepted

## Context

Piipan will need to build out several web applications. We anticipate that these apps will be of low-complexity and have only a modest need for client-side interactivity. 

Based on our partner's strong platform preference, [we are working in the Microsoft ecosystem, using C# and .NET](https://github.com/18F/piipan/blob/main/docs/adr/0003-programming-framework.md). For web applications, that implies the [ASP.NET Core](https://en.wikipedia.org/wiki/ASP.NET_Core) web application framework.

Currently, Microsoft promotes 2 approaches for web application construction: [Razor Pages](https://docs.microsoft.com/en-us/aspnet/core/razor-pages/?view=aspnetcore-5.0&tabs=visual-studio) and [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor).

Razor Pages is a template markup syntax and a page-focused abstraction layer. It is based on the C# programming language and replaces [ASP.NET MVC](https://en.wikipedia.org/wiki/ASP.NET_MVC).

Blazor is Microsoft's response to UI libraries and frameworks like React and Angular. In its most straight-forward usage, developers write client code in C#, compile the code to Web Assembly, deliver it to the browser, and run it as part of a Single Page Application (SPA).

## Decision

We will use ASP.NET Core with Razor Pages to implement web applications in Piipan.

## Consequences

* We continue to primarily use Microsoft ecosystem "defaults" for Piipan, hypothetically making long-term onboarding easier within our partner agency, but at the cost of near-term onboarding time for 18F members.
* By using a server-side rendering approach, we avoid a significant amount of complexity inherent with Blazor. 

## References
* [_Choosing a web application architecture_ from the TTS Engineering Practices Guide](https://engineering.18f.gov/web-architecture/)
