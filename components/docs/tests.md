# Component Tests application

## Prerequisites
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/en/) >= 12.20.0 and `npm` [Node Package Manager](https://docs.npmjs.com/downloading-and-installing-node-js-and-npm) for compiling assets during build

## Local testing
The tests are written in [bUnit](https://bunit.dev/), a testing framework for Blazor components. All of the tests run within the scope of the application, and do not open a browser to run. This means that they run very fast and very consistently. In the event that Blazor makes a call to Javascript using the JSRuntime, these calls are mocked to return the expected value.

To run the test suite locally:
1. Test the components using the [dotnet test](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-test) CLI command:
```
    cd components/tests/Piipan.Components.Tests
    dotnet test
```

## Other Notes
Visual Studio Community 2022 tends to mess up formatting in the razor test files, particularly around the Render Fragments. Before checking in changes to razor test files, compare your file carefully and remove any of the bad formatting that was applied.