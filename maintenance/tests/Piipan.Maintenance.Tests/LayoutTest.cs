using System.Threading.Tasks;
using Piipan.Maintenance.Pages;
using Xunit;

namespace Piipan.Maintenance.Tests
{
    public class LayoutTests : BasePageTest
    {
        private const string PageName = "/Pages/Tests/EmptyViewForTestingLayout.cshtml";

        [Fact]
        public async Task ListPageLink_IsShown_ForNationalUser()
        {
            // arrange
            var renderer = GetViewRenderer();

            // act
            var model = new Index();
            var (page, output) = await renderer.RenderView(PageName, model);

            // assert the list page link is shown when the user is a national user
            Assert.Contains("website belongs to an official government organization in the United States", output);
        }
    }
}
