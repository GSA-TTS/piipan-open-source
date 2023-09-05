using System.Collections.Generic;

namespace Piipan.Shared.Tests.TestFixtures
{
    public static class TestDefaults
    {
        private const string MockUserFilePath = @"tests\{0}\MockUsers\{1}";

        /// <summary>
        /// Provies the location of the Mock User file given the assembly name and the state.
        /// For each client integration test project, a mock_user.json file should be saved in the MockUsers\{state} folder
        /// </summary>
        /// <param name="assemblyName">The assembly name of the integration tests project</param>
        /// <param name="state">The state the user is in that we want to mock from</param>
        /// <returns></returns>
        public static string MockUserFile(string assemblyName, string state)
        {
            return string.Format(MockUserFilePath, assemblyName, state);
        }

        /// <summary>
        /// The default configuration settings for tests. Should be similar to the appsettings.Development.json files in the client project
        /// </summary>
        public static Dictionary<string, string> DefaultInMemorySettings =>
            new Dictionary<string, string> {
                {"Claims:Email", "email"},
                {"Claims:Role", "role"},
                {"Claims:LocationPrefix", "Location-"},
                {"Claims:RolePrefix", "Role-"},
                {"Locations:NationalOfficeValue", "National-"},
                {"AuthorizationPolicy:RequiredClaims:0:Type", "email" },
                {"AuthorizationPolicy:RequiredClaims:0:Values:0", "*"},
            };

    }
}
