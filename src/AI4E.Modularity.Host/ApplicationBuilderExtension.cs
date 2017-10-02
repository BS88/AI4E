using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AI4E.Modularity
{
    public static class ApplicationBuilderExtension
    {
        public static void UseModularity(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
                throw new ArgumentNullException(nameof(applicationBuilder));

            var serviceProvider = applicationBuilder.ApplicationServices;

            if (serviceProvider.GetService<ModularityMarkerService>() == null)
            {
                throw new InvalidOperationException("Cannot use the modular host without adding the modularity services.");
            }

            // Initialize the module-host.

            var host = serviceProvider.GetRequiredService<IModuleHost>();
        }
    }
}
