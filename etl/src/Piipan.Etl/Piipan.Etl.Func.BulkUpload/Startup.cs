using System;
using FluentValidation;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.DependencyInjection;
using Piipan.Etl.Func.BulkUpload.Models;
using Piipan.Etl.Func.BulkUpload.Parsers;
using Piipan.Etl.Func.BulkUpload.Validators;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Extensions;
using Piipan.Shared.Cryptography.Extensions;
using Piipan.Shared.Database;
using Piipan.Shared.Deidentification;

[assembly: FunctionsStartup(typeof(Piipan.Etl.Func.BulkUpload.Startup))]

namespace Piipan.Etl.Func.BulkUpload
{
    public class Startup : FunctionsStartup
    {
        public const string ParticipantsDatabaseConnectionString = "ParticipantsDatabaseConnectionString";
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();

            builder.Services.AddSingleton<IDatabaseManager<ParticipantsDbManager>>((s) => 
                new SingleDatabaseManager<ParticipantsDbManager>(Environment.GetEnvironmentVariable(ParticipantsDatabaseConnectionString)));
            
        
            builder.Services.AddTransient<IValidator<ParticipantCsv>, ParticipantValidator>();
            builder.Services.AddTransient<IParticipantStreamParser, ParticipantCsvStreamParser>();
            builder.Services.AddTransient<IBlobClientStream, BlobClientStream>();
            builder.Services.AddTransient<ICsvValidator, CsvValidator>();
            builder.Services.AddTransient<IRedactionService, RedactionService>();

            builder.Services.RegisterParticipantsServices();

            builder.Services.RegisterKeyVaultClientServices();
        }
    }
}
