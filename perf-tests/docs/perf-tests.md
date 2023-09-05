## Prerequisites
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download)

## Summary

An initial application for running Bulk Upload performance tests that uploads a desired number of generated participants (with random values).

The test runner is implemented in the `Etl.BulkUpload.Performance.TestRunner` project.

## Environment variables

The following environment variables are required by the runner (alternatively, you can specify this in a local appsettings.json file):

| Name | |
|---|---|
| `AZURE_STORAGE_ACCOUNT` | The name of the desired Azure environment's upload storage account (It should look something like <prefix>steaupload<env> e.g. bgfsteauploaddev) |
| `AZURE_STORAGE_KEY` | The key associated with the desired Azure environment's upload storage account (see the Access keys section of your Storage account) |

## appsettings.json

As an alternative to environment variables, you can create an appsettings.json file and place it under piipan/perf-tests/src/Etl.BulkUpload.Performance.TestRunner. 

The contents of the file should look like the following

{ "AZURE_STORAGE_ACCOUNT": "example_storageaccount", "AZURE_STORAGE_KEY": "example_gvlujs370fIq0TgoF=" }

## Running a bulk upload performance test
There are a couple options to run the test:
* Use dotnet run {arg}
* Build the solution and run the Etl.BulkUpload.Performance.TestRunner.exe executable. Provide the desired number of participants as an argument
* Run it within your IDE