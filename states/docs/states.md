# States API

## Local development
To run the app locally:
1. Install the .NET Core SDK development certificate so the app will load over HTTPS ([details](https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-6.0&tabs=visual-studio#trust-the-aspnet-core-https-development-certificate-on-windows-and-macos)):
```
    dotnet dev-certs https --trust
```

2. If using a remote States URI, follow the [instructions](../../docs/securing-internal-apis.md) to assign your Azure user account the `States.Query` role for the remote states Function App and authorize the Azure CLI.

3. Run the app using the `dotnet run` CLI command:
```
    cd query-tool/src/Piipan.States
    dotnet run
```
Alternatively, use the `watch` command to update the app upon file changes:
```
    cd query-tool/src/Piipan.States
    dotnet watch run
```

4. Visit https://localhost:5001