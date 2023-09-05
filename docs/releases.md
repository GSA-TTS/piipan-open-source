# Releases

This project tracks two types of releases, both using [Git tags](https://git-scm.com/book/en/v2/Git-Basics-Tagging):
- Versions intended for production
- Versions at the end of each sprint

## Versioning nomenclature

### Production releases

We adhere to [Semantic Versioning](https://semver.org/spec/v2.0.0.html) for the system version number.

The convention for each release tag is `v<major>.<minor>.<patch>`; e.g., `v1.0.0`, `v1.1.2`.

Previous versions of this product have used different versioning nomenclature approaches that are no longer followed:
* Production releases were originally versioned as `v<major>.<minor>.<patch>.<sprint>`; e.g., `v1.1.1.44`.
* Zero-based versioning (e.g., `v0.1`, `v0.2`, etc) was used initially as a subtle indicator that we did not yet have a release in production.

As production releases are moved through the piipan's remote environments, they are additionally tagged with an environment-specific indicator. The convention for each environment tag is `env-<environment>`; e.g., `env-preprod`, `env-prod`.

### Sprint releases

The convention for each sprint release tag is `sprint-<sprint-number>`; e.g., `sprint-51`, `sprint-52`.

## Creating a production release

### Tagging the release 

Note: the CHANGELOG should be updated before tagging a release. This is done with a commit to `dev` before following the tagging process.

Tagging process:

```
git checkout dev
git tag v<version-number>                   # e.g., v1.2.1
git push origin v<version-number>
```

### Creating the release in GitHub

- Go to Releases tab from main Git repository page.
- Create new release, type in the tag name; e.g., `v1.2.1`. It will appear in drop down. (i.e., you won't be creating it in this UI, you wont specify any particular branch)
- Type in release notes (see other releases for template). You can get a template by going to previous release and click Edit, then copy the text there. Keep the highlights high-level, consolidating similar tickets into a basic description. While the audience for the CHANGELOG is primarily the development team (and integration teams), the release notes is tailored for other stakeholders.
- A draft release (prior to tagging) may already be present to capture deployment notes

### Moving an environment tag after deployment

Releases will be deployed to each of the piipan's remote environments as they move through the approval process. When a release moves to a new environment, the associated environment tag must also be updated. Git does not support re-assigning a tag; it must be deleted and applied to a new commit in separate steps.

```
git tag -d env-<environment>            # e.g., "env-dev"
git push origin :env-<environment>
git checkout v<version-number>
git tag env-<environment>
git push origin env-<environment>
```

## Creating a sprint release

Sprint releases are used to capture changes that were made during the previous sprint and submit work for approval.

Note: the CHANGELOG should be updated before tagging a sprint. This is done with a commit to `dev` before following the tagging process.

### Tagging the sprint release

```
git checkout dev
git checkout -b sprint/<sprint-number>    # e.g., sprint/52
git tag sprint-<sprint-number>            # e.g., sprint-52
git push origin sprint-<sprint-number>
git push origin sprint/<sprint-number>
```

### Creating the GitHub pull request to merge sprint work into the Main branch

Once tagged, go to GitHub:

- Click `New pull request`
- Change base to `main`, set compare to `sprint/<sprint-number>`
- Create pull request
- Wait for tests to finish running
- Add approver(s)
- Wait for approval
- Merge, delete branch

### Creating the GitHub release

- Go to Releases tab from main Git repository page.
- Create new release, type in the tag name; e.g., `sprint-52`. It will appear in drop down. (i.e., you won't be creating it in this UI, you wont specify any particular branch)
- Type in release notes (see other releases for template). You can get a template by going to previous release and click Edit, then copy the text there. Keep the highlights high-level, consolidating similar tickets into a basic description. While the audience for the CHANGELOG is primarily the development team (and integration teams), the release notes is tailored for other stakeholders.
- A draft release (prior to tagging) may already be present to capture deployment notes
