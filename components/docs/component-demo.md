# Dashboard application

## Prerequisites
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/en/) >= 12.20.0 and `npm` [Node Package Manager](https://docs.npmjs.com/downloading-and-installing-node-js-and-npm) for compiling assets during build

## Local development
To run the app locally:
1. Install the .NET Core SDK development certificate so the app will load over HTTPS ([details](https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-6.0&tabs=visual-studio#trust-the-aspnet-core-https-development-certificate-on-windows-and-macos)):
```
    dotnet dev-certs https --trust
```

2. Run the app using the `dotnet run` CLI command:
```
    cd components/src/Piipan.Components.Demo
    dotnet run
```
Alternatively, use the `watch` command to update the app upon file changes:
```
    cd components/src/Piipan.Components.Demo
    dotnet watch run
```

3. Visit https://localhost:7051

## Building Assets

The app's UI uses [USWDS](https://designsystem.digital.gov/), which necessitates a dependency on NPM. If you make changes to the app's SCSS, you'll need to install USWDS (and related dependencies), by running:
```
    cd components/src/Piipan.Components.Demo
    npm install
```

After installing the dependencies but before making any changes to the SCSS (in `components/src/Piipan.Components.Demo/wwwroot/sass/`), run:
```
    npx gulp watch
```

Gulp will then watch for changes to the SCSS and compile them into the main CSS file.

[Instructions for updating Node dependencies](../../docs/node.md)
