using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace Piipan.Notification.Common
{
    public class ViewRenderService : IViewRenderService
    {
        private IRazorViewEngine _viewEngine;
        private ITempDataProvider _tempDataProvider;
        private IServiceProvider _serviceProvider;

        public ViewRenderService(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, IServiceProvider serviceProvider)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> GenerateMessageContent<TModel>(string name, TModel model)
        {
            var actionContext = GetActionContext();
            var viewPath = "~/Templates/";

            var viewEngineResult = _viewEngine.GetView("", viewPath + name, false);

            if (!viewEngineResult.Success)
            {
                throw new InvalidOperationException(string.Format("Email template '{0}' not found.", name));
            }

            using (var output = new StringWriter())
            {
                var viewContext = new ViewContext(
                    actionContext,
                    viewEngineResult.View,
                    new ViewDataDictionary<TModel>(
                        metadataProvider: new EmptyModelMetadataProvider(),
                        modelState: new ModelStateDictionary())
                    {
                        Model = model,
                    },
                    new TempDataDictionary(
                        actionContext.HttpContext,
                        _tempDataProvider),
                    output,
                    new HtmlHelperOptions());
                try
                {
                    await viewEngineResult.View.RenderAsync(viewContext);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(ex.Message);
                }

                return output.ToString();
            }
        }

        private ActionContext GetActionContext()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = _serviceProvider;
            return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        }
    }
}