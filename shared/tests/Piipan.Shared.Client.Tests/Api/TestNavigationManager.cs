using Microsoft.AspNetCore.Components;
using Piipan.Components.Routing;

namespace Piipan.Shared.Client.Tests.Api
{
    public class TestNavigationManager : NavigationManager
    {
        public TestNavigationManager()
        {
            Initialize("https://test.example/", "https://test.example/");
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            NotifyLocationChanged(false);
        }
    }
    public class TestPiipanNavigationManager : PiipanNavigationManager
    {
        public TestPiipanNavigationManager() : base(new TestNavigationManager())
        {
            Initialize("https://test.example/", "https://test.example/");
        }
    }
}
