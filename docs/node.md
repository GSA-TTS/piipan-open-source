# Managing Node Dependencies

Two Piipan subsystems use Node: the [Query Tool](../query-tool) and [Dashboard](../dashboard) (both web applications). These web apps use Node mainly to manage the build pipeline for [USWDS](https://designsystem.digital.gov/) stylesheets and other assets, which is done through [Gulp](https://github.com/uswds/uswds-gulp).

In Github, Dependabot will alert us to new available versions of Node packages by opening a Pull Request for each package and subsystem. How much manual testing we do for these PR's relies on the level of our frontend test coverage. Currently, the Cypress test suites are included in CI, so if all checks are green on these PR's, it's generally safe to merge them. If you want to manually test the effects of a node package upgrade (like for example, a major version change), follow the instructions below.

## Updating USWDS

**warning**: Running the task `gulp init` will override USWDS-related files, including our root stylesheet `styles.scss`. Do not run this task unless you intend to upgrade USWDS and re-implement any custom theme configuration or styles.

USWDS assets are now vendored into the web apps through our MSBuild processes (i.e. `dotnet run`, `dotnet build`, `dotnet publish`). Any upgrade the the USWDS npm package version should be tested (including manual testing) for any broken styles or frontend interactivity.

## Manually testing dependency upgrades

Dependabot often will create PR's in bulk. It's best to work on one PR at a time, from oldest to newest so that Dependabot can rebase the newer branches in a consistent way.

Steps for all subsystems are the same. These steps use dashboard as an example.

For the related subsystem on the Dependabot PR:

1. First, read the release notes provided by Dependabot in the PR and check if the version update is a Major, Minor, or Patch [release](https://docs.npmjs.com/about-semantic-versioning). If the update has possible breaking changes, then frontend testing prior to merging becomes more critical.
1. Checkout the Dependabot branch locally
1. Navigate to the project root where `package.json` is located: `cd dashboard/src/Piipan.Dashboard`
1. Build the project by running `dotnet build` which will call `npm install` to update the node dependencies.
1. Start the dev server by running `dotnet run --no-build`. Since node packages were updated in the previous step, you can pass the `--no-build` flag here to avoid redundant installation.
1. If the app spins up successfully and frontend tests pass, cancel the above process.
1. When CI checks go green, approve the Dependabot PR for merging and merge if you are responsible for merging.
1. If there are other outstanding PR's for Node updates, Dependabot will take some time to rebase them. It's best to wait until this is finished for another PR before going through these steps for that PR.

### Do I check in my Node package lockfiles?

Our CI processes seek to use the latest stable version of Node. Your local version of Node may be different.

Differing node versions can result in package lockfile changes between local and remote builds. If your `package-lock.json` file updates during local development, drop these changes in favor of the lockfile changes that result from Dependabot PR's.

There is an opportunity to containerize both remote and local development to avoid these discrepancies.

## References
- [NPM](https://docs.npmjs.com/about-npm)
- [USWDS](https://designsystem.digital.gov/)
- [uswds-gulp](https://github.com/uswds/uswds-gulp)
- [Gulp.js](https://gulpjs.com/)
- [Dependabot on Github](https://docs.github.com/en/code-security/supply-chain-security/keeping-your-dependencies-updated-automatically/enabling-and-disabling-version-updates)
