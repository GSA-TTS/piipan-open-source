using System.Threading.Tasks;
using Piipan.QueryTool.Pages;
using Xunit;

namespace Piipan.QueryTool.Tests
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
            var model = new SignedOutModel(serviceProviderMock(states: new string[] { "*" }));
            model.PageContext.HttpContext = contextMock();
            var (page, output) = await renderer.RenderView(PageName, model);

            // assert the list page link is shown when the user is a national user
            Assert.Contains("<a href=\"/list", output);
        }

        [Fact]
        public async Task ListPageLink_IsNotShown_ForEAUser()
        {
            // arrange
            var renderer = GetViewRenderer();

            // act
            var model = new SignedOutModel(serviceProviderMock(states: new string[] { "EA" }));
            model.PageContext.HttpContext = contextMock();
            var (page, output) = await renderer.RenderView(PageName, model);

            // assert the list page link is shown when the user is a national user
            Assert.DoesNotContain("<a href=\"/list", output);
        }
    }
}
