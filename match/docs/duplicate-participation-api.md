# Duplicate participation API

## Summary

The duplicate participation API is intended as a collection of external-facing endpoints for consumption by state systems. It is managed as an Azure API Management (APIM) instance and deployed by the [IaC](../../docs/iac.md).

The API includes the following endpoints:

| Endpoint | Backend | Function |
|---|---|---|
| `/find_matches` | [Orchestrator Function App](orchestrator-match.md) | `Find` |

For a general overview of APIM, refer to [Microsoft's documentation](https://docs.microsoft.com/en-us/azure/api-management/). Piipan makes use of the following concepts:

- [APIs](https://docs.microsoft.com/en-us/azure/api-management/api-management-key-concepts#-apis-and-operations)
- [Operations](https://docs.microsoft.com/en-us/azure/api-management/api-management-key-concepts#-apis-and-operations)
- [Policies](https://docs.microsoft.com/en-us/azure/api-management/api-management-howto-policies)
- [Versions](https://docs.microsoft.com/en-us/azure/api-management/api-management-versions)
- [Subscriptions](https://docs.microsoft.com/en-us/azure/api-management/api-management-subscriptions)

## Calling the API

To call an endpoint:

1. [Obtain an API key](#managing-api-keys)
1. Send a request, passing the API key in the `Ocp-Apim-Subscription-Key` header

## Managing endpoints

In APIM, endpoints take the form of operations. Operations are collected together within a parent resource called an API ([details](https://docs.microsoft.com/en-us/azure/api-management/api-management-key-concepts#-apis-and-operations)). Operations and their associated resources are managed in the [apim.json ARM template](../../iac/arm-templates/apim.json). New operations can be added to the duplicate participation API by including additional `Microsoft.ApiManagement/service/apis/operations` resources in the template.

An operation's endpoint is constructed using the following components, all provided via the ARM template:

| Component | Example values | Example URL |
|---|---|---|
| The APIM instance's base gateway URL | `https://<apim-name>.azure-api.net` | `https://<apim-name>.azure-api.net` |
| The API's optional `path` property | `path` | `https://<apim-name>.azure-api.net/path` |
| The API's optional version identifier | `v1` | `https://<apim-name>.azure-api.net/path/v1` |
| The operation's `urlTemplate` property | `/find_matches` | `https://<apim-name>.azure-api.net/path/v1/find_matches` |

Operations are frontend layers for backend services. A client's request is received by an operation and forwarded to a backend server for processing. The operation receives the resulting response from the backend server and completes the loop by forwarding it to the client.

The connection between an operation and a backend server is made by specifying the backend's base URL (e.g., `https://<function-app-name>.azurewebsites.net/api/v1`) as the `serviceUrl` property of the operation's parent API resource.

## Managing API keys

API keys are managed as [subscriptions](https://docs.microsoft.com/en-us/azure/api-management/api-management-subscriptions) in APIM. Currently, keys are managed ad-hoc by system developers via the Azure Portal (Portal > {APIM instance} > Subscriptions).

When creating subscriptions during the onboarding process for states add two new subscriptions: one for the Duplicate Participation API and one for the Bulk Upload API. Subscriptions can be created using provided scripts:

```
    # For example state Echo Alpha (ea) in the tts/dev environment
    ./match/tools/create-apim-match-subscription.bash tts/dev ea
    ./etl/tools/create-apim-bulk-subscription.bash tts/dev ea
```

Subscriptions can also be created manually using the Azure Portal:

- If creating a subscription for the Duplicate Participation API:
    - Name: `{state-abbreviation}-DupPart`; e.g., `EA-DupPart`
    - Display name: `{state-abbreviation}-DupPart`; e.g., `EA-DupPart`
    - Scope: API
    - API: (Current version of the Duplicate Participation API)
    - User: leave blank
- If creating a subscription for the Bulk Upload API:
    - Name: `{state-abbreviation}-BulkUpload`; e.g., `EA-BulkUpload`
    - Display name: `{state-abbreviation}-BulkUpload`; e.g., `EA-BulkUpload`
    - Scope: API
    - API: (Current version of the state-specific Bulk Upload API)
    - User: leave blank

*Note:* The APIM instance relies on a consistent naming format to dynamically derive a state's two-letter postal abbreviation from incoming requests.
