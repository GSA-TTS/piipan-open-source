# 37. Change Azure Functions to Isloated Process

Date: 2022-09-29

## Status
 
Accepted

Continuated [32. Upgrade to .NET 6.0 and Azure function runtime 4.0](0032-use-net-6.md)
 
 
## Context

Previously Azure Functions has only supported a tightly integrated mode(in-process) for .NET functions, which run as a class library in the same process as the host. This mode provides deep integration between the host process and the functions. Also, middleware is not supported in in-process.

Considering to change it to isolated-process, as the application runs on .Net 6 and also need to have support for middleware going forward. Both isolated process and in-process C# class library functions run on .NET 6.0.

Following advantages when running as isolated-process:
* Fewer conflicts: because the functions run in a separate process, assemblies used in your app won't conflict with different version of the same assemblies used by the host process.
* Full control of the process: you control the start-up of the app and can control the configurations used and the middleware started.
* Dependency injection: because you have full control of the process, you can use current .NET behaviors for dependency injection and incorporating middleware into your function app.

 .NET isolated also supports middleware registration, again by using a model similar to what exists in ASP.NET. This model gives you the ability to inject logic into the invocation pipeline, and before and after functions execute. In .NET isolated, you can write to logs by using an ILogger instance obtained from a FunctionContext object passed to your function. 
 
## Decision

We will change to use .NET 6.0  isolated-process for Azure functions.  This would provide the support for middleware.

## Consequences

* Next step we plan to implement logging middleware and exception handling middleware.  This would reduce the code duplication, consistent logging and reduced effort for new functions.


## Resources
* [Azure Functions in an isolated process](https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide)
