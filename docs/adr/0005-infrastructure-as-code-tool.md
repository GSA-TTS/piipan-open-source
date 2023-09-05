# 5. Infrastructure as Code tool

Date: 2020-10-22

## Status

Accepted

## Context

We want an approach to Infrastructure as Code that allows us to easily replicate our TTS Azure sandbox in environments at our partner agency. Our partners are invested in the Microsoft ecosystem and Azure; we do not anticipate this system needing to support other cloud environments in the foreseeable future. The current 18F engagement team does not have deep experience with either Terraform or Azure Resource Manager templates, though Terraform is used widely at TTS.

## Decision

We will use Azure Resource Manager (ARM) templates to implement an Infrastructure as Code approach.

## Consequences

We anticipate that:
- other partner agency teams will more readily adopt ARM than Terraform
- obscure Azure errors will be (slightly) easier to diagnose without the Terraform abstraction layer
- ARM's ability to recognize when resources are modified outside of the template will be useful in environments where other teams (not 18F) have control
- Terraform's syntax advantage will lessen as [Project Bicep](https://github.com/Azure/bicep) matures
- Azure features will be available in ARM before they appear in the Terraform Azure provider

## References

- [Comparing Azure ARM and Terraform](https://kari-marttila.medium.com/comparing-azure-arm-and-terraform-d2b38975c7ea)
- [what-if operation in Azure/bicep](https://github.com/Azure/bicep/issues/552)
- [Validating ARM Templates with ARM What-if Operations](https://blog.tyang.org/2020/04/26/validating-arm-templates-with-arm-what-if-operations/)
- [Using Terraform with Azure - What's the benefit?](https://samcogan.com/terraform-and-azure-whats-the-benefit/)
