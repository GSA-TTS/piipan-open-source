# 13. Use Azure Private Endpoints for Azure Database for PostgreSQL instances

Date: 2021-05-03

## Status

Accepted

## Context

By default, many Azure PaaS (Platform-as-a-Service) resources are exposed to the internet via a public IP address. This is not ideal because it increases the attack surface of the system. As we move towards production, we would like to improve our security posture by strictly limiting network access of internal resources to only the necessary components.

An Azure Virtual Network (VNet) can be used to create a private overlay network for system components to communicate over and allow us to disable public network access to a PaaS resource. It can also permit us to restrict outbound network traffic (to varying degrees) with a network security group.

Microsoft has two approaches to route network requests between internal resources over a VNet: Service Endpoints and Private Endpoints.

Service Endpoints is an older Azure feature. It has some characteristics to note:

- The destination resource (e.g., PostgreSQL) must remain publicly addressable. Enforcement of inbound network access is done via a resource-specific firewall rule that only permits traffic from a VNet subnet.
- Outbound traffic from the source (initiating) resource is not limited to a specific destination resource, but all resources of that particular service class (e.g., all SQL PaaS resources in Azure). This makes it difficult to add safeguards for data exfiltration.

Private Endpoints is a more recent addition to Azure and has more granular controls:

- It permits network traffic from the VNet to only a specific resource.
- In a least some cases, the public access point for the destination resource can be completely disabled.

Private Endpoints has a recurring cost; very roughly estimating our network usage, it is on the order of $10 a month per endpoint.

Service Endpoints and Private Endpoints are only available for certain PaaS resources. And VNet support is a prerequisite feature for both approaches but VNet support is only available at certain pricing tiers of Azure PaaS offerings. The intersection of endpoint, VNet, and public interface features is complex and not uniform across PaaS and requires further research spikes.
## Decision

We will incorporate the Private Endpoint and VNet approach for just our Azure Database for PostgreSQL instances and the related VNet configuration that allows Function Apps to communicate with those PostgreSQL instances. This choice allows us to completely disable the public interface to our internal databases – the most security sensitive components of the system – without undue cost.

## Consequences

PostgreSQL and App Service plans need to be changed to more expensive plans, General and Premium respectively.

Since it’s more straightforward for resources that communicate with each other to be housed in the same Virtual Network, resources needing the same VNet will be grouped into the same resource group.

Cost may also encourage us to use a different resource group and/or pricing plan schemas for dev and testing environments than we do for production. In other cases, a cost/benefit analysis may lead us to not use a VNet at all in certain, limited circumstances (e.g., API Management only supports VNets at its Premium pricing level, which is almost $3k a month). Finally, cost may also require us to revisit the highly segregated, per-state Function Apps in the system design – at $10 a month per Function App, the Private EndPoint approach becomes prohibitive.

Since we are only initially shifting a portion of our subsystem to communicate over a VNet, that leaves the rest of the system communicating over their publicly addressable IPs, relying on their authentication mechanisms and their PaaS-specific firewalls as security layers.

## Resources
- [What is Azure Private Endpoint?](https://docs.microsoft.com/en-us/azure/private-link/private-endpoint-overview)
- [What is Azure Private Link?](https://docs.microsoft.com/en-us/azure/private-link/private-link-overview)
- [Private Link for Azure Database for PostgreSQL-Single server](https://docs.microsoft.com/en-us/azure/postgresql/concepts-data-access-and-security-private-link)
- [Forum: Service Endpoints vs. Private Endpoints?](https://acloud.guru/forums/az-500-microsoft-azure-security-technologies/discussion/-M5IkN1SzQcDUNRyvaVL/Service%20endpoints%20vs.%20Private%20Endpoints%3F#:~:text=Both%20appear%20to%20allow%20a,IP%20address%20in%20your%20subnet.))
- [Integrate Azure Functions with an Azure virtual network by using private endpoints](https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-vnet)
