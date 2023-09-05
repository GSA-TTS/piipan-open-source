# Branching

This project incorporates branching for various different development scenarios

## Feature Branch Development

For all feature & bug related development, a new Git branch should be utilized for all work. 

### Creating the feature/bug branch off of Dev branch

When creating the branch, the name should follow the convention "<jira ticket number>-<optional description>" e.g. nac-1960-add-maintenance-page or in the case of multiple tickets nac-1960-nac-1962-add-maintenance-page

```
git checkout dev
git pull
git checkout -b <jira ticket number-<optional description> # e.g. git checkout -b nac-1960-add-maintenance-page" 
git push origin <jira ticket number-<optional description> # e.g. git push origin nac-1960-add-maintenance-page
```

### Creating the GitHub pull request to merge feature/bug work into the Dev branch

Upon completion, a Pull Request should be generated, reviewed, and eventually merged into Dev GitHub. 

Once development is complete, go to GitHub:

- Click `New pull request`
- Change base to `dev`, set compare to `<jira ticket number>-<optional description>`
- Create pull request
- Wait for tests to finish running
- Add approver(s)
- Wait for approval
- Merge, delete branch

## Fixes & Patches for issues found during UAT Testing of New Releases In Preproduction environment

There may be times when a fix is needed to address an issue discovered during UAT testing of a planned release. In this scenario, a new Git Release branch should be generated from that version's release tag. We'll refer to this as a "UAT" branch. All patch/fix work should be performed on this UAT branch and then merged down to the DEV branch upon completion.

### Creating the UAT branch

```
git checkout -b release/<uat-version-number> v<uat-version-number> # e.g. git checkout -b release/1.4.0 v1.4.0 generates a branch named "release/1.4.0" from the tag "v1.4.0"
git push origin release/<version-number>
```

### Tagging a new release version for the fixes/patches
Upon completion of the fix(es), the changes can be redeployed to the Preproduction environment for continued UAT testing. The changes should be tagged with an incremented version number e.g for this example it would be v1.4.1

```
git tag v<version-number>   # e.g., "v1.4.1" 
git push origin v<version-number>
```

### Creating a new GitHub Release to capture the fixes/patches
Follow the steps at [Releases Documentation](./releases.md#creating-the-release-in-github) for creating an associated GitHub Release and moving the environment tag.

### Upon completion of UAT testing for the planned release, create a GitHub pull request to merge the fixes/patches down into the Dev branch

Upon UAT Testing completion, a Pull Request should be generated, reviewed, and eventually merged into the Dev Branch in GitHub. 

Once development is complete, go to GitHub:

- Click `New pull request`
- Change base to `dev`, set compare to `release/<uat-version-number>`
- Create pull request
- Wait for tests to finish running
- Add approver(s)
- Wait for approval
- Merge, delete branch

## Developing a Hotfix for Production

We consider a hotfix to refer to fixes needed in Production immediately, fixes that cannot afford to go through the normal development & testing channels and timelines (e.g. a Zero-day bug is discovered and needs to be addressed immediately). When a hotfix is needed in Production, a new Git Release branch should be generated from the current tag in Production and then utilized for the hotfix development work. 

### Creating the Release branch

When a fix is necessary in Production the following steps should be taken to establish a Release branch for developers to use for creating fixes

```
git checkout -b release/<version-number> v<version-number> # e.g. git checkout -b release/1.3.0 v1.3.0 generates a branch named "release/1.3.0" from the tag "v1.3.0"
git push origin release/<version-number>
```

### Tagging the Hotfix Release
Upon completion of the work, the changes can be redeployed to Production (or potentially the Preproduction environment first for testing). The changes should be tagged with an incremented version number e.g for this example it would be v1.3.1

```
git tag v<version-number>   # e.g., "v1.3.1" 
git push origin v<version-number>
```

### Creating GitHub Release for the Hotfix & Updating Environment Tags
Follow the steps at [Releases Documentation](./releases.md#creating-the-release-in-github) for creating an associated GitHub Release and moving the environment tag.

### Creating the GitHub pull request to merge hotfix work down into the Dev branch

Upon completion, a Pull Request should be generated, reviewed, and eventually merged into Dev GitHub. 

Once development is complete, go to GitHub:

- Click `New pull request`
- Change base to `dev`, set compare to `release/<version-number>`
- Create pull request
- Wait for tests to finish running
- Add approver(s)
- Wait for approval
- Merge, delete branch

## Hotfix to Production during UAT Testing of New Release In Preproduction environment

There may be times when a hotfix is needed in Production (e.g. v1.3) while in the midst of UAT testing for a planned new release (e.g. v1.4). For this scenario, two new Git Release branches should be generated- 1) One for the current Production release from that release's tag 2) One for the release being UAT tested from that version's release tag (we'll refer to this as a "UAT" branch.). All hotfix work should be performed on the Production release branch and then merged down to the "UAT" branch and then that branch should eventually be merged down to DEV.

### Creating the Release Branch
Follow the same steps as above to [generate the Release branch](#developing-a-hotfix-for-production)

### Creating the UAT branch
Follow the same steps as above to [generate the UAT branch](#creating-the-uat-branch)

### Tagging the Hotfix Release
Upon completion of the work, the changes can be redeployed to Production (or potentially the Preproduction environment first for testing). 

### Creating GitHub Release for the Hotfix & Updating Environment Tags
Follow the steps at [Releases Documentation](./releases.md#creating-the-release-in-github) for creating an associated GitHub Release and moving the environment tag.

### Creating the GitHub pull request to merge hotfix work down into the UAT branch
Upon completion, a Pull Request should be generated, reviewed, and eventually merged into the UAT Branch in GitHub. 

Once development is complete, go to GitHub:

- Click `New pull request`
- Change base to `release/<uat-version-number>`, set compare to `release/<version-number>`
- Create pull request
- Wait for tests to finish running
- Add approver(s)
- Wait for approval
- Merge, delete branch

### Upon completion of UAT testing for the new release, create a GitHub pull request to merge all fixes (including the hotfix) down into the Dev branch

Upon UAT completion, a Pull Request should be generated, reviewed, and eventually merged into the Dev Branch in GitHub. 

Once development is complete, go to GitHub:

- Click `New pull request`
- Change base to `dev`, set compare to `release/<uat-version-number>`
- Create pull request
- Wait for tests to finish running
- Add approver(s)
- Wait for approval
- Merge, delete branch

