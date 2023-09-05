# 2. Deployment platform

Date: 2020-10-22

## Status

Accepted

## Context

Our partner agency has two primary deployment platforms:

1. RHEL or Windows-based virtual machines and either MySQL, MSSQL, or Oracle databases, hosted in an agency-run datacenter
1. Azure Government cloud environment, available in six regions

Some of the key quality attributes of the partner's product vision center on maintaining confidentiality of any stored PII and minimizing the stored PII.

## Decision

We will target the Azure Government cloud environment, beginning in our TTS Azure sandbox with an eye to transition to our partner's Azure environment once we gain access to it.

## Consequences

By choosing Azure, we can leverage several of its properties across a few dimensions:
- Improved security posture – enables additional layers of defense to keep PII confidential
  - Management of encryption keys can be delegated to Azure Key Vault, which includes hardware security modules to help prevent key leakage.
  - Ability to apply Infrastructure as Code techniques, which increases the auditability of the system, automation, and improves consistency across development/pre-production/production environments.
  - Azure Functions eliminate the maintenance/security/compliance traditionally required for virtualization abstractions.
  - Lower-level security primitives for compute, storage, network isolation – permitting finer granularity on application of the principle of least privileges.
- Better cost elasticity – gives us the resources we need at lower cost
  - To achieve some of the requirements and quality attributes, it would be advantageous to be able to provision compute resources with large amounts of RAM nightly, perform calculations, then deprovision the resource.
- More comprehensive services – allows us to accelerate development and reduce compliance footprint
  - Azure offers commonly-needed standard services that allow for efficient roll-out of product features. In particular, the Azure Storage service seems to map well to the extract-transform-load requirement of the system.

Our early discovery has identified two co-mingled risks in adopting Azure: 
1. Virtual machines (e.g., running Windows and Linux) in partner's Azure must connect to an Active Directory to manage policy and authentication of system administrators to those virtual machines.
 
 *Mitigation*: We are recommending a container-based deployment (not virtual machines), hosted in one of the Azure container platforms. Our conversation with the partner's centralized IT group indicated that this would very likely not require connection to Active Directory. Further, container-based deployments are a very important architectural pattern that other agency teams were pursuing. 
 

2. In the event our application containers truly did need to connect to Active Directory, the existing Active Directory used in Azure is only for internally-facing applications. A solution for externally-facing applications is forthcoming.
 
 *Mitigation*: This system needs to be exposed externally by September 30, 2021 – almost a 12 month lead time. Early estimates from the agency's IT group seems to have them resolving this issue months ahead of that date. Our intended Infrastructure as Code approach and automated deployments make it simple to tear down and re-build easily, if it was required due to shifting Active Directory requirements.
