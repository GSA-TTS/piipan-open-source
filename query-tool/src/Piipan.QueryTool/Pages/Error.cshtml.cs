using System;
using Microsoft.Extensions.Logging;

namespace Piipan.QueryTool.Pages
{
    public class ErrorModel : BasePageModel
    {
        private readonly ILogger<ErrorModel> _logger;
        public string Message = "";

        public ErrorModel(ILogger<ErrorModel> logger,
            IServiceProvider serviceProvider)
                          : base(serviceProvider)
        {
            _logger = logger;
        }

        public void OnGet(string message)
        {
            _logger.LogError($"Arrived at error page with message {message}");
            if (message != null)
            {
                Message = message;
            }
        }
    }
}
