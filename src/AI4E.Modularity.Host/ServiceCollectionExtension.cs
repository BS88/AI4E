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

            services.AddOptions();

            // This is added for the implementation to know that the required services were registered properly.
            services.AddSingleton<ModularityMarkerService>();

            // These services are running only once and therefore are registered as singleton instances.
            // The services are not intended to be used directly but are required for internal use.
            services.TryAddSingleton<IModuleHost, ModuleHost>();
            services.TryAddSingleton<IModuleInstaller, ModuleInstaller>();
            services.TryAddSingleton<IModuleSupervision, ModuleSupervision>();

            // These services are the public api for the modular host.
            services.TryAddScoped<IModuleManager, ModuleManager>();

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

        private sealed class ModularityBuilder : IModularityBuilder
        {
            public ModularityBuilder(IServiceCollection services)
            {
                Services = services;
            }

            public IServiceCollection Services { get; }
        }
    }

    internal class ModularityMarkerService { }
}
