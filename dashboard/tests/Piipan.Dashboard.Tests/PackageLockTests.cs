using System.IO;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Piipan.Dashboard.Tests
{
    public class PackageLockTests
    {
        /// <summary>
        /// Sometimes when generating the package-lock.json file the peer dependencies for uswds-gulp get added back in.
        /// If this test is failing, manually remove the peer dependencies from the node_modules/uswds-gulp file.
        /// </summary>
        [Fact]
        public void VerifyUSWDSPeerDependenciesDoesNotExist()
        {
            // arrange
            using StreamReader reader = new StreamReader("package-lock.json");
            string json = reader.ReadToEnd();
            var jObject = JObject.Parse(json);
            var gulp = jObject["packages"]["node_modules/uswds-gulp"];

            // assert
            Assert.Null(gulp["peerDependencies"]);
        }
    }
}
