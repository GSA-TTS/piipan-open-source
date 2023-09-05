using System;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Piipan.Shared.Web;

namespace Piipan.QueryTool.Pages
{
    public abstract class BasePageModel : PageModel
    {
        public IWebAppDataServiceProvider WebAppDataServiceProvider { get; set; }

        public BasePageModel(IServiceProvider serviceProvider)
        {
            WebAppDataServiceProvider = serviceProvider.GetRequiredService<IWebAppDataServiceProvider>();
        }
    }
}