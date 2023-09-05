# Match Resolution API


## Prerequisites
- [Azure Command Line Interface (CLI)](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download)
- [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)


## Summary

An initial API for resolving matches between states. This API is currently only used by the Query and Collaboration Tool, but is built with an eye toward future state integration.

The Match Resolution API is implemented in the `Piipan.Match.Func.ResolutionApi` project and deployed to an Azure Function App.

OpenApi schema is defined [here](./openapi/resolution/index.yaml).


## Environment variables

The following environment variables are required by the API and are set by the [IaC](../../docs/iac.md):

| Name | |
|---|---|
| `CollaborationDatabaseConnectionString` | [details](../../docs/iac.md#\:\~\:text=CollaborationDatabaseConnectionString) — Additionally, `Database` is set to the placeholder value `{database}`. The relevant per-state database name is inserted at run-time as needed. |
| `States` | [details](../../docs/iac.md#\:\~\:text=States) |
| `EnabledStates` | [details](../../docs/iac.md#\:\~\:text=EnabledStates) |


## Local development
To run the app locally:
1. Install the .NET Core SDK development certificate so the app will load over HTTPS ([details](https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-6.0&tabs=visual-studio#trust-the-aspnet-core-https-development-certificate-on-windows-and-macos)):
```
    dotnet dev-certs https --trust
```

2. If using a remote match resolution URI, follow the [instructions](../../docs/securing-internal-apis.md) to assign your Azure user account the `MatchResolution.Query` role for the remote match resolution Function App and authorize the Azure CLI.

3. Run the app using the `dotnet run` CLI command:
```
    cd query-tool/src/Piipan.Match
    dotnet run
```
Alternatively, use the `watch` command to update the app upon file changes:
```
    cd query-tool/src/Piipan.Match
    dotnet watch run
```

4. Visit https://localhost:5001


## Unit / integration tests

Unit tests for API are included in the match SLN, so they are included in our [unit test build scripts](../../README.md#unit-tests).

To run Unit tests in isolation, from root:
```bash
$ cd match/tests/Piipan.Match.Func.ResolutionApi.Tests/
$ dotnet test
```

Integration tests are run by connecting to a PostgreSQL Docker container. With Docker installed on your machine, run the integration tests using Docker Compose:
```bash
$ cd match/tests/
$ docker-compose run --rm app dotnet test /code/match/tests/Piipan.Match.Func.ResolutionApi.IntegrationTests/Piipan.Match.Func.ResolutionApi.IntegrationTests.csproj
```
For additional infomation of running Integration tests locally using [Visual Studio](../../docs/integration-test-local.md).

## App deployment

Deploy the app using the Functions Core Tools, making sure to pass the `--dotnet` flag:

```
func azure functionapp publish <app_name> --dotnet
```

`<app_name>` is the name of the Azure Function App resource created by the IaC process.

