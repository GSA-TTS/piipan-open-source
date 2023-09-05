# Piipan Metrics

*Piipan subsystem for monitoring other subsystems*

## Sources

The `metrics/src` directory contains:

* [Metrics API Library](./src/Piipan.Metrics/Piipan.Metrics.Api) - data models and API interfaces
* [Metrics Core Library](./src/Piipan.Metrics/Piipan.Metrics.Core) - logic and data access layer
* [Metrics API Function App](./src/Piipan.Metrics/Piipan.Metrics.Func.Api) - serves metrics data to external systems
* [Metrics Collect Function App](./src/Piipan.Metrics/Piipan.Metrics.Func.Collect) - monitors other subsystems and stores metrics data

## Summary

### Metrics API Function App

The Metrics API is a Function App made up of Azure HTTPTrigger event functions.

For now, there's a 1-1 relationship between an API endpoint and a function within this app.

### Metrics Collect Function App

Metrics Collect is an Azure Function App made up of Azure Event Grid event functions.

For now, there's a 1-1 relationship between a specific metric needing to be captured and a function within this app.

## Environment variables

The following environment variables are required by both the Metrics Functions and Metrics API apps and are set by the [IaC](../docs/iac.md):

| Name | |
|---|---|
| `MetricsDatabaseConnectionString ` | [details](../../docs/iac.md#\:\~\:text=MetricsDatabaseConnectionString ) |
| `CloudName` | [details](../../docs/iac.md#\:\~\:text=CloudName) |
| `KeyVaultName` | [details](../../docs/iac.md#\:\~\:text=KeyVaultName) |

## Local development

Local development is currently limited as a result of using a managed identity to connect to the metrics database. The Instance Metadata Service used by managed identities to retrieve authentication tokens is not available locally. There are [potential solutions](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/service-to-service-authentication#local-development-authentication) using the `Microsoft.Azure.Services.AppAuthentication` library. None have been implemented at this time.

The app will still build and run locally. However, any valid request sent to the local endpoint will result in an exception when the app attempts to retrieve an access token. Invalid requests (e.g., malformed or missing data in the request body) will return proper error responses.

To build and run the app with this limited functionality:

1. Fetch any app settings using `func azure functionapp fetch-app-settings {app-name}`. The app name can be retrieved from the Portal.
1. Run `func start` or, if hot reloading is desired, `dotnet watch msbuild /t:RunFunctions`.

## Testing

Each project in the `Piipan.Metrics` subsystem has a dedicated test project in the `metrics/tests` directory. 

To execute tests for the subsystem as a whole:

``` bash
$ cd metrics
$ dotnet test
```

To execute tests for a particular subsystem project:

``` bash
$ cd metrics/tests/Piipan.Metrics.<*>.Tests
$ dotnet test
```
## Remote testing

To test the Metrics API remotely:
1. Follow the [instructions](../../docs/securing-internal-apis.md) to assign your Azure user account the `Metrics.Read` role for the remote metrics Function App and authorize the Azure CLI.
1. Retrieve a token for your user using the Azure CLI: `az account get-access-token --resource <metrics application ID URI>`.
1. Send a request to the remote endpoint—perhaps using a tool like Postman or `curl`—and include the access token in the Authorization header: `Authorization: Bearer {token}`.