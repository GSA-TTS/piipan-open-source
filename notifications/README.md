# Piipan Notifications

*Piipan subsystem for sending Notifications to users after events have occured*

## Sources

The `Notifications/src` directory contains:

* [Notifications Core Library](./src/Piipan.Notifications/Piipan.Notifications.Core) - collection of services that can be used to publish emails to an Azure Event Grid
* [Notifications API Function App](./src/Piipan.Notifications/Piipan.Notifications.Func.Api) - Takes emails from the queue and sends them out to recipients
* [Notification Common Function App](./src/Piipan.Notifications/Piipan.Notification.Common) - contains email templates and email rendering logic

## Summary

### Notifications API Function App

The Notifications API is a Function App that takes emails from an Azure Event Grid queue and sends them to the email recipients. This logic will retry if an error occurs.

### Notifications Core Class Library

Notifications Core is a class library that can be referenced by any other project that needs to send notifications. After referencing the project, simply inject the NotificationService and call the PublishNotifications method.

### Notifications Common Class Library

Notifications Common is a class library that should be referenced by any other project that needs to send notifications along with Core. This project contains the email templates for sending notifications. If you need a new email template for new functionality, you can add them to the Templates folder.

## Environment variables for Notifications.Func.Api

For the Piipan.Notifications.Func.Api, the following environment variables are required and are set by the [IaC](../docs/iac.md):

| Name | |
|---|---|
| `smtpserver` | [details](../../docs/iac.md#\:\~\:text=smtpserver) |

## Environment variables for Projects referencing Core and Common projects

For projects referencing the Piipan.Notifications.Core and Piipan.Notifications.Common libraries, the following environment variables are required and are set by the [IaC](../docs/iac.md):

| Name | |
|---|---|
| `EventGridNotifyEndPoint` | [details](../../docs/iac.md#\:\~\:text=EventGridNotifyEndPoint) |
| `EventGridNotifyKeyString` | [details](../../docs/iac.md#\:\~\:text=EventGridNotifyKeyString) |
| `QueryToolUrl` | [details](../../docs/iac.md#\:\~\:text=QueryToolUrl) |
| `SmtpReplyToEmail` | [details](../../docs/iac.md#\:\~\:text=SmtpReplyToEmail) |

## Local development

Local development for the Notifications.Func.Api is currently limited as a result of not being able to connect to an SMTP server locally.

To build and run the app with this limited functionality:

1. Fetch any app settings using `func azure functionapp fetch-app-settings {app-name}`. The app name can be retrieved from the Portal.
1. Run `func start` or, if hot reloading is desired, `dotnet watch msbuild /t:RunFunctions`.

## Testing

Each project in the `Piipan.Notifications` subsystem has a dedicated test project in the `Notifications/tests` directory. 

To execute tests for the subsystem as a whole:

``` bash
$ cd Notifications
$ dotnet test
```

To execute tests for a particular subsystem project:

``` bash
$ cd Notifications/tests/Piipan.Notifications.<*>.Tests
$ dotnet test
```
## Remote testing

To test the Notifications API remotely:
1. Follow the [instructions](../../docs/securing-internal-apis.md) to assign your Azure user account the `Notifications.Read` role for the remote Notifications Function App and authorize the Azure CLI.
1. Retrieve a token for your user using the Azure CLI: `az account get-access-token --resource <Notifications application ID URI>`.
1. Send a request to the remote endpoint—perhaps using a tool like Postman or `curl`—and include the access token in the Authorization header: `Authorization: Bearer {token}`.