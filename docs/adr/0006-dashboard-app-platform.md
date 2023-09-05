# 6. Dashboard app platform

Date: 2020-10-23

## Status

Accepted

## Context

We will likely need multiple web accessible "dashboard" applications in order to expose functionality and reporting tools via a user interface. Azure offers several services around web apps. We are looking for the lowest friction platform that is not overkill for our current needs.

## Decision

We will use Azure's App Service platform for our web apps.

## Consequences

* Using a PaaS will allow us to focus our effort on development over infrastructure.
* We will be building on [past 18F experience with App Service](https://github.com/AlaskaDHSS/DevSecOpsMvp/tree/master/appservice)
* We will be able to naturally expand into Web App for Containers if needed
* This decision is consistent with [Azure's architecture decision tree](https://docs.microsoft.com/en-us/azure/architecture/guide/technology-choices/compute-decision-tree)