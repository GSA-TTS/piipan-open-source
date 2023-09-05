# Query Tool application

## Prerequisites
- [.NET Core SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/en/) >= 12.20.0 and `npm` [Node Package Manager](https://docs.npmjs.com/downloading-and-installing-node-js-and-npm) for compiling assets during build

## Local development
To run the app locally:
1. Install the .NET Core SDK development certificate so the app will load over HTTPS ([details](https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-6.0&tabs=visual-studio#trust-the-aspnet-core-https-development-certificate-on-windows-and-macos)):
```
    dotnet dev-certs https --trust
```

2. Set the `OrchApiUri` environment variable to a local or remote instance of `Piipan.Match.Orchestrator`:
```
export OrchApiUri=https://tts-func-orchestrator-dev.azurewebsites.net/api/v1/
```

3. Set the `OrchApiAppId` environment variable to the [application ID](../../docs/securing-internal-apis.md#application-id-uri) associated with the API in the previous step:
```
export OrchApiAppId=<application-id>
```

4. Set the `MatchResApiUri` environment variable to a local or remote instance of `Piipan.Match.Func.ResolutionApi`:
```
export MatchResApiUri=https://tts-func-matchres-dev.azurewebsites.net/api/v1/
```

5. Set the `MatchResApiAppId` environment variable to the [application ID](../../docs/securing-internal-apis.md#application-id-uri) associated with the API in the previous step:
```
export MatchResApiAppId=<application-id>
```

6. Set the `StatesApiUri` environment variable to a local or remote instance of `Piipan.States`:
```
export StatesApiUri=https://tts-func-states-dev.azurewebsites.net/api/v1/
```

7. Set the `StatesApiAppId` environment variable to the [application ID](../../docs/securing-internal-apis.md#application-id-uri) associated with the API in the previous step:
```
export StatesApiAppId=<application-id>
```

8. If using a remote orchestrator URI, follow the [instructions](../../docs/securing-internal-apis.md) to assign your Azure user account the `OrchestratorApi.Query` role for the remote orchestrator Function App and authorize the Azure CLI.

9. Run the app using the `dotnet run` CLI command:
```
    cd query-tool/src/Piipan.QueryTool
    dotnet run
```
Alternatively, use the `watch` command to update the app upon file changes:
```
    cd query-tool/src/Piipan.QueryTool
    dotnet watch run
```

10. Visit https://localhost:5001


### Building Assets

The app's UI uses [USWDS](https://designsystem.digital.gov/), which necessitates a dependency on NPM. If you make changes to the app's SCSS, you'll need to install USWDS (and related dependencies), by running:
```
    cd query-tool/src/Piipan.QueryTool
    npm install
```

After installing the dependencies but before making any changes to the SCSS (in `query-tool/src/Piipan.QueryTool/wwwroot/sass/`), run:
```
    npx gulp watch
```

Gulp will then watch for changes to the SCSS and compile them into the main CSS file.

[Instructions for updating Node dependencies](../../docs/node.md)

## Authorization

The app will restrict anyone with an appropriate role and location claim from accessing the app. The claims needed depend on the appsettings.json file in the src/Piipan.QueryTool directory. For local development, two role claims are needed... one whose value starts with "Location-", and one whose value starts with "Role-". For the purposes of this section, we will refer to those claims as the Location and Role claim respectively. If you are missing one of these claims you will get a Not Authorized page.

For local development, change your Role and Location claims by editing the mock_user.json file in the src/Piipan.QueryTool directory.

Below are the permissions currently needed to access different areas of the application:
- "Search for SNAP Participants" is only accessible if you have a 2 character Location claim, for example "EA". An example value in the mock_user.json file is "Location-EA"
- "Find a Match Record" is accessible to all roles and locations, however when searching for a match, if your Location claim does not map to a state that has access to the match, no match will be returned. A National location is also accepted. The state/location mapping is found in the state_info table in the Collaboration database.
- "Match Detail" has more permissions built around it. To be able to view the match detail screen, a few things need to line up:
   1. You must have a Location claim that maps to a state that has access to the match, or a National location. Local development examples: Location-EA if EA is involved in the match, Location-National can see all matches, etc.
   1. You must have a Role claim that is one of the accepted ViewMatch roles specified in appsettings.json. For local development, that is "Worker" or "Oversight".
- To be able to edit details for a match on the "Match Detail" screen, you must have a Role claim that is one of the accepted "EditMatch" roles specified in appsettings.json. For local development, that is "Worker". If you have view access but not edit access, you will see a read-only version of the match.
- "List of Piipan Matches" is accessible only if your Location claim matches the NationalOfficeValue in appsettings.json. If you have this Location, you will see a list of matches, but if you do not you will get an Unauthorized banner.

To add role based authentication to certain pages in the future, simply add a new item in the Roles array in appsettings.json, and check for that role in your page. There may be a way to add a role or location attribute to pages to make the authentication logic more streamlined, but research on that has not been done yet. Research on this will be done after the work done to change into a SPA is completed.

## Testing

Tests will be run on the continuous integration server, but
to manually run tests locally use the `dotnet test` command:
```
    cd query-tool/tests/Piipan.QueryTool.Tests
    dotnet test
```

## Deployment

The app is deployed from [CircleCI](https://app.circleci.com/pipelines/github/18F/piipan) upon updates to `main`.

Upon deployment, the app can be viewed via a trusted network at `https://<app_name>.azurewebsites.net/`. Currently, only the GSA network block is trusted.

### Deployment approach

The app is deployed from CircleCI using the [ZIP deployment method](https://docs.microsoft.com/en-us/azure/app-service/deploy-zip). The app is first built using `dotnet publish -o <build_directory>` and the output is zipped using `cd <build_directory> && zip -r <build_name>.zip .`. Note: the zip file contains the *contents* of the output directory but not the directory itself.

The zip file is then pushed to Azure:

```
    az webapp deployment source config-zip -g <resource_group> -n <app_name> --src <path_to_build>.zip
```
