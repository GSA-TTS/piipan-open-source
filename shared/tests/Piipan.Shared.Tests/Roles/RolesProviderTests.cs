using Microsoft.Extensions.Options;
using Piipan.Shared.API.Constants;
using Piipan.Shared.Roles;
using Xunit;

namespace Piipan.Shared.Tests.Roles
{
    public class RolesProviderTests
    {
        const string WorkerRole = "Worker";
        const string ProgramOversightRole = "Oversight";

        RoleOptions roleOptions = new RoleOptions
        {
            { RoleConstants.EditMatchArea, new string[] { WorkerRole } },
            { RoleConstants.ViewMatchArea, new string[] { WorkerRole, ProgramOversightRole } },
        };

        /// <summary>
        /// Make sure the RolesProvider is returning back the acceptable roles for editing a match
        /// </summary>
        [Fact]
        public void GetEditMatchRoles()
        {
            // Arrange
            var options = Options.Create(roleOptions);
            var rolesProvider = new RolesProvider(options);

            // Act
            var roles = rolesProvider.TryGetRoles(RoleConstants.EditMatchArea);

            // Assert
            Assert.Equal(new string[] { WorkerRole }, roles);
        }

        /// <summary>
        /// Make sure the RolesProvider is returning back an empty list when roleOptions is null
        /// </summary>
        [Fact]
        public void GetEditMatchRolesWhenOptionsAreNull()
        {
            // Arrange
            var options = Options.Create<RoleOptions>(null);
            var rolesProvider = new RolesProvider(options);

            // Act
            var roles = rolesProvider.TryGetRoles(RoleConstants.EditMatchArea);

            // Assert
            Assert.Empty(roles);
        }

        /// <summary>
        /// Make sure the RolesProvider is returning back an empty list when roleOptions.EditMatch is null
        /// </summary>
        [Fact]
        public void GetEditMatchRolesWhenEditMatchIsNull()
        {
            // Arrange
            var options = Options.Create(new RoleOptions());
            var rolesProvider = new RolesProvider(options);

            // Act
            var roles = rolesProvider.TryGetRoles(RoleConstants.EditMatchArea);

            // Assert
            Assert.Empty(roles);
        }

        /// <summary>
        /// Make sure the RolesProvider is returning back the acceptable roles for viewing a match
        /// </summary>
        [Fact]
        public void GetViewMatchRoles()
        {
            // Arrange
            var options = Options.Create(roleOptions);
            var rolesProvider = new RolesProvider(options);

            // Act
            var roles = rolesProvider.TryGetRoles(RoleConstants.ViewMatchArea);

            // Assert
            Assert.Equal(new string[] { WorkerRole, ProgramOversightRole }, roles);
        }

        /// <summary>
        /// Make sure the RolesProvider is returning back an empty list when roleOptions is null
        /// </summary>
        [Fact]
        public void GetViewMatchRolesWhenOptionsAreNull()
        {
            // Arrange
            var options = Options.Create<RoleOptions>(null);
            var rolesProvider = new RolesProvider(options);

            // Act
            var roles = rolesProvider.TryGetRoles(RoleConstants.ViewMatchArea);

            // Assert
            Assert.Empty(roles);
        }

        /// <summary>
        /// Make sure the RolesProvider is returning back an empty list when roleOptions.ViewMatch is null
        /// </summary>
        [Fact]
        public void GetViewMatchRolesWhenViewMatchIsNull()
        {
            // Arrange
            var options = Options.Create(new RoleOptions());
            var rolesProvider = new RolesProvider(options);

            // Act
            var roles = rolesProvider.TryGetRoles(RoleConstants.ViewMatchArea);

            // Assert
            Assert.Empty(roles);
        }
    }
}