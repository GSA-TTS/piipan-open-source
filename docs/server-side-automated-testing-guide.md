# Server-Side Automated Testing Guide
## .NET Unit Test Basics

* We use [XUnit](https://xunit.net/) for automated testing of .NET code.
* We use [Moq](https://methodpoet.com/unit-testing-with-moq/) to mock up objects. This allows us to have fine-grained control of what we are testing.
* Unit tests should test one specific class, while mocking most of the other classes to test specific scenarios.
* There is a Test project for each project in the src folder of each area of the program.
* Each unit test project gets added to the .sln file for the given area you are testing.
* Web Applications use [BUnit](https://bunit.dev/) for testing, which is a Blazor library built on top of XUnit and is able to effectively test Blazor components.

### Startup Tests
To effectively test dependency injection throughout the application, we need to test each entry point into the system. This includes, but is not limited to:
* [Azure Function Triggers](https://learn.microsoft.com/en-us/azure/azure-functions/functions-create-your-first-function-visual-studio?tabs=in-process)
* [ASP.NET Page Models](https://learn.microsoft.com/en-us/aspnet/core/razor-pages/?view=aspnetcore-6.0&tabs=visual-studio)
* [ASP.NET Controllers](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions?view=aspnetcore-6.0)

To accomplish this, you should use the [DependencyTester class](https://github.com/18F/piipan/blob/dev/shared/tests/Piipan.Shared.Tests/DependencyInjection/DependencyTester.cs).
You can register any class that you want to attempt to instantiate based off the services injected in Startup. The test will fail if one of the registered classes has a problem instantiating.
The test will also fail if you forgot to register any Azure Functions, Page Models, or Controllers, and you should double check your code and register them appropriately.

## Integration Tests
Whereas Unit Tests are for automated testing of specific classes, Integration Tests are for mocking the larger part of the system, including the database. Here are some general tips:

* Avoid writing SQL that are to be used exclusively for testing. Attempt to use already written code (Dao objects) to do database calls whenever possible. This allows us to test more of the system.
* [Set up your local development environment](https://github.com/18F/piipan/blob/dev/docs/integration-test-local.md) for testing integration tests.
* You can use the PiipanTestServer to complete integration tests on Web Applications. This makes an actual HTTP call to an endpoint, going through the entire ASP.NET routing/middleware process, rather than simply calling controller methods.

## Code Coverage for .NET
To run code coverage locally to see what branches are not fully covered, you can follow [this guide](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-code-coverage?tabs=windows).
Specifically, these are the steps you need to implement to run code coverage for a given project:
1. (One time only) In the .NET CLI, run `dotnet tool install -g dotnet-reportgenerator-globaltool`.
1. In the .NET CLI, navigate to your test project and then run `dotnet test --collect:"XPlat Code Coverage"`
1. In the .NET CLI, run `reportgenerator -reports:"Path\To\TestProject\TestResults\{guid}\coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html`, replacing the path to the coverage file with the path output in the previous step.

## Running Tests
* You can run tests by following the documentation in the main [Readme file](https://github.com/18F/piipan#how-to-test).
* Alternatively, you can open up [Test Explorer](https://learn.microsoft.com/en-us/visualstudio/test/run-unit-tests-with-test-explorer?view=vs-2022) in Visual Studio and run tests directly from there.