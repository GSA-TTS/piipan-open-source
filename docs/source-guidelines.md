# Guidelines for Piipan .NET source trees

## Source tree checklist

When new source trees (e.g., a new component or test suite) are added to this repository, several items of configuration should be completed:

1. Organize the source tree in a manner generally consistent with this [widely adopted .NET project structure](https://gist.github.com/davidfowl/ed7564297c61fe9ab814).
1. Enable repeatable package restores:
   * Add `<RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>` and `<RestoreLockedMode Condition="'$(ContinuousIntegrationBuild)' == 'true'">true</RestoreLockedMode>` to the `<PropertyGroup>` of the `.csproj` file.
   * Run `dotnet restore` and commit the `package.lock.json` file.
1. Update [dependabot.yml](/.github/dependabot.yml) to monitor the source tree's dependencies.
1. Update the [Snyk console](https://app.snyk.io/org/18fpiipan/projects) to also monitor the source tree's dependencies. Dependabot and Snyk can sometimes flag different things – it's useful to have both.
1. For test suites, update [.circleci/config.yml](/.circleci/config.yml) to run the test code, format the coverage data, and include it in the coverage sum that gets sent to [Code Climate Quality project](https://codeclimate.com/github/18F/piipan).
1. For new components, update [.circleci/config.yml](/.circleci/config.yml) to build and deploy it to Azure. Any infrastructure should be pre-existing and established separately via [create-resources.bash](/iac/create-resources.bash).
1. For any `dotnet` command in CircleCI that calls MSBuild under the hood (e.g., `build`, `test`, `publish`), ensure that the `ContinuousIntegrationBuild=true` property is set. See the job-level `$MSBUILD_PROPS` variable in [.circleci/config.yml](/.circleci/config.yml).
1. Add project to the subsystem solutions (sln) file
1. Add project to the top-level system solutions file (system.sln) at the root of the repo

## Source code conventions

1. Format C# files using the [OmniSharp extension](https://github.com/OmniSharp/omnisharp-vscode) for Visual Studio Code. Add [this omnisharp.json](/tools/omnisharp.json) to `~/.omnisharp` to extend its defaults and automatically organize imports.
1. Avoid importing unused packages. With the OmniSharp extension, Visual Studio Code will highlight packages that can be removed.
1. Avoid unused package dependencies in the `.csproj`.
1. We haven't determined the best practice for line endings for .NET projects developed on macOS. For now, if a .NET tool generates a file using CRLF, we keep it that way.
1. If a .NET tool generates files without a newline at the end of the file (e.g., `package.lock.json`), we don't worry about fixing it.
