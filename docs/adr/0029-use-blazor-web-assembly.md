# 29. Use Blazor WebAssembly For Client Applications

Date: 2022-04-05

## Status

Proposed

Supercedes [11. Use Razor pages for web-apps](0011-use-razor-pages-for-web-apps.md)

## Context

While we have been able to have a pretty low complexity application until now, new screens and functionality are being designed that require a little more client-side interactivity. Current examples include giving errors immediately after tabbing out of fields, creating a Social Security Number field that has build-in masking, and showing an error box when submitting a form that shows a list of errors currently on the page. When an error in that box is clicked, it takes you to that field on the page. All of this should happen immediately without any round-trip time from the server. More examples will likely come as we delve into more pages on the site.

## Decision

Continuing to use server-side rendered pages from the Query Tool and Dashboard are still possible using Blazor components. After testing both Blazor Server and Blazor WebAssembly, WebAssembly is better for our use case for a few reasons:
1. We will not have to have an open SignalR connection to the server, which will alleviate some strain on our server and eliminate the need for considering architectural changes, such as Azure SignalR.
2. User interactions will be immediate and not have to go over the wire and back. An example of this was the SSN field, where if we used Blazor Server the checking that occurred after every character input was slow and not responsive when going over the wire. With Blazor WebAssembly it was instantaneous.
3. There were some errors even getting the web sockets working with Blazor Server. This problem could probably have been fixed, but due to the first two items it was decided that it wasn't worth looking into it.

Our hosting model is not the "normal" hosting model, because normally a WebAssembly application is a Single Page Application that only queries server APIs. With our server-side rendered approach, we maintain all of the benefits of EasyAuth and Session timeouts without having to worry about monitoring that on the client side.

Here's how it works in a client's browser: Once the server-side rendered page reaches the client, the visible part of the page will be everything that is not a Blazor component. After this response is received, the WebAssembly runtime and client-side DLLs are then downloaded if they weren't already. Once all needed DLLs and the WebAssembly runtime is downloaded, the Blazor WebAssembly framework spins up whatever part of our page we noted as a Blazor Component. In the example of the QueryTool, this is the search form. These DLLs and WebAssembly framework (.wasm file) is downloaded the first time, it is cached by the browser. The process of downloading the DLLs and having the WebAssembly runtime interpret them is called Interpreted Mode. If there are any issues with Interpreted mode we can easily switch to [Ahead-of-time compilation](https://docs.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/webassembly?view=aspnetcore-6.0)

Each UI Project should be broken up into two parts, the main application and the client components. For example, the naming of these projects for the QueryTool should be Piipan.QueryTool for everything not Blazor, and Piipan.QueryTool.Client for the Blazor Components. The Piipan.QueryTool will have the server-side rendered pages in it, and it will reference components from the Client project. The Piipan.QueryTool project should also contain tests in a seperate project, and also Cypress/Pa11y tests.

## Consequences

* Projects that incorporate the Blazor Component Library must either be full WebAssembly projects using .Net 3.1, or server-side rendered using .Net 6. Since our current projects are server-side rendered, they will need to upgrade to .Net 6.
* We will need to make a concious effort not to push any secrets to the client side. For the Query Tool WebAssembly project, that means only incorporating the component library and possibly a shared client library, but none of the server side libraries. Developers should be aware of Blazor basics and treat C# code that is referenced by the project as if it was fully visible to the client, as if it was a Javascript file. A compiled DLL can easily be de-compiled so that the file can be viewed in plain text.
* Users would need a browser that supports WebAssembly, but only Internet Explorer could be an issue. After discussion with the team, it was determined that Internet Explorer would not be an acceptable browser and the user should upgrade.

## References
* [Blazor WebAssembly Basics](https://blog.stevensanderson.com/2018/02/06/blazor-intro/)
* [WebAssembly from Server Side pages](https://docs.microsoft.com/en-us/aspnet/core/mvc/views/tag-helpers/built-in/component-tag-helper?view=aspnetcore-6.0)
* [WebAssembly Browser Support](https://caniuse.com/wasm)
