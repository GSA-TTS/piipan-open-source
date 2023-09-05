# Infrastructure-as-Code

## Prerequisites

- [Azure Command Line Interface (CLI)](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) >= 2.37.0
- [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local) >= 4.x
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download)
- `bash` shell >= 4.1, `/dev/urandom` – included in macOS, Linux, Git for Windows, Azure Cloud Shell
- `psql` client for PostgreSQL – included in Azure Cloud Shell
- [Node.js](https://nodejs.org/en/) >= 12.20.0 and `npm` [Node Package Manager](https://docs.npmjs.com/downloading-and-installing-node-js-and-npm) for compiling assets during the build of ASP.NET Core web applications. NOTE: As of 4/26/22, it is recommended to use Node.js version 14.19.1, otherwise the package-lock files you generate may not be compatible with the rest of the team's setup.
- [coreutils/sha256sum](https://formulae.brew.sh/formula/coreutils)  >= 9.1, – sha256sum is included in Linux, Git for Windows, Azure Cloud Shell
- [Liquibase](https://docs.liquibase.com) >= 4.16
    - [Install](https://docs.liquibase.com/start/install/home.html)
    - [Configuration](https://github.com/18F/piipan/blob/dev/docs/database-migrations.md#running-liquibase-via-command-line)

## Steps
To (re)create the Azure resources that `piipan` uses:

NOTE: If you are using docker you can skip step 1 to 5 running the following command:
```
    docker-compose up --build
```

1. Run `install-extensions` to install Azure CLI extensions required by the `az` commands used by our IaC:
```
    ./iac/install-extensions.bash
```
2. Connect to a trusted network. Currently, only the GSA network block is trusted. [//]: # (Does this need to be changed in the code?)
3. Configure the desired Azure cloud:
```
    az cloud set --name AzureCloud
```
4. Sign in with the Azure CLI `login` command. An account with at least the Contributor role on the subscription is required.
```
    az login
```

5. If deploying to TEST environment, update states.csv to enable all tenants. It is required to have a states.csv file configured for your enviroment under `iac/env` for example `iac/env/sub/dev/states.csv`. Edit the states.csv file so that each tenant has the correct ENABLED/DISABLED field based on desired match responses
[//]: # (Should we change the `states.csv` file name?)

6. Run `create-resources`, which deploys Azure Resource Manager (ARM) templates and runs associated scripts, specifying the [name of the deployment environment](#deployment-environments). Once this step is completed, all the infrastructure components will be provisioned.
```
    cd iac
    ./create-resources.bash sub/dev
```

7. Configuring user account authentication for piipan's Query Tool and Dashboard web apps requires additional actions. These actions can be deferred, but until completed, users will not be able to access these web apps.

    - Securely obtain the OpenID Connect (OIDC) client secrets for the identity provider (IdP) configured in the `iac/env` file for your environment.
    - Run `store-oidc-secrets` and follow the prompts to store the client secrets in the system's core key vault.
    - Run `configure-oidc-apps`, to configure the Query Tool and Dashboard App Service instances with the client secrets.
```
    ./store-oidc-secrets.bash sub/dev
    ./configure-oidc-apps.bash sub/dev
```

8. Create a subscription in the API Management service. For now, the API Management subscriptions are created manually and not by the IaC. For example, you’ll need to create `EB-DupPart` and `EA-BulkUpload` before you can use the `test-apim-upload-api.bash` and `test-apim-match-api.bash` test scripts.

```
    ./match/tools/create-apim-match-subscription.bash sub/dev eb
    ./etl/tools/create-apim-bulk-subscription.bash sub/dev ea
```

9. Now you have to assign the necessary “application role” for the API. [Detailed documentation is found here](https://github.com/18F/piipan/blob/dev/docs/securing-internal-apis.md#working-locally), but if you just want to test your environment you can run the following steps.

    Use assign-app-role to assign your user account the necessary application role:

    ```
    #Template
    ./tools/assign-app-role.bash <azure-env> <function-app-name> <app-role-name>

    #Examples
    ./tools/assign-app-role.bash tts/dev tts-func-metricsapi-dev Metrics.Read
    ./tools/assign-app-role.bash tts/dev tts-func-orchestrator-dev OrchestratorApi.Query
    ./tools/assign-app-role.bash tts/dev tts-func-matchres-dev MatchResolution.Query
    ./tools/assign-app-role.bash tts/dev tts-func-states-dev States.Query
    ./tools/assign-app-role.bash tts/dev tts-func-eaetl-dev Etl.Query     (repeat for all enabled tenants.)
    ./tools/assign-app-role.bash tts/dev tts-func-notify-dev Notify.Read 
    ./tools/assign-app-role.bash tts/dev tts-func-metricscol-dev MetricsCol.Read

    ```

    Use authorize-cli to add the Azure CLI and Visual Studio as authorized client applications for the Function's application registration:
    ```
    #Template
    ./tools/authorize-cli.bash <azure-env> <function-app-name>

    #Examples
    ./tools/authorize-cli.bash tts/dev tts-func-metricsapi-dev
    ./tools/authorize-cli.bash tts/dev tts-func-orchestrator-dev
    ./tools/authorize-cli.bash tts/dev tts-func-matchres-dev
    ./tools/authorize-cli.bash tts/dev tts-func-states-dev
    ./tools/authorize-cli.bash tts/dev tts-func-eaetl-dev   (repeat for all enabled tenants.)
    ./tools/authorize-cli.bash tts/dev tts-func-notify-dev 
    ./tools/authorize-cli.bash tts/dev tts-func-metricscol-dev 

    ```

10. Time to test your infrastructure
    ```
    #Test ETL
    ./etl/tools/test-apim-upload-api.bash tts/dev ea
    ./etl/tools/test-apim-upload-api.bash tts/dev eb
    ./etl/tools/test-apim-get-upload-by-id.bash tts/dev ea e.g. 0x8DA972FB2DA60E9
    #Test Match
    ./match/tools/test-apim-match-api.bash tts/dev
    ./match/tools/test-match-res.bash tts/dev e.g. 7CTD2FG

    #Test Metrics
    ./metrics/tools/test-metricsapi.bash tts/dev
    ```


## Deployment environments

Configuration for each environment is in `iac/env` in a corresponding, `source`-able bash script.

| Name | Description |
|---|---|
| `sub/dev`  | Azure commercial cloud subscription, updated continuously within a sprint |
| `sub/test` | Azure commercial cloud subscription, updated at the end of each sprint |


## Environment variables

#### Automatically configured
The following environment variables are pre-configured by the Infrastructure-as-Code for Functions or Apps that require them. Most often they are used to [bind backing services to application code](https://12factor.net/backing-services) via connection strings.

| Name | Value | Used by |
|---|---|---|
| `ParticipantsDatabaseConnectionString` | ADO.NET-formatted database connection string. If `Password` has the value `{password}`; i.e., `password` in curly quotes, then it is a partial connection string indicating the use of managed identities. An access token must be retrieved at run-time (e.g., via [AzureServiceTokenProvider](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/service-to-service-authentication)) to build the full connection string.  | Piipan.Etl, Piipan.Match.Orchestrator |
| `CollaborationDatabaseConnectionString` | ADO.NET-formatted database connection string. If `Password` has the value `{password}`; i.e., `password` in curly quotes, then it is a partial connection string indicating the use of managed identities. An access token must be retrieved at run-time (e.g., via [AzureServiceTokenProvider](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/service-to-service-authentication)) to build the full connection string.  | Piipan.Match.Orchestrator, Piipan.Match.Func.ResolutionApi, Piipan.States.Func.Api |
| `MetricsDatabaseConnectionString` | ADO.NET-formatted database connection string. If `Password` has the value `{password}`; i.e., `password` in curly quotes, then it is a partial connection string indicating the use of managed identities. An access token must be retrieved at run-time (e.g., via [AzureServiceTokenProvider](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/service-to-service-authentication)) to build the full connection string.  | Piipan.Metrics.Func.Collect, Piipan.Metrics.Func.Api |
| `BlobStorageConnectionString` | Azure Storage Account connection string for accessing blobs. | Piipan.Etl |
| `OrchApiUri` | URI for the Orchestrator API endpoint. | Piipan.QueryTool |
| `OrchApiAppId` | [Application ID](./securing-internal-apis.md#application-id-uri) for Orchestrator API's Azure Active Directory application object | Piipan.QueryTool |
| `MatchResApiUri` | URI for the Match Resolution API endpoint. | Piipan.QueryTool |
| `MatchResAppId` | [Application ID](./securing-internal-apis.md#application-id-uri) for Match Resolution API's Azure Active Directory application object | Piipan.QueryTool |
| `States` | Comma-separated list of the lower-case two letter abbreviations for each participating tenant. | Piipan.Match.Orchestrator |
| `EnabledStates` | Comma-separated list of the lower-case two letter abbreviations for each participating tenant that should return matches from the Match API. | Piipan.Match.Orchestrator,Piipan.Match.Func.ResolutionApi |
| `MetricsApiUri` | URI for the Metrics API endpoint. | Piipan.Dashboard |
| `MetricsApiAppId` | [Application ID](./securing-internal-apis.md#application-id-uri) for Metrics API's Azure Active Directory application object | Piipan.Dashboard |
| `KeyVaultName` | Name of key vault resource needed to acquire a secret | Piipan.Metrics.Func.Api, Piipan.Metrics.Func.Collect |
| `CloudName` | Name of the active Azure cloud environment | Piipan.Etl, Piipan.Match.Orchestrator, Piipan.Metrics.Func.Api, Piipan.Metrics.Func.Collect |
| `QueryToolUrl` | URL for the associated Query Tool. | Piipan.Match.Orchestrator, Piipan.Match.Func.ResolutionApi |
| `ColumnEncryptionKey` | Encryption Key for encrypting/decrypting column values. | Piipan.Etl, Piipan.Match.Orchestrator, Piipan.States.Func.Api, Piipan.Match.Func.ResolutionApi |
| `EventGridEndpoint` | Connection string for Metrics EventGrid endpoint. | Piipan.Etl, Piipan.Metrics.Func.Collect |
| `EventGridKeyString` | Key for Metrics EventGrid endpoint.  | Piipan.Etl, Piipan.Metrics.Func.Collect |
| `UploadPayloadKey` | Payload Encryption Key. | Piipan.Etl |
| `UploadPayloadKeySHA` | Payload Encryption Key SHA. | Piipan.Etl |
| `StatesApiUri` | URI for the States API endpoint. | Piipan.QueryTool, Piipan.Dashboard |
| `StatesApiAppId` | [Application ID](./securing-internal-apis.md#application-id-uri) for State API's Azure Active Directory application object | Piipan.QueryTool, Piipan.Dashboard |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Connection string to the associated App Insights resource. | Piipan.Dashboard, Piipan.QueryTool, Piipan.Etl, Piipan.Match.Orchestrator, Piipan.Metrics.Func.Collect, Piipan.Metrics.Func.Api, Piipan.Match.Func.ResolutionApi, Piipan.States, Piipan.Notifications |
| `EventGridNotifyEndPoint` | Connection string for Notify EventGrid endpoint. | Piipan.Match.Orchestrator, Piipan.Match.Func.ResolutionApi |
| `EventGridNotifyKeyString` | Key for the Notify EventGrid endpoint. | Piipan.Match.Orchestrator, Piipan.Match.Func.ResolutionApi |
| `SmtpServer` | The SMTP Server address to use when sending Notifications. | Piipan.Notifications.Func.Api |
| `SmtpReplyToEmail` | Email to reply to if you have any questions on Piipan Email Notifications | Piipan.Match.Orchestrator, Piipan.Match.Func.ResolutionApi |

## `SysType` resource tag

 The below resource tagging scheme is used for key Piipan components, using the `SysType` ("System Type") tag. This tag is used to ease enumeration of resource instances in IaC and to make a resource's system-level purpose more obvious in the Azure Portal. While a resource's name can make obvious its system type, often Azure naming restrictions and cloud-level uniqueness requirements can make those names inscrutable.

| Value | Description |
|---|---|
| PerStateEtl | one of _N_ function apps for per-state bulk ETL process |
| PerStateStorage | one of _N_ storage accounts for per-state bulk PII uploads |
| OrchestratorApi | the single Function App for the Orchestrator API |
| DashboardApp | the single Dashboard App Service |
| QueryApp | the single Query tool App Service |
| DupPartApi | the single API Management instance for the external-facing matching API |
| MatchResApi | API for Match Resolution |

In the Azure Portal, tags can be added to resource lists using the "Manage view" and/or "Edit columns" menu item that appears at the top left of the view. Specific tag values can also be filtered via "Add filter".

In the Azure CLI, `az resource list` can be used. Be sure to query for only the resources in the environment-specific resource group (e.g., `-dev`, `-test`, etc.):
```
az resource list  --tag SysType=PerStateMatchApi --query "[? resourceGroup == 'rg-match-dev' ].name"
```

## Notes
- `iac/env/{enviroment}/states.csv` (for example, `iac/env/sub/dev/states.csv`) contains the comma-delimited records of participating tenants. The first field is the [two-letter postal abbreviation](https://pe.usps.com/text/pub28/28apb.htm); the second field is the name of the tenant. The third field notes whether the tenant is Enabled or Disabled for the Match API. If they are disabled, a match will still be created, but will not be returned to the user.
- For development, dummy tenants are used (e.g., the tenant of `Echo Alpha`, with an abbreviation of `EA`).
- If you forget to connect to a trusted network and `create-resources` fails, connect to the network, then re-run the script.
- If you have recently deleted all the Piipan resource groups and are re-creating the infrastructure from scratch, you will need to explicitly purge resource types that are initially soft-deleted. Be sure to **perform these commands against the correct subscription/resource groups** as they will cause **irreversible data loss**.
  - Key Vaults
    1. `az keyvault list-deleted` for the names of soft-deleted vaults and identify the ones corresponding to your environment's resource groups.
    1. `az keyvault recover --name <vault-name>` for each relevant vault. Vaults can only be hard deleted after 90 days, due to purge protection. Vaults must be recovered and then altered as needed.
    1. If the vault was provisioned with `enablePurgeProtection=false` it can be purged by performing `az keyvault purge --name <vault-name>` for each relevant vault.
    1. Now when you re-run `create-resources`, you should not get the `Exist soft deleted vault with the same name` error.
  - API Management (APIM) instances
    1. `az rest --method GET --uri https://management.azure.com/subscriptions/<subscription-id>/providers/Microsoft.ApiManagement/deletedservices?api-version=2020-06-01-preview` for the names of soft-deleted APIM instances and identity the ones corresponding to your environment's resource groups. You'll need to use `management.usgovcloudapi.net` in AzureUSGovernment. [//]: # (How should we updae this?)
    1. `az rest --method DELETE --uri https://management.azure.com/subscriptions/<subscription-id>/providers/Microsoft.ApiManagement/locations/<region>/deletedservices/<apim-service-id>?api-version=2020-06-01-preview` for each relevant APIM instance.
    1. Now when you re-run `create-resources`, you should not get an `DeploymentFailed` error in `create-apim`.
- Some Azure CLI provisioning commands will return before all of their behind-the-scenes operations complete in the Azure environment. Very occasionally, subsequent provisioning commands in `create-resources` will fail as it won't be able to locate services it expects to be present; e.g., `Can't find app with name` when publishing a Function to a Function App. As a workaround, re-run the script.
- If you have deleted any Function App and need to re-create, then re-run the script or run the scripts related to Alter Role which binds the Function App to database.
- .NET 5 with Azure Functions v3 is [not (yet) supported by Microsoft](https://github.com/Azure/azure-functions-host/issues/6674).
- `iac/.azure` contains local Azure CLI configuration that is used by `create-resources`
- In order for IaC to automatically configure the OIDC client secrets for the Dashboard and Query Tool applications, the secrets need to be present in a key vault with a particular naming format. See `configure-oidc.bash` for details.
- The iac docker container in the `iac` folder has all pre-requisite for running the IAC code, with tested software versions. If you only need to deploy the infrastructure, you can run the docker container to skip installing pre-requisite software.
