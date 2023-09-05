using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Piipan.QueryTool.Pages
{
    /// <summary>
    /// This is the page that gets shown when the user gets any 500 error.
    /// We are allowing anonymous here just in case they got this error trying to authenticate.
    /// </summary>
    [AllowAnonymous]
    public class ServerErrorModel : PageModel
    {
        public string HelpDeskEmail = "";
        public RenderMode RenderMode { get; set; } = RenderMode.Static;

        /// <summary>
        /// Creates the ServerErrorModel page and retrieve the HelpDeskEmail.
        /// </summary>
        public ServerErrorModel(IServiceProvider serviceProvider)
        {
            HelpDeskEmail = serviceProvider.GetRequiredService<IConfiguration>()["HelpDeskEmail"];
        }

        /// <summary>
        /// Returns the ServerError page on get.
        /// </summary>
        public IActionResult OnGet()
        {
            return new PageResult { StatusCode = 500 };
        }
    }
}
