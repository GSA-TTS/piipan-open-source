using Microsoft.Extensions.DependencyInjection;

namespace Piipan.Components.Routing
{
    public static class NavigationManagerExtensions
    {
        /// <summary>Adds services for the lockable navigation manager.</summary>
        /// <param name="services">The service collection to register with.</param>
        public static void AddPiipanNavigationManager(this IServiceCollection services)
        {
            services.AddSingleton<PiipanNavigationManager>();
        }
    }
}
