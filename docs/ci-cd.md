# CI/CD Pipeline

## CircleCI

We use CircleCI to automate the build and deployment of our subsystems.

### Environment variables

CircleCI is configured with several environment variables that provide information and credentials necessary to automate deployment to Azure.

| Environment variable | Value |
|---|---|
| `AZURE_SP` | Service principal `appId` (aka `clientId`) |
| `AZURE_SP_PASSWORD` | Service principal `password` (aka `clientSecret`)|
| `AZURE_SP_TENANT` | Service principal `appOwnerTenantId` |
| `CC_TEST_REPORTER_ID` | Code Climate test reporter ID |

The three environment variables beginning with `AZURE_SP` provide CircleCI with credentials for an Azure service principal. The credentials are used to log in to the Azure CLI via a user ID and password that are provided to the [circleci/azure-cli orb](https://circleci.com/developer/orbs/orb/circleci/azure-cli).

A service principal named `piipan-cicd` (note, this is the *display name* and not the *`appId`*) is created as part of the IaC process and intended to be used by CircleCI.

### Retrieving Azure service principal credentials

Service principal credentials are not stored outside of CircleCI and can not be retrieved after they are created. To get credentials, follow the process for refreshing credentials in [`service-principals.md`](service-principals.md):

```
    cd iac
    ./create-service-principal.bash piipan-cicd
```

*Note:* Refreshing credentials will invalidate existing credentials and the `AZURE_SP` and `AZURE_SP_PASSWORD` environment variables will need to be updated.

## Dependency Analysis

Both [Snyk](https://snyk.io) and [Dependabot](https://github.com/features/security) are enabled on this repository to identify known vulnerabilities in software dependencies.

Each repo subdirectory with package references (e.g., a `.csproject` file) should be explicitly configured in both Snyk and Dependabot.

Dependabot is configured via [.github/dependabot.yml](../.github/dependabot.yml). Snyk can be configured by project administrators via its [ web console](https://app.snyk.io/org/18fpiipan/projects).

## Code Coverage

[Code Climate](https://codeclimate.com/) is enabled on this repository to report test coverage results from automated testing in CircleCI. The Code Climate test reporter ID is stored in CircleCI using the `CC_TEST_REPORTER_ID` environment variable, allowing CircleCI to automatically upload coverage reports. The test reporter ID can be found in the Repo Settings section of Code Climate's [web console](https://codeclimate.com/github/18F/piipan).
