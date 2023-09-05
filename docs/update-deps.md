# Updating .NET dependencies

The .NET ecosystem and the GitHub Dependabot have a few rough edges as far as updating dependencies go:
* [.NET dependency pinning/locking isn't straight forward](https://github.com/18F/piipan/pull/158)
* [Dependabot doesn't support lock files](https://github.com/18F/piipan/pull/165#issuecomment-752654442)
* [.NET tools don't update lock files when we'd expect them to](https://github.com/18F/piipan/pull/183#pullrequestreview-563530549)

One consequence is that Dependabot PRs can not be directly used to update our dependencies. They can only merely alert us that we must manually run the process below.

## Manual steps
1. For each affected source/test tree (e.g., directory with a `.csproj`), run: 
```
    dotnet list package --outdated
```
At times, you may need to run `dotnet restore` in the directory before `dotnet list package` will run correctly.

2. For each out-of-date package listed, run:
```
    dotnet add package <PACKAGE_NAME>
```
If you do not specify the `--highest-minor` option, major versions will be considered.

3. Update the package lockfile:
```
    dotnet restore --force-evaluate
```
4. Merge in updated `.csproj` and `packages.lock.json` files. The Dependabot PRs will automatically rebase and close themselves.

## Semi-automated steps

To recursively look for packages that can be updated, run:
```
./tools/update-packages.bash .
```
By default it will look only for minor updates. To look for major updates, run:
```
./tools/update-packages.bash . --highest-major
```
but inspect major updates carefully to avoid .NET 5 incompatibility issues.

## Notes

*  Adding a completely brand new package with `dotnet add package` will update the project's corresponding lock file without any subsequent commands.

## References

* [`dotnet list package`](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-list-package)
* [`dotnet add package`](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-add-package)
