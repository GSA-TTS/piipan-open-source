# Extract-Transform-Load process for PII bulk import

## Prerequisites
- [Azure Command Line Interface (CLI)](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download)
- [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)

## Summary

An initial approach for the bulk import of PII records into piipan has been implemented:
1. Begin with a state-specific CSV of PII records, in our [bulk import format](bulk-import.md).
1. The CSV is uploaded to a per-state storage account, in a container named `upload`.
1. An Event Grid blob creation event triggers a function named `BulkUpload` in a per-state Function App.
1. The function extracts the CSV records, performs basic validation, and inserts into a table in a per-state database. Any error in the CSV file will abort the entire upload.

While all states have separate storage accounts, function apps, and databases, the function code is identical across each state.

## Environment variables

The following environment variables are required by `BulkUpload` and are set by the [IaC](../../docs/iac.md):

| Name | |
|---|---|
| `ParticipantsDatabaseConnectionString` | [details](../../docs/iac.md#\:\~\:text=ParticipantsDatabaseConnectionString) |
| `BlobStorageConnectionString` | [details](../../docs/iac.md#\:\~\:text=BlobStorageConnectionString) |
| `CloudName` | [details](../../docs/iac.md#\:\~\:text=CloudName) |

## Local development

To Be Determined

## Manual deployment

### Prerequisites
1. The [Piipan infrastructure](../../docs/iac.md) has been established in the Azure subscription.
1. An administrator has signed in with the Azure CLI.

### App deployment
To republish the `BulkUpload` Azure Function for one specific state:
```
func azure functionapp publish <function-app-name> --dotnet
```

## Ad-hoc testing

In a development environment, the `test-apim-upload-api.bash` tool can be used to upload test CSV files to a storage account. APIM subscription, and CSV file information can be controlled by adjusting the variables at the top of the script.  Pass the environment and state to the script. Usage:

```
./tools/test-apim-upload-api.bash tts/dev EA
```
