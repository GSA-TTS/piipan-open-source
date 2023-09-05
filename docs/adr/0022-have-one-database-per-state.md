# Have One Database Per State

Date: 2021-12-15

## Status

Accepted

Relates to [4. Default Database](./0004-default-database.md)

## Context

When the project began, our intent was to treat state agencies and their data as "tenants" of a federally-administered platform (NAC). Each state would own their data, and isolation best practices and the principle of least privilege would be applied throughout the system.

In theory, this provided a framework that could be extended to support a future version of the platform, where participant data never left a state’s system. The state could host their data store and the federally-run API orchestrator would bind to that resource rather than its own copy of state participant records. Creating a separate PostgreSQL database for each state in the NAC to which states could upload their data was meant to be a stepping stone toward this long-term goal.

We believe the benefits of this design largely dissolved once we pivoted to our [PPRL approach](../pprl.md), where state agencies de-identify PII before uploading and matches are performed directly against this de-identified data.

We understand that having a separate PostgreSQL database for each state's participant data now brings added complexity to the project without the original intended benefits. This opens up new possibilities for storing participant data in a way that realizes security benefits and cuts development overhead by reducing complexity. In deciding what will be best going forward, we believe the following considerations are important:

1. Using the principle of least privilege and isolation best practices:
  - State agencies should not have bulk access to other states' participant data under any circumstances.
  - State agencies should not be able to modify their data once they've submitted it to the platform. (In the current system, data can be replaced via bulk upload but not modified or deleted.) There also may be no reason for state agencies to read their own data in bulk once it's submitted.
1. Security: how would the new system change the threat model to the data?
1. Scalability: the future data store will contain at least tens of millions of records at a time
1. Performance: how can data be stored to make near real-time matching faster?
1. Well established support within the Microsoft/Azure ecosystem
1. Ease of use for the [Bulk Upload API](../openapi/generated/bulk-api/openapi.md)

## Decision

We will keep our current database structure while remaining open to possibilities for change. Making these changes will be weighed against other competing project priorities.

## Consequences

* We will need to support and maintain a separate PostgreSQL database for each state and ensure schema consistency across all of them.

