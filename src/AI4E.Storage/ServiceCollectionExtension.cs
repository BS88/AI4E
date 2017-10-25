using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace AI4E.Storage
{
    public static class ServiceCollectionExtension
    {
        public static IEventSourcingBuilder AddEventSourcing(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            // Configure necessary application parts
            var partManager = services.GetApplicationPartManager();
            partManager.ConfigureEventSourcingFeatureProvider();
            services.TryAddSingleton(partManager);

            // Configure services
            // TODO

            return new EventSourcingBuilder(services);
        }

        public static IEventSourcingBuilder AddEventSourcing(this IServiceCollection services, Action<EventSourcingOptions> configuration)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var builder = AddEventSourcing(services);
            builder.Configure(configuration);
            return builder;
        }

        private static void ConfigureEventSourcingFeatureProvider(this ApplicationPartManager partManager)
        {
            if (!partManager.FeatureProviders.OfType<EventReplayerFeatureProvider>().Any())
            {
                partManager.FeatureProviders.Add(new EventReplayerFeatureProvider());
            }
        }

        private static ApplicationPartManager GetApplicationPartManager(this IServiceCollection services)
        {
            var manager = services.GetService<ApplicationPartManager>();
            if (manager == null)
            {
                manager = new ApplicationPartManager();
                var parts = DefaultAssemblyPartDiscoveryProvider.DiscoverAssemblyParts(Assembly.GetEntryAssembly().FullName);
                foreach (var part in parts)
                {
                    manager.ApplicationParts.Add(part);
                }
            }

            return manager;
        }

        private static T GetService<T>(this IServiceCollection services)
        {
            return (T)services
                .LastOrDefault(d => d.ServiceType == typeof(T))
                ?.ImplementationInstance;
        }
    }
}
