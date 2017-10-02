using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AI4E.Modularity
{
    public static partial class ServiceCollectionExtension
    {
        public static IModularityBuilder AddModularity(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            // This is added for the implementation to know that the required services were registered properly.
            services.AddSingleton<ModularityMarkerService>();

            services.TryAddSingleton<IModuleHost, ModuleHost>();
            services.TryAddSingleton<IModuleInstaller, ModuleInstaller>();
            services.TryAddSingleton<IModuleSupervision, ModuleSupervision>();

            services.TryAddScoped<IModuleManager, ModuleManager>();
            services.TryAddScoped<IModuleSourceManager, ModuleSourceManager>();

            return new ModularityBuilder(services);
        }

        public static IModularityBuilder AddModularity(this IServiceCollection services, Action<ModularityOptions> configuration)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var result = services.AddModularity();

            result.Configure(configuration);

            return result;
        }
    }

    internal class ModularityMarkerService { }
}
