# 21. Use a recent bash version for scripting

Date: 2021-10-13

## Status

Accepted

Supercedes [10. Use bash for scripting](0010-use-bash-for-scripting.md)

## Context

We need a basic scripting tool to assist with deployment of code, automate tasks in CI/CD, and perform manual integration tests against our deployment environments.

PowerShell and sh/bash are our primary options. bash is installed by default on macOS and in Linux distributions. sh/bash is available on Windows via the Windows Subsystem for Linux (WSL),  Cygwin, and Git for Windows. PowerShell ships with Windows 10 and is available for macOS and Linux. bash and PowerShell are available as options in Azure Cloud Shell.

In general, our implementation team does not have much experience with PowerShell, while sh/bash scripts are pervasive across 18F projects and our CI/CD tools (e.g., CircleCI, GitHub Actions).

In some environments, `/bin/sh` will be a soft-link to `/bin/bash`. Other times it will be linked to other implementations (e.g., dash or zsh). It is often the case that non-trivial shell scripts that describe themselves as POSIX sh scripts will incorporate useful "bash-isms" which can fail when `/bin/sh` is not actually bash.

Finally, it should be noted that for some time [Apple has not updated the version of bash that ships with macOS](http://meta.ath0.com/2012/02/05/apples-great-gpl-purge/), leaving `/bin/bash` at version 3.2, which was released in 2007. [Bash 4.1 fixes a significant error log issue](https://github.com/18F/piipan/issues/1581#issuecomment-941317598) that makes it far easier to identify the source of an error when a script fails.

## Decision

Use bash (explicitly as `/bin/bash`, not `/bin/sh`), as our primary scripting language. Bash scripts should be written to support bash 4.1 or greater.

## Consequences

* The macOS-equipped 18F team is more productive in the near term, deferring the ramp up on PowerShell.
* macOS developers will need to install a recent version of bash, separate from the system supplied version (e.g., via Homebrew).
* A future Windows-equipped team may have to install bash and gain familiarity with it. Or they may strongly prefer PowerShell and the team will need to rewrite any existing bash scripts.