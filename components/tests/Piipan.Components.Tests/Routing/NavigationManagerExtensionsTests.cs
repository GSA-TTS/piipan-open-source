using Bunit;
using Piipan.Components.Routing;
using Xunit;

namespace Piipan.Components.Tests.Routing
{
    public class NavigationManagerExtensionsTests : TestContext
    {
        [Fact]
        public void RegisterPiipanNavigationManager_HasPiipanNavigationManager()
        {
            // Arrange
            Services.AddPiipanNavigationManager();

            // Assert
            Assert.NotNull(Services.GetService<PiipanNavigationManager>());
        }
    }
}