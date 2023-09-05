using System.Threading.Tasks;
using Piipan.QueryTool.Pages;
using Xunit;

namespace Piipan.QueryTool.Tests
{
    public class SignedOutTest : BasePageTest
    {
        private const string PageName = "/Pages/SignedOut.cshtml";

        [Fact]
        public void TestBeforeOnGet()
        {
            // arrange
            var mockServiceProvider = serviceProviderMock();
            var pageModel = new SignedOutModel(mockServiceProvider);

            // act

            // assert
            Assert.Equal("noreply@tts.test", pageModel.WebAppDataServiceProvider.Email);
        }

        [Fact]
        public async Task TestMessageOnGet()
        {
            // arrange
            var mockServiceProvider = serviceProviderMock();
            var pageModel = new SignedOutModel(mockServiceProvider);
            var renderer = GetViewRenderer();

            // act
            var (page, output) = await renderer.RenderPage(PageName, pageModel);

            // assert
            Assert.Equal("User Signed Out", page.ViewContext.ViewData["Title"]);
            Assert.Contains("<h1>You are signed out.</h1>", output);
        }
    }
}