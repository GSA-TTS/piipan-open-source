using Piipan.Maintenance.Pages;

namespace Piipan.Maintenance.Tests
{
    public class MaintenancePageTests : BasePageTest
    {
        [Fact]
        public async Task TestBeforeOnGet()
        {
            // arrange
            var pageModel = new IndexModel();

            var renderer = GetViewRenderer();

            // act
            var (page, output) = await renderer.RenderPage("/Pages/Index.cshtml", pageModel);


            // assert
            Assert.Equal("System down for maintenance", page.ViewContext.ViewData["Title"]);
            Assert.Contains("<p class=\"large-text\">NAC is down for scheduled maintenance but we'll be back up and running soon.</p>", output);
            Assert.Contains("<h1>We're Making Improvements!</h1>", output);
        }
    }
}