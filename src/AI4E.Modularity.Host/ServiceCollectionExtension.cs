using System;
using Microsoft.Extensions.DependencyInjection;

namespace AI4E.Modularity
{
    public static class ServiceCollectionExtension
    {
        public static void AddModularHost(this IServiceCollection serviceCollection, string workingDirectory)
        {
            if (serviceCollection == null)
                throw new ArgumentNullException(nameof(serviceCollection));

            serviceCollection.AddSingleton<IModuleManager, ModuleManager>(serviceProvider => new ModuleManager(workingDirectory, serviceProvider));
            //serviceCollection.AddSingleton<ModuleHostConfiguration>();
        }

        public static void UseModularHost(this IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            var moduleHost = serviceProvider.GetRequiredService<IModuleManager>();


        }
    }
}
