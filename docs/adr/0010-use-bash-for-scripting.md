# 10. Use bash for scripting

Date: 2021-01-22

## Status

Superceded by [21. Use recent bash version for scripting](0021-use-recent-bash-version-for-scripting.md)

## Context

We need a basic scripting tool to assist with deployment of code, automate tasks in CI/CD, and perform manual integration tests against our deployment environments.

PowerShell and sh/bash are our primary options. bash is installed by default on macOS and in Linux distributions. sh/bash is available on Windows via the Windows Subsystem for Linux (WSL) and Cygwin. PowerShell ships with Windows 10 and is available for macOS and Linux.

In general, our implementation team does not have much experience with PowerShell, while sh/bash scripts are pervasive across 18F projects and our CI/CD tools (e.g., CircleCI, GitHub Actions).

In some environments, `/bin/sh` will be a soft-link to `/bin/bash`. Other times it will be linked to other implementations (e.g., dash or zsh). It is often the case that non-trivial shell scripts that describe themselves as POSIX sh scripts will incorporate useful "bash-isms" which can fail when `/bin/sh` is not actually bash.

## Decision

For now, use bash (explicitly as `/bin/bash`, not `/bin/sh`), as our primary scripting language. Bash scripts should be written to support bash 3.2, which is the latest version of bash on macOS.

## Consequences

* The macOS-equipped 18F team is more productive in the near term, deferring the ramp up on PowerShell.
* A future Windows-equipped team may have to install bash and gain familiarity with it. Or they may strongly prefer PowerShell and the team will need to rewrite any existing bash scripts.
