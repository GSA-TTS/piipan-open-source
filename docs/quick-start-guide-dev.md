# So you're an Engineer on the NAC

Hello and welcome to the NAC Engineering Team! As you may have seen, there’s a lot of documentation to sift through, so where do you start? Great question! This guide is here to help.

This guide is meant to introduce how the NAC works for Engineers. It threads together various NAC subsystems and workflows to show you how everything works as a whole at a high level. It may touch on program specifics where needed, but for deeper dives into the history and purpose of the project, see the [project overview](https://github.com/18F/piipan#overview).

This guide will show you how to perform each of the key features that are currently implemented in the NAC in a developer/playground environment.

## Prerequisites

This guide assumes that:

1. You can read and write code
1. You can navigate the Microsoft Azure portal
    - For example, you know what a "resource" and "resource group" are
1. You have access to an Azure portal, with a version of the NAC deployed on it
    - If you have access to an existing DEV environment, use that!
    - Otherwise, you may deploy onto a sandbox Azure environment that's available to you following the [Infrastructure As Code](./iac.md) steps.
1. You are [logged into the Azure CLI](https://docs.microsoft.com/en-us/cli/azure/authenticate-azure-cli#sign-in-interactively) using an account that has access to your specified Azure environment
1. You have sufficient Azure access privileges
    - There may be some kinks to work out as you go through this guide. Contact an 18F developer if access restrictions get in your way.

## The NAC at a glance

Broadly, the main tasks of the NAC are:

1. Each state uploads deidentified participant data, in bulk, to the NAC
1. SNAP Eligibility Workers query the NAC for participant matches between states
1. SNAP Eligibility Workers collaborate to resolve matches (not yet implemented)
1. FNS monitors state uploads and other metrics (partially implemented)

From an Engineering perspective, this guide will walk you through:

1. How to deidentify participant data and prepare it for upload
1. How to upload the data using the Bulk Upload API
1. How to query for a match
    - using the Duplicate Participation API
    - using the Query Tool website
1. How to view metrics
    - using the Metrics API
    - using the Dashboard website

Some particularly helpful docs to read before you start:

- [How to build, test, and deploy subsystems](https://github.com/18F/piipan#development)
- [Software subsystem standards](./adr/0018-standardize-subsystem-software-architecture.md)
- [Azure naming conventions](./azure-naming-conventions.md)

## How to prepare participant data for upload

The NAC can’t find matches without having participant data in its system.

First, states will deidentify their participant data according to the [PPRL specifications](./pprl.md). While each state is responsible for how they implement these specs, the NAC has [its own implementation of the hashing process](../shared/src/Piipan.Shared/Deidentification/LdsDeidentifier.cs) that the Query Tool uses.

The resulting file to upload will be in csv format. You can either attempt to create a csv yourself using these techniques, or use this [example csv](../etl/docs/csv/example.csv) for the upload instructions in the next section.

#### To Read

- [PPRL Specs](./pprl.md)
- [Bulk import format for participant records](../etl/docs/bulk-import.md)

#### Steps

1. Using either the example csv or a csv that you created, [validate](../etl/docs/bulk-import.md#validating-files) that the csv is in the correct format for upload.


## How to upload data

The [ETL subsystem](../etl) is responsible for anything related to uploading participant data to the NAC. The ETL documentation has instructions for uploading data in various developer-friendly ways. Here, we will show how to upload data in the same way that a state would, using the [Bulk Upload API](./openapi/generated/bulk-api/openapi.md).

#### To Read

- [Bulk Upload API documentation](../etl/docs/api.md)(for engineers)
- [Bulk Upload API documentation](./openapi/generated/bulk-api/openapi.md#bulk-api-v200) (for states)

#### Steps

1. [Choose which state](../iac/env/tts/dev/states.csv) you’d like to perform the upload for, it is required to have a states.csv file for your enviroment in `/iac/env`, for example `iac\env\tts\dev\states.csv`.
1. Ensure that an APIM subscription exists for the state you’d like to query as
    - If not, create a subscription for the Bulk Upload API for this state using [these instructions](../match/docs/duplicate-participation-api.md#managing-api-keys).
1. Perform the request. You can use [the curl example](./openapi/generated/bulk-api/openapi.md#upload-a-file) from the docs, or your own request method.
    - Example curl request:
      ```bash
      curl -X PUT 'https://tts-apim-duppartapi-dev.azure-api.net/bulk/ea/v2/upload/example.csv'\
      -H 'Content-Type: text/plain'\
      -H 'Ocp-Apim-Subscription-Key: <api key>'\
      --upload-file "example.csv"
      ```
1. View the uploaded material in the portal by going to the state-specific upload Blob Storage account.
    - For example, a storage account for state EA in the TTS DEV environment will be named ttssteauploaddev.
    - Once you’re on the resource overview page, go to the Containers tab, then click the Upload container.
    - You will need to include your IP address in the list of allowed IP addresses in order to access your uploaded file.


## How to Query for a Match

Once data from states is in the system, data can be queried for matches between states.

### Querying through the Duplicate Participation API

As the name suggests, the [Match](../match) subsystem handles all things related to matching. The NAC exposes an API to states called the Duplicate Participation API. Here, we will show how to query this API for matches.

#### To Read

- [Duplicate Participation API documentation](../match/docs/duplicate-participation-api.md) (for engineers)
- [Duplicate Participation API documentation](./openapi/generated/duplicate-participation-api/openapi.md) (for states)

#### Steps

1. Ensure that an APIM subscription exists for the state you’d like to query as
    - Select a state that’s different from the state you uploaded as
    - If it doesn’t exist, create a subscription for the Duplicate Participation API for this state using [these instructions](../match/docs/duplicate-participation-api.md#managing-api-keys).
1. Collect the state APIM subscription key to use for credentials
1. Select one or more `lds_hash` values from the csv you successfully uploaded
1. Put the hashes into the data of a request to the `find_matches` endpoint. You may use [the curl example](./openapi/generated/duplicate-participation-api/openapi.md#find-matches) from the docs, or your own request method.
    - Example curl request targeting the tts/dev environment and using credentials for the example state EA:
      ```bash
      curl --location --request POST 'https://tts-apim-duppartapi-dev.azure-api.net/match/v2/find_matches' \
      --header 'Content-Type: application/json' \
      --header 'Ocp-Apim-Subscription-Name: EA-DupPart' \
      --header 'Ocp-Apim-Subscription-Key: <api-key> \
      --data-raw '{
          "data": [
              {
                  "lds_hash": "a3cab51dd68da2ac3e5508c8b0ee514ada03b9f166f7035b4ac26d9c56aa7bf9d6271e44c0064337a01b558ff63fd282de14eead7e8d5a613898b700589bcdec"
              }
          ]
      }'
      ```
1. Perform the request. You should expect to see match results from the state you uploaded.

### Querying through the Query Tool

The Query Tool is a web app that uses the Duplicate Participation API to query for matches. States can either integrate with the API directly or use this app. The [Query-tool](../query-tool/docs/query-tool.md) subsystem manages this app. Here, we’ll show how to stand up a local version of the app and submit match queries through it.

#### To Read

- [Query Tool Application Documentation](../query-tool/docs/query-tool.md)

#### Steps

1. Follow the [local development instructions](../query-tool/docs/query-tool.md#local-development) to set up your environment
1. Once a server is running, go to the localhost homepage
1. Fill out the “Search for SNAP Participants” form using participant data that you used previously to compile your csv
1. Submit the form. You should expect to see a match for your previously uploaded state in the list of results.

## How to View Metrics

FNS federal personnel are able to view a variety of metrics through a dashboard application. The [Metrics](../metrics) subsystem manages the metrics API. The only consumer of this API is the Dashboard application, found in the [Dashboard](../dashboard/docs/dashboard.md) subsystem. These systems are not available to states.

### Viewing Metrics through the Metrics API

Unlike the Bulk Upload and Duplicate Participation API’s, this API is not managed through the public-facing API Management instance. Instead, we query the Metrics API Azure Function App directly. For authorization, the Metrics Function App requires a token which is placed in the header.

#### To Read

- [Metrics subsystems overview](../metrics)
- [Metrics API Integration Testing tool](../metrics/tools/test-metricsapi.bash)
- [Metrics API Openapi schema](../metrics/docs/openapi/metrics/index.yaml)

#### Steps

1. Collect the access token for the Metrics API function app
    - This can be done through the Azure CLI by first [retrieving the App ID](../metrics/tools/test-metricsapi.bash#L23), then using the App ID to [fetch the token](../metrics/tools/test-metricsapi.bash#L29).
1. Place this token in [the header of your request](../metrics/tools/test-metricsapi.bash#L42)
    - key: `Authorization`
    - value: `Bearer [token]`
1. Gather the url for the `/GetLastUpload` endpoint
    - In the portal, go to the metrics api function app. It’ll be named something like `tts-func-metricspi-dev`
    - Click on the **Functions** tab
    - Click on **GetLastUpload** in the list
    - Click the **Get Function URL** tab and copy the url
1. Submit a request using this url. You can reference this curl example or use your own method. You should expect to see your own uploads in the list of responses.
1. Gather the url for the `GetParticipantUploads` endpoint (using the same steps as above)
1. Append your state abbreviation as a query parameter to this url
    - Example: `?state=ea`
1. Submit a request using this url + query param. Use the same token you used for the first endpoint. You should expect to see a list of uploads from your specified state

### Viewing Metrics through the Dashboard

Like the Query Tool app, the Dashboard is a Razor Pages web app. Currently, the Dashboard app uses the Metrics API under the hood to show a list of recent uploads from each state, as well as a basic search for a single state’s uploads.

#### To Read

- [Dashboard Application docs](../dashboard/docs/dashboard.md#dashboard-application)

#### Steps

1. Follow the [local development instructions](../dashboard/docs/dashboard.md#local-development) to set up your environment
1. Once a server is running, go to the localhost homepage
1. Click the link **SNAP participant data uploads by state**
1. On the **/ParticipantUploads** page, you should see a list of most recent participant uploads by state. You should expect to see the state you uploaded to in this list.
1. Enter your state abbreviation into the search bar and click **Search**. You should expect to see a list of your state’s uploads returned.
1. You can clear your search by clicking **Clear Search**

## How these resources are managed

Broadly speaking, all of the APIs and web apps you’ve accessed in the previous steps exist as Platform-as-a-Service (Paas) or Function-as-a-Service (FaaS) resources in Azure. For example, the ETL subsystem used to upload data is essentially a [.NET application](../etl/src/Piipan.Etl/Piipan.Etl.Func.BulkUpload) running as an Azure Function.

The creation, configuration, and modification of all of these resources is done via an automated [Infrastructure-as-Code (Iac) process](./iac.md). Resources are declared as [ARM templates](./adr/0005-infrastructure-as-code-tool.md)—or, occasionally, collections of [Azure CLI commands](../iac/create-resources.bash#L437-L448)—and deployed by following the [documented steps](./iac.md#steps).

Microsoft provides comprehensive [documentation on ARM templates](https://docs.microsoft.com/en-us/azure/templates/), though it can be difficult to string together the collection of resources, properties, and values necessary to create the correct JSON document for a new piece of infrastructure from scratch. It can often be helpful to first create a resource using the Azure Portal UI, [export the resulting ARM template](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/export-template-portal#export-template-from-a-resource), and use that as a starting point.

Once the IaC process has been run, individual applications are deployed to the resources using a [standardized build.bash script](../README.md#development) that can be run manually by a developer or as part of a CI/CD pipeline.

## How to Run and Debug in Visual Studio

Either create a json file called "launchSettings.json" in the properties folder of the function you're running, or an "appsettings.json" file in the main folder of the function you're running. Launchsettings can be used for local settings when running it through visual studio and appsettings can be used when the information is going to be deployed and needs to stay with the other project files.

Variables will be pulled from the launchSettings.Json file if the developer uses **Environment.GetEnvironmentVariable** and the appsettings.json file if the developer uses **config[VARIABLE_NAME_HERE]**. When using from the appsettings.json the IConfiguration interface should be used.

An example of the environment variables are below. The following environment variables can be found:

| Variables     | Where to find them                   | 
| :------------ |:-------------------------------------:| 
| OrchApiAppId,  MatchResApiAppId, StatesApiAppId      | Azure Active Directory -> Enterprise Applications -> Remove Enterpirse Applications filter, add 'Assignment Required' = required filter -> App Id's should be with every application |
| OrchApiUri, MatchResApiUri, StatesApiUri     | 1) Go to function on portal -> overview and copy the URL  2)  Go to function on portal -> app files -> append the route prefix to the URL copied in step 1    |
| CollaborationDatabaseConnectionString, ParticipantsDatabaseConnectionString, MetricsDatabaseConnectionString | Go to your __-psql-core-__ or __-psql-participants-__ -> connection strings -> copy the .NET connection string -> Remove 'Ssl Mode=Require' and replace with the endings in the examples below      |
| BlobStorageConnectionString, UploadPayloadKey, UploadPayloadKeySHA | Go to the function you are running in the Azure Portal -> Configuration -> Copy the appropriate values from the table |
| ColumnEncryptionKey | Key Vault -> secrets -> column-encryption-key -> current version -> secret value |
| PSQL DB Passwords | Key Vault -> secrets -> appropriate PSQL DB -> current version -> secret value |

```
{
  "profiles": {
    "Piipan.Match.Func.ResolutionApi": {
      "commandName": "Project",
      "launchBrowser": true,
      "applicationUrl": "https://localhost:5001;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "OrchApiUri": "https://cc-func-orchestrator-cjc.azurewebsites.net/api/v1/",
        "OrchApiAppId": "ead134cc-606a-4a8b-a4bd-aeb727195dc6",
        "MatchResApiUri": "https://cc-func-matchres-cjc.azurewebsites.net/api/v1/",
        "MatchResApiAppId": "861b330f-bc66-44cc-8d24-0f1a6889f863",
        "StatesApiAppId": "8dd6d72f-1aa6-4d1c-bb4f-980fa76c9ddb",
        "StatesApiUri": "https://cc-func-states-cjc.azurewebsites.net/api/v1/",
        "CollaborationDatabaseConnectionString": "Server=cc-psql-core-cjc.postgres.database.azure.com;Database=collaboration;Port=5432;User Id=USER_ID;Password=CORE_DB_PASSWORD;Ssl Mode=VerifyFull;",
        "MetricsDatabaseConnectionString": "Server=cc-psql-core-cjc.postgres.database.azure.com;Database=metrics;Port=5432;User Id=USER_ID;Password=CORE_DB_PASSWORD;Ssl Mode=VerifyFull;",
        "ParticipantsDatabaseConnectionString": "Server=cc-psql-participants-cjc.postgres.database.azure.com;Database=ea;Port=5432;User Id=USER_ID;Password=PARTCIPANT_DB_PASSWORD;Ssl Mode=VerifyFull;SearchPath=piipan;",
        "ColumnEncryptionKey": "ENCRYPTION_KEY_HERE",
        "EnabledStates": "ea,eb",
        "States": "ea,eb",
        "BlobStorageConnectionString": "DefaultEndpointsProtocol=https;EndpointSuffix=core.windows.net;AccountName=ccsteauploadcjc;AccountKey=ACCOUNT_KEY;BlobEndpoint=https://ccsteauploadcjc.blob.core.windows.net/;FileEndpoint=https://ccsteauploadcjc.file.core.windows.net/;QueueEndpoint=https://ccsteauploadcjc.queue.core.windows.net/;TableEndpoint=https://ccsteauploadcjc.table.core.windows.net/",
        "UploadPayloadKey": "UPLOAD_KEY",
        "UploadPayloadKeySHA": "UPLOAD_SHA_KEY"
      }
    }
  }
}
```

**Remote Debugging**

Go to your function on Azure Portal -> Configuration -> General Settings -> Ensure Remote Debugging is set to on and your version of Visual Studio -> Deploy a NON RELEASE version of the function to Azure -> Click the 3 dots menu under hosting and attach Remote Debugger

For additional infomation of running Integration tests locally using [Visual Studio](../docs/integration-test-local.md).

## Review

Congrats on making it through all of the steps! By now, you should:

- have a general idea of how states will interact with the NAC
- know what each subsystem is responsible for
- have a general idea of where to look to find more information
- understand the broad strokes of how the infrastructure underlying the NAC is managed and how applications are deployed

## Further Reading

- [ADR Log](./adr)

