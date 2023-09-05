# Run Integration Test Locally
### Executing Integration test in development environment.

* Following steps are involved in configuring the integration tests to run locally against designated database(s) rather than developer's database resources. This would help developer to troubleshoot any issues in integration test.

### Steps for running test(s) in development environment

1.  Create Core and Participants test database(s).  for e.g.  collabrationtest, metricstest and {tenant}test.
1. Create a .runsettings file in the solution. Test projects in Visual Studio can be configured by using a .runsettings file. For example, you can change the connection string or any environment variables on which the tests are run. Following is the sample of .runsettings file.

    ```
    <?xml version="1.0" encoding="utf-8"?>
        <RunSettings>
        <!-- Configurations that affect the Test Framework -->
        <RunConfiguration>
            <EnvironmentVariables>
                <!-- List of environment variables we want to set-->
                <MetricsDatabaseConnectionString>Server={core host}.postgres.database.cloudapi.net;Database=metricstest;Port=5432;User Id={user};Password={password};Ssl Mode=VerifyFull;</MetricsDatabaseConnectionString>
                <ParticipantsDatabaseConnectionString >Server={participants host}.postgres.database.cloudapi.net;Database=eatest;Port=5432;User Id={user};Password={password};Ssl Mode=VerifyFull;SearchPath=piipan;</ParticipantsDatabaseConnectionString>
                <CollaborationDatabaseConnectionString>Server={core host}.postgres.database.cloudapi.net;Database=collaborationtest;Port=5432;User Id={user};Password={password};Ssl Mode=VerifyFull;</CollaborationDatabaseConnectionString>
            </EnvironmentVariables>
        </RunConfiguration>
    </RunSettings>  
    ```
1. Follow the steps to configure .runsettings file in ([Visual studio](https://learn.microsoft.com/en-us/visualstudio/test/configure-unit-tests-by-using-a-dot-runsettings-file?view=vs-2022#manually-select-the-run-settings-file))
1. Run/Debug tests from the test explorer.
 
