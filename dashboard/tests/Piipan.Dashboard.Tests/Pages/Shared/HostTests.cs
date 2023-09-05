using System.Threading.Tasks;
using Xunit;

namespace Piipan.Dashboard.Tests.Pages.Shared
{
    public class HostTests : BasePageTest
    {
        private const string PageName = "/Pages/_Host.cshtml";

        [Fact]
        public async Task TestOnGetHost()
        {
            // arrange
            var renderer = GetViewRenderer();

            // act
            var (page, output) = await renderer.RenderViewWithNoModel(PageName);

            // assert it contains the correct title and both expected Blazor component references
            Assert.Contains("<title>NAC Metrics Dashboard</title>", output);
            Assert.Contains("Piipan.Shared.Client.Components.AppInitiator", output);
            Assert.Contains("Piipan.Dashboard.Client.App", output);
        }
    }
}
