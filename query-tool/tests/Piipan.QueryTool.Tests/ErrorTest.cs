using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Piipan.QueryTool.Pages;
using Xunit;

namespace Piipan.QueryTool.Tests
{
    public class ErrorPageTests : BasePageTest
    {
        private const string PageName = "/Pages/Error.cshtml";

        [Fact]
        public void TestBeforeOnGet()
        {
            // arrange
            var mockServiceProvider = serviceProviderMock();
            var pageModel = new ErrorModel(new NullLogger<ErrorModel>(), mockServiceProvider);

            // act

            // assert
            Assert.Equal("noreply@tts.test", pageModel.WebAppDataServiceProvider.Email);
        }

        [Fact]
        public async Task TestMessageOnGet()
        {
            // arrange
            var mockServiceProvider = serviceProviderMock();
            var pageModel = new ErrorModel(new NullLogger<ErrorModel>(), mockServiceProvider);
            var renderer = GetViewRenderer();

            // act
            string message = "test message";
            pageModel.OnGet(message);
            var (page, output) = await renderer.RenderPage(PageName, pageModel);

            // assert
            Assert.Equal(message, pageModel.Message);
            Assert.Equal("Error", page.ViewContext.ViewData["Title"]);
            Assert.Contains("<h2 class=\"usa-alert__heading\">test message</h2>", output);
        }
    }
}