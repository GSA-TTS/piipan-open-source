# 33. Use Liquibase for Db Migrations

Date: 2022-04-28

## Status
 
Accepted
 
## Context

For database changes during early development, we have taken the approach of deleting and recreating the database(s). Without any clients or real data, this simple approach requires little effort and works fine. However, now that states are being onboarded for the MVP we need a strategy for database migrations that avoids deleting existing data.

There are several tools available that offer support for database migrations & source control. Liquibase, Redgate, DbMaestro, and Flyway are four options we considered.

## Decision

We will use Liquibase for running migration scripts and GitHub for source control of these scripts. 

DbMaestro & Redgate's Source Control and Compare tools are both expensive. Redgate only offers support for SQL Server and Oracle. DbMaestro is not SDK friendly and is more UI driven. They both provide state-based database management which is effectively compare and sync tooling. This is appealing in that it generates upgrade scripts for you, but the problem is that the change process is not repeatable. Another problem with state-based management is that the tooling will push changes forward that may have not been reviewed (e.g. someone has made a manual change to the model database rather than through source control).

Flyway and Liquibase both offer migrations-based approaches to database changes. Both support SQL-base migrations and both run from command line. At update time, they both check if changes have already been deployed. This approach provides a repeatable process at the cost of having to write scripts manually. We see that overhead as beneficial rather than prohibitive. Developers are already writing scripts or making database changes manually. Their changes are then incorporated into IaC scripts and reviewed prior to merging into Git. With this approach, they'd follow a similar process, writing change scripts and submit those scripts for code review prior to merging.

The reason we chose Liquibase over Flyway
* Support of both relational and NoSQL database types.
* Can compare the state of two databases
* Allows rollbacks to undo changes
* More flexible set of options for defining database changes/migrations
* The ability to perform dry-runs of migrations

## Consequences

* Developers need to install liquibase.
* Documentation needs to be provided for writing migration scripts and for using liquibase to run them.
* DevOps procedures need to be created to run migration scripts.


## Resources
* [Liquibase](https://www.liquibase.org/)
* [Flyway](https://flywaydb.org/)
* [Redgate SQL Source Control](https://www.red-gate.com/products/sql-development/sql-source-control/)
* [DbMaestro](https://www.dbmaestro.com/)
* [Evolutionary Database Design](https://martinfowler.com/bliki/ParallelChange.html)
