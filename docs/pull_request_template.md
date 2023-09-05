## What’s changing?

_For External API changes, add the changelog tag to the PR and some CHANGELOG text here_

## Why?

_Link to specific issue (ex: Closes piipan-123)_

## This PR has:

- [ ] **JIRA ticket(s)** (_Must be listed in PR request and branch name should follow convention piipan-123-description_)<description>

- [ ] **Security Related Changes** (_Security Tab questions have been answered on JIRA ticket(s)_)

- [ ] **Commented source code** (_required on public classes and methods, and elsewhere as appropriate_)

- [ ] **Documented API changes** (_required on public classes and methods, and elsewhere as appropriate_)
    - [ ] External API documentation - OpenApi docs and Generated Docs (e.g. https://github.com/18F/piipan/tree/dev/docs/openapi) 
    - [ ] Internal API documentation (e.g. https://github.com/18F/piipan/tree/dev/metrics/docs/openapi)   

- [ ] **Developer documentation** (_for new build/test/API changes or complex portions of the system_)

- [ ] **Automated unit tests** (_to maintain or increase level of code coverage_)

- [ ] **Changes to IAC** (_add any deployment steps that require manual intervention to the draft release notes_)
    - [ ] Have IaC scripts run successfully against an existing deployed environment?
    - [ ] Have IaC scripts run successfully against a newly deployed environment?
    - [ ] Have any new bash scripts been given the appropriate permissions? (e.g. git update-index --chmod=+x $script)
    - [ ] Do the IaC changes require deleting existing resources?


## Anything else?

_Add other details that are helpful for review here_

## Code Review Checklist:

- [ ] Automated checks all pass? (code builds successfully, tests pass, test coverage meets/exceeds threshold, etc.) 
- [ ] Have all requirements and Acceptance Criteria been met?
- **If there are code changes**
    - [ ] Is there no unnecessary complexity? Code is readable, maintainable, etc.
    - [ ] Do the changes follow SOLID principles?
    - [ ] Are the changes robust? Concurrency considered? Performance? Security (e.g. no sql injections)?
    - [ ] Are newly added dependencies needed and appropriate? Are dependency files (e.g. packages.json & packages.lock.json) updated?
    - [ ] For any new projects/solutions, have they been added to code coverage files?
    - [ ] Public classes & methods are documented    
- **If there are API changes**
    - [ ] Does API make sense? Useful and concise?
    - [ ] Are existing API interfaces/contracts intact? i.e. no breaking changes
    - [ ] Are API tests included?
    - [ ] Are API responses appropriate and documented? (200s, 201, 404s, etc.)
    - [ ] Are internals kept private and not leaked into the API?
    - [ ] External API documentation - OpenApi docs and Generated Docs 
    - [ ] Internal API documentation    
- **If there are IAC changes**
    - [ ] IAC Scripts are readable & maintainable
    - [ ] IAC Scripts are documented
    - [ ] IAC scripts pass required checks
    - [ ] IAC scripts were run successfully by author
- **General Logging Questions**
    - [ ] Do new capabilities (or changed capabilities) include logging?
    - [ ] Is PII not logged?
- **General Error Handling Questions**
    - [ ] Has exception handling been added for new capabilities (or changed capabilities)?
    - [ ] Are errors/exceptions properly communicated to end-users? Were there requirements for user facing error messages?
    - [ ] Are errors/exceptions logged?
    - [ ] Is PII kept out of error/exception logging?
- **If there are Developer Documentation Changes**
    - [ ] General Project documentation (under piipan\docs. e.g. iac.md, architecture.md, etc.)
    - [ ] Github documentation additions/changes (Readmes)
- **General automated test questions**
    - [ ] End-to-End Tests provided (if code implements new user story)
    - [ ] Are tests sufficient?
    - [ ] Corner cases considered?
    - [ ] Are performance tests needed?

