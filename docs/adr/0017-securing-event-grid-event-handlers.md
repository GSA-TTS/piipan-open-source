# 17. Securing Event Grid event handlers

Date: 2021-07-26

## Status

Accepted

## Context

We use Azure Functions with [Event Grid triggers](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-event-grid-trigger?tabs=csharp%2Cbash) as handlers for system events such as blob storage uploads. We want a means of securing these handlers and limiting their access to Event Grid only.

Azure offers a means of delivering system events using managed identities consistent with [our approach for securing our internal APIs](0012-use-app-service-authentication-between-subsystems.md). However, implementing this approach when the event handler is an Azure Function requires [changing the Function to a webhook](https://docs.microsoft.com/en-us/azure/event-grid/handler-functions) and [implementing authentication logic](https://docs.microsoft.com/en-us/azure/event-grid/webhook-event-delivery#endpoint-validation-with-event-grid-events) in the application's code.

An alternative approach is to use Event Grid's built-in [access key authentication](https://docs.microsoft.com/en-us/azure/event-grid/security-authentication#overview) combined with network-level protection in the form of an access restriction on the Azure Function app. 

## Decision

We will secure our Azure Function event handlers using access key authentication and network-level protection. Specifically, all Azure Function event handlers will:

- Use Event Grid triggers
- Have an access restriction that limits inbound access to the `AzureEventGrid` [service tag](https://docs.microsoft.com/en-us/azure/virtual-network/service-tags-overview)

## Consequences

We are prioritizing our aversion to adding custom authentication code — which must be maintained over time — to our applications. This comes at the cost of deviating from our preference for using a zero-trust approach based on managed identities.
