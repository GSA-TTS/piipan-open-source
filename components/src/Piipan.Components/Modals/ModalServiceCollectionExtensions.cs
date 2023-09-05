using Microsoft.Extensions.DependencyInjection;

namespace Piipan.Components.Modals
{
    public static class ModalServiceCollectionExtensions
    {
        /// <summary>Adds services for the Modal Manager.</summary>
        /// <param name="services">The service collection to register with.</param>
        public static void AddModalManager(this IServiceCollection services)
        {
            services.AddSingleton<IModalManager, ModalManager>();
        }
    }
}
