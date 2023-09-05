# 12. Use App Service authentication between subsystems

Date: 2021-02-16

## Status

Accepted

## Context

Piipan's microservices-oriented architecture stipulates multiple Azure Function Apps operating as internal APIs [accessible over HTTP](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-http-webhook-trigger?tabs=csharp). We require a means of securing these APIs and granting access to authorized consumers in the form of other Azure applications (e.g., Function Apps, App Service apps, API Management instances), service principals, and developers.

Microsoft promotes three possible approaches for [securing APIs in production](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-http-webhook-trigger?tabs=csharp#secure-an-http-endpoint-in-production):

- App Service authentication (aka Easy Auth)
- Azure API Management
- Isolated App Service Environment

In discussions with folks from Microsoft Solutions, App Service authentication has been suggested as the appropriate approach for our use case.

## Decision

We will use App Service authentication to secure our APIs and allow communication between Piipan sub-systems. Additionally, all APIs will be configured with an [application role](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-add-app-roles-in-azure-ad-apps) and to [require users be assigned that role](https://docs.microsoft.com/en-us/azure/active-directory/manage-apps/assign-user-or-group-access-portal#configure-an-application-to-require-user-assignment).

## Consequences

- Our IaC will likely need to make use of [deployment scripts](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/deployment-script-template) to handle the necessary Active Directory configuration from within an ARM template.

## Resources
- [Securing internal APIs](../securing-internal-apis.md)
- [Secure an HTTP endpoint in production](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-http-webhook-trigger?tabs=csharp#secure-an-http-endpoint-in-production)
- [How to: Add app roles to your application and receive them in the token](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-add-app-roles-in-azure-ad-apps)
- [Configure an application to require user assignment](https://docs.microsoft.com/en-us/azure/active-directory/manage-apps/assign-user-or-group-access-portal#configure-an-application-to-require-user-assignment)
