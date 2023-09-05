using System;
using System.Diagnostics.CodeAnalysis;
using MailKit.Net.Smtp;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Piipan.Notifications.Core.Extensions;
using Piipan.Notifications.Core.Services;

[assembly: FunctionsStartup(typeof(Piipan.Notifications.Func.Api.Startup))]

namespace Piipan.Notifications.Func.Api
{
    [ExcludeFromCodeCoverage]
    public class Startup : FunctionsStartup
    {
        public const string SmtpConnection = "SmtpServer";
        public const string EnableEmails = "EnableEmails";
        public const string SmtpCcEmail = "SmtpCcEmail";
        public const string SmtpBccEmail = "SmtpBccEmail";
        public const string SmtpFromEmail = "SmtpFromEmail";

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();
            builder.Services.AddTransient<EmailDelivery>(x =>
            {
                return new EmailDelivery()
                {
                    Enabled = bool.Parse(Environment.GetEnvironmentVariable(EnableEmails)),
                    EmailCc = Environment.GetEnvironmentVariable(SmtpCcEmail),
                    EmailBcc = Environment.GetEnvironmentVariable(SmtpBccEmail),
                    EmailFrom = Environment.GetEnvironmentVariable(SmtpFromEmail)
                };
            });
            builder.Services.AddTransient<IMessageBuilder, MessageBuilder>();
            builder.Services.AddTransient<IMailService, MailService>();
            builder.Services.AddTransient<IUsdaImageRetriever, UsdaImageRetriever>();
            builder.Services.AddTransient<ISmtpClient>(x =>
            {
                return new SmtpClient();
            });
          
        }
    }
}