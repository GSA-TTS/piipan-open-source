# 36. Use Blazor WebAssembly in a Single Page Application

Date: 2022-09-28

## Status

Proposed

Supercedes [29. Use Blazor WebAssembly for Client Applications](0029-use-blazor-web-assembly.md)

## Context

Ever since the decision was made to use Blazor WebAssembly for client applications, it has fulfilled its purpose by giving us a higher amount of client-side interactivity while having a lower development effort. The decision at the time was to continue using server-side rendered pages to reduce the amount of backend code we'd have to change as well as the uncertainties about how to handle EasyAuth with a Single Page Application (SPA). It was not feasible for MVP to go this route, but now that we have some bandwidth we should consider switching to a SPA.

The downside to the server-side rendered pages include the following:
1. Every page that we navigate to currently has to fetch all of common server variables, render the HTML code, and wire up Blazor WebAssembly. Because of this the application suffers unnecessary performance issues and could run much faster.
2. Blazor wasn't really meant to work like this and we have had to put in a couple of "hacks" to get the whole process to work. For example, every form hooks up into Javascript's onsubmit event, then when that event fires off it transitions back into Blazor to validate the form. Once the form is validated, it transitions back into Javascript to actually finish submitting the form. Another example is the Match Detail form when a server-side issue occurs in the application. The form should re-render with the typed values in them, even when they do not match the match detail's information. Because of this, we need to save both sets of values and pass them from server to client side when the server renders the page. With a SPA, we just send the values that we are trying to save to the API, and if the request succeeds we update the right side of the application. These issues as well as others we would encounter in the future make an odd developer experience that is prone to errors.
3. It will be much easier to use automated testing, because we will be able to mock API requests. Mocking an API request is much, much easier than mocking an entire HTML response, especially when said response is not requested with Javascript. Using Cypress/BUnit in this way will make our tests a lot more thorough.
4. We ran into an issue on the Dashboard where we did not know the user's timezone, because the page was rendered server side. If it was a SPA, we would know their timezone and could pass that information down to the API.

## Decision

After putting together a [Proof of Concept](https://github.com/18F/piipan/pull/3922), it is clear that we should be able to convert to a SPA relatively quickly, and without running into issues with EasyAuth.

The following changes are needed for this architecture update:

1. Switching to a controller-based API system, rather than delivering an entire page on every server request.
2. Fetching all of the data shared between views up front the first time the page loads, and client-side caching the results. Things like your user role, user location, email, what permissions you need to view certain pages, etc. This is done in the ServerAppData's Initialize function, which gets called from _Host.cshtml. _Host.cshtml is downloaded only once when the user first goes to the site.
3. A new Piipan.Shared.Client project, which contains classes and objects that are meant to be used by both web applications.
A new client-side PiipanApiService deals with calling the APIs. This ApiService is meant to be extended in each Web Application's client project to perform specific API requests.
4. The PiipanApiService uses a special HttpMessageHandler (PiipanHttpMessageHandler), which sets cookies and handles the responses. It must also handle what happens when our authentication token times out. While some additional testing needs to be done with the actual production workflow, the response will not come back as JSON (it seems like it comes back as HTML that is supposed to run Javascript that re-directs the page). The current though is that if this response happens, we will simply refresh the page, causing the entire server workflow to take place which will get us the new tokens.
5. UsaForms no longer hook up to the Javascript "onsubmit" event, and instead you need to call the corresponding ApiService.
6. Other minor changes about how to handle figuring out who the referring page is since it's all client side, updating the navigation manager to not force a full refresh when continuing on from a vulnerable individual modal, and other miscellaneous updates to get the proof of concept to work.


## Consequences

* Dashboard and QueryTool Applications should be converted to a SPA. This will immediately improve the performance of the apps.
* We will need to continue to make a concious effort not to push any secrets to the client side. For the Query Tool WebAssembly project, that means only incorporating the component library and possibly a shared client library, but none of the server side libraries. Developers should be aware of Blazor basics and treat C# code that is referenced by the project as if it was fully visible to the client, as if it was a Javascript file. A compiled DLL can easily be de-compiled so that the file can be viewed in plain text.
* After the conversion to a SPA, additional testing should be done to make sure none of the existing functionality is broken in any way.
* After the conversion to a SPA, we should achieve the following benefits:
   1. Increased application performance for end users.
   2. Removing error-prone application architecture by using Blazor WASM in it's intended way, leading to a more stable codebase.
   3. Automation testing improvements by being able to test a wider range of scenarios.

## References
* [Blazor WebAssembly Basics](https://blog.stevensanderson.com/2018/02/06/blazor-intro/)
* [WebAssembly from Server Side pages](https://docs.microsoft.com/en-us/aspnet/core/mvc/views/tag-helpers/built-in/component-tag-helper?view=aspnetcore-6.0)
* [WebAssembly Browser Support](https://caniuse.com/wasm)
* [Single Page Application vs Multi Page Application](https://medium.com/@NeotericEU/single-page-application-vs-multiple-page-application-2591588efe58)
