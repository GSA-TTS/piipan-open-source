using Bunit;
using Microsoft.AspNetCore.Components;
using Piipan.Components.Routing;
using Xunit;

namespace Piipan.Components.Tests.Routing
{
    public class PiipanNavigationManagerTests : TestContext
    {
        [Fact]
        public void CheckNavigationBlockedEventCalledWhenBlockNavigationIsTrue()
        {
            // Arrange
            bool navigationBlockedCalled = false;
            PiipanNavigationManager navigationManager = new PiipanNavigationManager(this.Services.GetService<NavigationManager>());
            navigationManager.BlockNavigation = true;
            navigationManager.NavigationBlocked += (o, e) => navigationBlockedCalled = true;

            // Act
            navigationManager.NavigateTo("/test");

            // Assert
            Assert.True(navigationBlockedCalled);
        }

        [Fact]
        public void CheckNavigationBlockedEventNotCalledWhenBlockNavigationIsTrue()
        {
            // Arrange
            bool navigationBlockedCalled = false;
            PiipanNavigationManager navigationManager = new PiipanNavigationManager(this.Services.GetService<NavigationManager>());
            navigationManager.BlockNavigation = false;
            navigationManager.NavigationBlocked += (o, e) => navigationBlockedCalled = true;

            // Act
            navigationManager.NavigateTo("/test");

            // Assert
            Assert.False(navigationBlockedCalled);
        }

        [Fact]
        public void CheckNavigationBlockedEventNotCalledWhenBlockNavigationIsFalseAndParentLocationChanges()
        {
            // Arrange
            bool navigationBlockedCalled = false;
            var parentNavigationManager = new TestNavigationManager();
            PiipanNavigationManager navigationManager = new PiipanNavigationManager(parentNavigationManager);
            navigationManager.BlockNavigation = false;
            navigationManager.NavigationBlocked += (o, e) => navigationBlockedCalled = true;

            // Act - Simulate a link intercepted by the Blazor router
            parentNavigationManager.NotifyLocationChanged("https://www.example.com/subdir/test", true);

            // Assert
            Assert.False(navigationBlockedCalled);
        }

        [Fact]
        public void CheckNavigationBlockedEventCalledWhenBlockNavigationIsTrueAndParentLocationChanges()
        {
            // Arrange
            bool navigationBlockedCalled = false;
            var parentNavigationManager = new TestNavigationManager();
            PiipanNavigationManager navigationManager = new PiipanNavigationManager(parentNavigationManager);
            navigationManager.BlockNavigation = true;
            navigationManager.NavigationBlocked += (o, e) => navigationBlockedCalled = true;

            // Act - Simulate a link intercepted by the Blazor router
            parentNavigationManager.NotifyLocationChanged("https://www.example.com/subdir/test", true);

            // Assert
            Assert.True(navigationBlockedCalled);
        }

        internal class TestNavigationManager : NavigationManager
        {
            public TestNavigationManager() =>
                Initialize("https://www.example.com/subdir/", "https://www.example.com/subdir/jan");

            public void NotifyLocationChanged(string uri, bool intercepted)
            {
                Uri = uri;
                NotifyLocationChanged(intercepted);
            }

            protected override void NavigateToCore(string uri, NavigationOptions options)
            {
            }
        }
    }
}
