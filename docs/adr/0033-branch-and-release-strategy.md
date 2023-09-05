# 33. Branching and Release Strategy

Date: 2022-08-26

## Status
 
Accepted

## Context

As we officially begin supporting production releases in addition to supporting multiple environments (dev, test, pre-prod, etc.), the likelihood of supporting multiple versions simultaneously is increasing. We have started experiencing merging headaches with our current strategy when Dev, Main, and Preprod all had different versions. Introducing a hotfix involved backing changes out, applying fixes, re-applying backed out changes, modifying old release notes and changelogs, and fixing tags. It's becoming more important to have a solid branching and release strategy in place to support development and minimize overhead.

## Decision

As we strive to move towards a CICD workflow, we will be adopting a modified version of the Trunk-based development workflow. This workflow has increased in popularity due to it support of modern DevOps practices and CICD. Some other options considered include GitFlow, GitHub Flow, & GitLab Flow. 

We will utilize our "Dev" branch as our "trunk" and source of truth. We will strive to keep it releasable at all times. Unlike traditional trunk-based development, we will still require developers to utilize short-lived feature branches and create pull requests for review prior to merging in work. When the Git repository is no longer public, we may revisit this requirement.

We will utilize a "Main" branch as an official branch of stakeholder accepted/approved code. That's the only purpose for it. Commits to this branch would only flow upstream from the "Dev" branch and these merges would occur at the end of each sprint. The merge PRs will stand as a time-stamped approval artifacts that are clearly linked to a specific individual. Having PRs with the results of automated tests/checks readily available will also make review a lot more efficient for stakeholders.

We will also make use of tags for tracking milestones. We will create a "Sprint" tag for the end of every sprint and we will create a "Release" version tag for every potential release candidate. Instead of using the Main branch to track federally approved code, we can utilize tags for tracking federally approved code instead (i.e. every sprint, the commit tagged with the recently finished sprint number can be reviewed and provided a federal approval tag to indicate it's been reviewed and accepted). 

We will also keep "Environment" tags. Each environment will have a moving tag pointing to its current state, this will allow to know instantly which version is deployed in each environment. Upon deploying to an environment, we will move that enviornment's tag to the appropriate commit that the release was generated from.

We will deploy from the "Dev" branch to the development environment any time a PR is merged. If desired, these deployments could also be promoted (i.e. deployed) to subsequent environments if given manual approval during approval gates in the DevOps pipeline. More likely for test and subsequent environments though, releases or release candidates will be manually tagged with a "Release" tag and a DevOps pipeline will be manually initiated (with the desired "Release" tag provided as an input parameter) that deploys that release to the test environment. Promotion to preprod or prod will require manual approval via approval gates in the pipeline.  

If fixes or changes are necessary for a release, we will generate a temporary "hotFix"/"uatFix"/etc. branch from the desired "Release" tag, apply fixes/changes, and deploy the updates. The final commit for the new release containing the fix should be tagged with a new "Release" version tag prior to merging the changes back to the "Dev" branch. Upon successful merge back to "Dev" the temporary branch can be deleted.

All "Release" tags should be protected (i.e. an administrator should mark them so that they cannot be changed or deleted).

For examples of tag usage in regards to typical development workflow scenarios, see [Common Git Workflows & Tag Usage Scenarios](../supporting-files/NAC-Release-Support-Workflows.pptx). Sample DevOps pipeline scenarios depicting the use of tags and branches to support multiple enviornments can be viewed [Sample DevOps Pipelines](../supporting-files/DevOpsPipelineScenarios.pdf).

## Consequences

Developers continue working and updating the "Dev" branch, and releases (and release branches when necessary) can be generated from any tag.

This approach should reduce "Main" branch merge headaches as we no longer worry about keeping the "Dev" branch in sync with the "Main". We will recreate "Main" from "Dev" and from now on, commits will only flow upstream from "Dev" to "Main".

When hotfixes are necessary, a release branch will be generated from the "production" environment tag as mentioned above. This workflow supports making the fixes, applying them, and merging them back to "Dev". 

## Resources
* [Branching Strategies](https://www.flagship.io/git-branching-strategies/)
* [Git Flow](https://www.atlassian.com/git/tutorials/comparing-workflows/gitflow-workflow)
* [GitLab Flow](https://docs.gitlab.com/ee/topics/gitlab_flow.html)
* [Trunk-based development vs Git Flow](https://www.toptal.com/software/trunk-based-development-git-flow)
