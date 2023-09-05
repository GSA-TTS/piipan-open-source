using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Piipan.QueryTool.Pages;
using Xunit;

namespace Piipan.QueryTool.Tests
{
    /// <summary>
    /// Tests for the Server Error page. This is the page that gets shown when the user gets any 500 error.
    /// </summary>
    public class ServerErrorPageTests : BasePageTest
    {
        [Fact]
        public async Task TestBeforeOnGet()
        {
            // arrange
            var mockServiceProvider = serviceProviderMock();
            var pageModel = new ServerErrorModel(mockServiceProvider);

            var renderer = GetViewRenderer();

            // act
            var (page, output) = await renderer.RenderPage("/Pages/ServerError.cshtml", pageModel);


            // assert
            Assert.Equal("Something Unexpected Happened", page.ViewContext.ViewData["Title"]);
            Assert.Contains("<h1>Something Unexpected Happened</h1>", output);
        }

        [Fact]
        public void TestNotAuthorizedStatusCode()
        {
            // arrange
            var mockServiceProvider = serviceProviderMock();
            var pageModel = new ServerErrorModel(mockServiceProvider);

            // act
            var pageResult = pageModel.OnGet();
            // assert
            Assert.IsType<PageResult>(pageResult);
            Assert.Equal(500, (pageResult as PageResult).StatusCode);
        }
    }
}