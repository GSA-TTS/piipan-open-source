using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace Piipan.Shared.Tests.ViewRendering
{
    public class ViewRenderer
    {
        private IRazorViewEngine _viewEngine;
        private ITempDataProvider _tempDataProvider;
        private IServiceProvider _serviceProvider;
        private IRazorPageActivator _activator;

        public ViewRenderer(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, IServiceProvider serviceProvider,
            IRazorPageActivator activator)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
            _activator = activator;
        }

        public async Task<(IView, string)> RenderView<TModel>(string name, TModel model)
        {
            var actionContext = GetActionContext();

            var viewEngineResult = _viewEngine.GetView("", name, false);

            if (!viewEngineResult.Success)
            {
                throw new InvalidOperationException(string.Format("Couldn't find view '{0}'", name));
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
                await viewEngineResult.View.RenderAsync(viewContext);

                return (viewEngineResult.View, output.ToString());
            }

        }

        public async Task<(IView, string)> RenderViewWithNoModel(string name)
        {
            var actionContext = GetActionContext();

            var viewEngineResult = _viewEngine.GetView("", name, false);

            if (!viewEngineResult.Success)
            {
                throw new InvalidOperationException(string.Format("Couldn't find view '{0}'", name));
            }

            using (var output = new StringWriter())
            {
                var viewContext = new ViewContext(
                    actionContext,
                    viewEngineResult.View,
                    new ViewDataDictionary(
                        metadataProvider: new EmptyModelMetadataProvider(),
                        modelState: new ModelStateDictionary()),
                    new TempDataDictionary(
                        actionContext.HttpContext,
                        _tempDataProvider),
                    output,
                    new HtmlHelperOptions());
                await viewEngineResult.View.RenderAsync(viewContext);

                return (viewEngineResult.View, output.ToString());
            }

        }

        public async Task<(Page, string)> RenderPage<TModel>(string name, TModel model) where TModel : PageModel
        {
            var actionContext = GetActionContext();

            var viewEngineResult = _viewEngine.GetPage("", name);

            if (viewEngineResult.Page == null)
            {
                throw new InvalidOperationException(string.Format("Couldn't find view '{0}'", name));
            }

            var view = new RazorView(_viewEngine,
                _activator,
                new List<IRazorPage>(),
                viewEngineResult.Page,
                HtmlEncoder.Default,
                new DiagnosticListener("ViewRenderService"));

            using (var output = new StringWriter())
            {
                var viewContext = new ViewContext(
                    actionContext,
                    view,
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
                var page = ((Page)viewEngineResult.Page);

                page.PageContext = new PageContext
                {
                    ViewData = viewContext.ViewData,
                    HttpContext = actionContext.HttpContext,
                };

                page.ViewContext = viewContext;
                _activator.Activate(page, viewContext);

                await page.ExecuteAsync();


                return (page, output.ToString());
            }

        }

        private ActionContext GetActionContext()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = _serviceProvider;
            return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        }

        /// <summary>
        /// Sets up the server fixture, view engines, razor page activator, etc.
        /// This will be used to render CSHTML pages and test them.
        /// </summary>
        /// <returns></returns>
        public static ViewRenderer SetupRenderingApi(string solutionContentRootPath, string assembly)
        {
            var server = new PageTestServerFixture(solutionContentRootPath, assembly);

            var serviceProvider = server.GetRequiredService<IServiceProvider>();
            var viewEngine = server.GetRequiredService<IRazorViewEngine>();
            var tempDataProvider = server.GetRequiredService<ITempDataProvider>();
            var razorPageActivator = server.GetRequiredService<IRazorPageActivator>();

            var viewRenderer = new ViewRenderer(viewEngine, tempDataProvider, serviceProvider, razorPageActivator);
            return viewRenderer;
        }
    }

}
