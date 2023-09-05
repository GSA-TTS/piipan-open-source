# Piipan Support tools

*Piipan subsystem for support tools*

## Sources

The directory structure of the subsystem includes a `support-tools/src` directory with the two main components and a `support-tools/tests` directory with unit tests for each component.

The `support-tools/src` directory contains:

* [Support Tools Core Library](./src/Piipan.SupportTools.Core) - which has the logic and data access layer
* [Support Tools API Function App](./src/Piipan.SupportTools.Func.Api) - which has Azure HTTPTrigger event functions

The `support-tools/tests` directory contains:

* [Tests for Support Tools Core Library](./tests/Piipan.SupportTools.Core.Tests) - unit tests for logic and data access layer
* [Tests for Support Tools API Function App](./tests/Piipan.SupportTools.Func.Api.Tests) - unit tests for azure functions

## Summary

### Support Tools API Function App

The Support Tools API Function App has an HTTPTrigger event function called PoisonMessageDequeuer, which is an API endpoint for dequeuing messages from a poison queue. 
The maximum number of messages allowed per execution is `10,000`.
To execute this function, the following steps must be taken:

1. Get an access token by executing the provided command line
`az account get-access-token --resource "api://{APPLICATION ID}" --query accessToken -o tsv`
2. Add the access token as a Bearer Token in the POST request
3. Add the request body as shown in the provided JSON format 
```JSON
{
 "queue_name": "{YOUR QUEUE NAME}",
 "account_name": "{YOUR ACCOUNT NAME}",
 "account_key": "{YOUR ACCOUNT KEY}"
}
```
4. Send the POST HTTP request to the specified URL
`https://{PREFIX}-func-support-tools-{ENV}.azurewebsites.net/api/PoisonMessageDequeuer`

## Testing

Each project in the `piipan.support-tools` subsystem has a dedicated test project in the `support-tools/tests` directory. 

To execute tests for the subsystem as a whole:

``` bash
$ cd support-tools
$ dotnet test
```

To execute tests for a particular subsystem project:

``` bash
$ cd support-tools/tests/Piipan.SupportTools.<*>.Tests
$ dotnet test
```
