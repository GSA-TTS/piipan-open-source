using Bunit;
using Piipan.Components.Modals;
using Xunit;

namespace Piipan.Components.Tests.Modals
{
    public class ModalServiceCollectionExtensionsTests : TestContext
    {
        [Fact]
        public void RegisterModalServices_HasIModalManager()
        {
            // Act
            Services.AddModalManager();

            // Assert
            Assert.NotNull(Services.GetService<IModalManager>());
        }
    }
}