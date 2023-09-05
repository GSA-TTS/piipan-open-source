# Bulk upload API

## Summary

An external-facing API for uploading [bulk participant records](bulk-import.md). Designed as a frontend for the [Blob service REST API's](https://docs.microsoft.com/en-us/rest/api/storageservices/blob-service-rest-api) [`Put Blob` operation](https://docs.microsoft.com/en-us/rest/api/storageservices/put-blob) that abstracts away authentication complexity.

Implemented as one API for each per-state storage account, all managed in a single Azure API Management (APIM) instance. The APIM instance is shared with the [duplicate participation API](../../match/duplicate-participation-api.md).

## APIM implementation

The [IaC](../../iac/arm-templates/apim.json) creates _N_ per-state APIs within the APIM instance. Each API resource contains a single `PUT` operation. The API resource is configured to require an active subscription.

An [APIM policy](../../iac/apim-bulkupload-policy.xml) is applied to the API to handle authentication and enrich the incoming request with required headers. Specifically, the policy:

- Uses the `authentication-managed-identity` [policy](https://docs.microsoft.com/en-us/azure/api-management/api-management-authentication-policies#ManagedIdentity) to authenticate with the state storage account using the APIM instance's system-assigned identity and add the necessary `Autorization` header to the request.
- Automatically adds the required `Date`, `x-ms-version`, and `x-ms-blob-type` headers and sets them to appropriate values.

## Endpoints

The bulk upload API consists of a single endpoint which receives `PUT` requests:

| Endpoint | Backend |
|---|---|
| `/upload/{filename}` | [`Put Blob` operation](https://docs.microsoft.com/en-us/rest/api/storageservices/put-blob) |

The base URL for the endpoint is configured per-state according to the following format:

```https://<apim-base-uri>/bulk/<state>/<version>/```

For example, the endpoint for uploading `example.csv` to state EA in the `tts/dev` environment might be:

```https://tts-apim-duppartapi-dev.azure-api.net/bulk/ea/upload/example.csv```

## Calling the API

To call an endpoint:

1. [Obtain an API key](../../match/docs/duplicate-participation-api.md#managing-api-keys)
1. Send a [valid request](openapi/openapi.yaml), passing the API key in the `Ocp-Apim-Subscription-Key` header
