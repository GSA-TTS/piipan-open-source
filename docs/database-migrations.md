# Database Migrations

### Overview of Folder Structure

* There will be a master changelog file at the top level or root of each Database's source control folder. Under each folder will be a series of quarterly folders each containing sql migrations created during that period. Each migration is a .sql file(s) and each sibling db-changelog.xml file will reference all of the scripts in that quarterly folder. The reason for the quarterly folders is to prevent the master-changelog.xml file from growing too large. It provides a means of breaking it up into a smaller series of dependent changelog files.

```
- databases
    |
    |--collaboration
    |    |
    |    |--master-changelog.xml
    |    |--migrations
    |        |
    |        |-2022-Q4
    |            |
    |            |-<migration script .sql files>
    |            |-database-changelog.xml
    |
    |--metrics
    |
    |--participants
```

### Steps for Creating a New Migration Script

1. Write sql migration script(s) 
    1. Written with the goal of Evolutionary Database Design in mind
    1. Should be idempotent if possible. This isn't always possible or in some cases not practical. But we should strive for idempotency.
    1. NOTE: Once a script has been applied to a database, liquibase will skip over it on future runs.
1. Name the file following a convention/format "mm_dd_{Script description}.sql". Naming should include a good description (e.g. 07_18_insert-state-info.sql)
1. Specify the author. It should be the author's email address
1. Set the "logicalFilePath" attribute to "path-independent". Without this setting, liquibase considers changesets path-dependent and it will attempt to rerun changesets if/when you run liquibase from a different directory from where you previously ran it.
1. Save the script as a sql file in the appropriate Git folder. (e.g. iac/databases/collaboration/migrations/2022-Q4)
1. Update the Liquibase changelog file with a changeset reference to the new script. e.g. In iac/databases/collaboration/migrations/2022-Q4/database-changelog-2022-Q4.xml as a new changeset block like the following
    1. Set relativeToChangelogFile attribute to true
    1. Set path attribute to the migration script's file name
    1. Set splitStatements = true if the script contains multiple statements to execute separately. e.g. it contains multiple update statements

```
    <changeSet id="05_30_match-record" logicalFilePath="path-independent" author="john.doe@abcd.test" labels="match-record" context="dg-initialization">
        <comment>768</comment><!-- Piipan-1234 ticket -->
        <sqlFile relativeToChangelogFile="true" path="05_30_match-record.sql" splitStatements="true"/> <!-- with >
        <!-- <rollback>
            <sqlFile relativeToChangelogFile="true" path="example-rollback-changeset.sql" splitStatements="false"/>
        </rollback> -->
    </changeSet>
```

1. Changeset Ids
    1. Make the changeset Ids should be the same as the sql file name (without the .sql suffix). Liquibase advises that using the file name itself is more unique than a date based naming convention. 
    1. It's worth noting that ids are used only as an identifier, they do not direct the order that changes are run. Changesets are executed in the order they are listed in the changelog files.

```
<changeSet id="05_30_match-record" logicalFilePath="path-independent" author="john.smith@abcd.test" labels="match-record" context="dg-initialization">
```

1. Include a comment tag in the changeset that contains the JIRA ticket number. Just the numeric value ONLY e.g. for Piipan-768 you would put

```
        <comment>768</comment><!-- piipan-768 ticket -->
```

1. Commit script and changelog updates to Git. Prior to check-in each developer should run both their migration and rollback script through liquibase to verify they are valid, run successfully, and are idempotent. 
1. CICD runs migration against DEV databases upon successful PR merge.

## Running liquibase via command line

Before starting, add a LIQUIBASE_HOME environment variable set to the location of your liquibase install (e.g. LIQUIBASE_HOME=C:\Program Files\liquibase)


### Steps for Applying Database Migration(s) for Participants database

In a command prompt,

```
cd iac/databases/participants
liquibase --changeLogFile=master-changelog.xml --username={user} --password={password} --url=jdbc:postgresql://{participants host}.postgres.database.usgovcloudapi.net/{state database} --liquibase-schema-name=piipan --headless=true update
```

1. Named parameters in the script can be set by appending -D<name of parameter>="<parameter value>" at the end of the above statement e.g. -Dadmin="id_eaadmin_bgf" -Dreader=readonly -Dsuperuser=postgres
1. For command line arguments/parameters that don't change often, consider incorporating a liquibase.properties file (see [sample properties file](../iac/databases/sample.liquibase.properties)) at the root of the database folder, next to the master-changelog.xml file.

### Steps for Rolling a Database Migration back for Participants database

In a command prompt,

```
cd iac/databases/participants
liquibase --changeLogFile=master-changelog.xml --username={user} --password={password} --url=jdbc:postgresql://{participants host}.postgres.database.usgovcloudapi.net/{state database} --liquibase-schema-name=piipan --headless=true rollback-count 1
```

### Steps for Applying Database Migration(s) for Metrics database 

In a command prompt,

```
cd iac/databases/metrics
liquibase --changeLogFile=master-changelog.xml --username={user} --password={password} --url=jdbc:postgresql://{core host}.postgres.database.usgovcloudapi.net/metrics --liquibase-schema-name=public --headless=true update
```

### Steps for Rolling a Database Migration back in Metrics database 

In a command prompt,

```
cd iac/databases/metrics
liquibase --changeLogFile=master-changelog.xml --username={user} --password={password} --url=jdbc:postgresql://{core host}.postgres.database.usgovcloudapi.net/metrics --liquibase-schema-name=public --headless=true rollback-count 1
```

### Steps for Applying Database Migration(s) for Collaboration database 

In a command prompt,

```
cd iac/databases/metrics
liquibase --changeLogFile=master-changelog.xml --username={user} --password={password} --url=jdbc:postgresql://{core host}.postgres.database.usgovcloudapi.net/collaboration --liquibase-schema-name=public --headless=true update
```

### Steps for Rolling a Database Migration back in Collaboration database 

In a command prompt,

```
cd iac/databases/metrics
liquibase --changeLogFile=master-changelog.xml --username={user} --password={password} --url=jdbc:postgresql://{core host}.postgres.database.usgovcloudapi.net/collaboration --liquibase-schema-name=public --headless=true rollback-count 1
```

## Resources
* [Liquibase](https://www.liquibase.org/)
* [Evolutionary Database Design](https://martinfowler.com/articles/evodb.html)
