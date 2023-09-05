# Piipan States

*Piipan subsystem for retrieving state information*

## Sources

The `states/src` directory contains:

* [States API Library](./src/Piipan.States/Piipan.States.Api) - data models and API interfaces
* [States Core Library](./src/Piipan.States/Piipan.States.Core) - logic and data access layer
* [States API Function App](./src/Piipan.States/Piipan.States.Func.Api) - serves States data to external systems
* [States Client](./src/Piipan.States/Piipan.States.Client) - API client that you can pull into other subsystems to call the function app

## Summary

### States API Function App

The States API is a Function App made up of Azure HTTPTrigger event functions.

For now, there's a 1-1 relationship between an API endpoint and a function within this app.

## Environment variables

The following environment variables are required by both the States Functions and States API apps and are set by the [IaC](../docs/iac.md):

| Name | |
|---|---|
| `CollaborationDatabaseConnectionString` | [details](../../docs/iac.md#\:\~\:text=ParticipantsDatabaseConnectionString) |

## Local development

Forthcoming

## Testing

Each project in the `Piipan.States` subsystem has a dedicated test project in the `States/tests` directory. 

To execute tests for the subsystem as a whole:

``` bash
$ cd States
$ dotnet test
```

To execute tests for a particular subsystem project:

``` bash
$ cd States/tests/Piipan.States.<*>.Tests
$ dotnet test
```
