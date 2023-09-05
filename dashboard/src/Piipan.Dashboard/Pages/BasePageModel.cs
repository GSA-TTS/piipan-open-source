using System;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Piipan.Shared.Web;

namespace Piipan.Dashboard.Pages
{
    public abstract class BasePageModel : PageModel
    {
        public IWebAppDataServiceProvider WebAppDataServiceProvider { get; set; }

        protected BasePageModel(IServiceProvider serviceProvider)
        {
            WebAppDataServiceProvider = serviceProvider.GetRequiredService<IWebAppDataServiceProvider>();
        }
    }
}