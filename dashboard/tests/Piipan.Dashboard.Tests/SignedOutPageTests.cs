using System.Threading.Tasks;
using Piipan.Dashboard.Pages;
using Xunit;

namespace Piipan.Dashboard.Tests
{
    public class SignedOutPageTests : BasePageTest
    {
        [Fact]
        public void Construct_CallsBasePageConstructor()
        {
            // Arrange
            var serviceProvider = serviceProviderMock();

            // Act
            var page = new SignedOutModel(serviceProvider);

            // Assert
            Assert.Equal("noreply@tts.test", page.WebAppDataServiceProvider.Email);
        }

        [Fact]
        public async Task TestBeforeOnGet()
        {
            // arrange
            var mockServiceProvider = serviceProviderMock();
            var pageModel = new SignedOutModel(mockServiceProvider);

            var renderer = GetViewRenderer();

            // act
            var (page, output) = await renderer.RenderPage("/Pages/SignedOut.cshtml", pageModel);


            // assert
            Assert.Equal("Signed Out", page.ViewContext.ViewData["Title"]);
            Assert.Contains("<h1>You are signed out.</h1>", output);
        }
    }
}