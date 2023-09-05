using Microsoft.Extensions.DependencyInjection;
using Piipan.Metrics.Api;
using Piipan.Metrics.Core.Builders;
using Piipan.Metrics.Core.DataAccessObjects;
using Piipan.Metrics.Core.Services;

namespace Piipan.Metrics.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterCoreServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IParticipantUploadDao, ParticipantUploadDao>();
            serviceCollection.AddTransient<IParticipantSearchDao, ParticipantSearchDao>();
            serviceCollection.AddTransient<IParticipantMatchDao, ParticipantMatchDao>();
            serviceCollection.AddTransient<IParticipantUploadReaderApi, ParticipantUploadService>();
            serviceCollection.AddTransient<IParticipantUploadWriterApi, ParticipantUploadService>();
            serviceCollection.AddTransient<IParticipantSearchWriterApi, ParticipantSearchService>();
            serviceCollection.AddTransient<IParticipantMatchWriterApi, ParticipantMatchService>();
            serviceCollection.AddTransient<IMetaBuilder, MetaBuilder>();
        }
    }
}