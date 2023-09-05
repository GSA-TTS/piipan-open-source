# Naming convention for Azure resources

Our naming convention for Azure resources is based on the [Azure guidelines](https://docs.microsoft.com/en-us/azure/cloud-adoption-framework/ready/azure-best-practices/resource-naming) with a few tweaks:

&lt;prefix>-&lt;resource_type>-&lt;app_name>-&lt;environment>

For example:
```
sub-func-metricsapi-dev
```
This resource name would be interpreted as referring to an Azure Function App, which implements our metrics API, for the development environment, hosted within an Azure subscription.

A few Azure resource types (e.g., storage accounts) have [very restrictive naming rules](https://docs.microsoft.com/en-us/azure/azure-resource-manager/management/resource-name-rules) that prevent the use of hyphens and/or significantly limit name length. In these cases:
- drop hyphens, but otherwise apply the naming convention;
- shorten &lt;app_name> until it fits within the required character length and document it.

## Naming Components

| Name | Description | 
| ---- | ----------- |
| prefix | Makes resource name globally unique in an Azure cloud, typically a short value that denotes agency/service. It is optional for resources that are not global in scope, e.g., VNets, resource groups, subscription topics. |
| resource_type | abbreviation of resource type, based on the [Azure documentation](https://docs.microsoft.com/en-us/azure/cloud-adoption-framework/ready/azure-best-practices/resource-naming#example-names-general); e.g., `psql`, `func`, `app` |
| app_name | concise name to denote purpose |
| environment | either `dev`, `test`, `stage`, or `prod`|


## Examples:

| resource | example |
| -------- | ------- |
| Resource Group | `rg-core-dev` |
| Key Vault | `sub-kv-metrics-dev` |
| Managed Identity | `id-eaadmin-dev` |
| Database for PostgreSQL | `sub-psql-metrics-dev` |
| Function App | `sub-func-metricscollect-dev` |
| Storage Account | `substmetricsapidev` |

## Notes

Azure regions are not currently incorporated into the naming convention. This may need to be revisited as we address disaster recovery scenarios.