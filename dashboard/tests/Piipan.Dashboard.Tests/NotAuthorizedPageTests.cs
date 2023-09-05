using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Piipan.Dashboard.Pages;
using Xunit;

namespace Piipan.Dashboard.Tests
{
    /// <summary>
    /// Tests for the Not Authorized page. This is the page that gets shown when the user gets any 403 error.
    /// </summary>
    public class NotAuthorizedPageTests : BasePageTest
    {
        [Fact]
        public async Task TestBeforeOnGet()
        {
            // arrange
            var mockServiceProvider = serviceProviderMock();
            var pageModel = new NotAuthorizedModel(mockServiceProvider);

            var renderer = GetViewRenderer();

            // act
            var (page, output) = await renderer.RenderPage("/Pages/NotAuthorized.cshtml", pageModel);


            // assert
            Assert.Equal("Not Authorized", page.ViewContext.ViewData["Title"]);
            Assert.Contains("<h1>Access Denied</h1>", output);
        }

        [Fact]
        public void TestNotAuthorizedStatusCode()
        {
            // arrange
            var mockServiceProvider = serviceProviderMock();
            var pageModel = new NotAuthorizedModel(mockServiceProvider);

            // act
            var pageResult = pageModel.OnGet();
            // assert
            Assert.IsType<PageResult>(pageResult);
            Assert.Equal(403, (pageResult as PageResult).StatusCode);
        }
    }
}