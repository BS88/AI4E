using System;
using System.Diagnostics;
using AI4E.Integration;
using Microsoft.Extensions.DependencyInjection;

namespace AI4E
{
    public static class ServiceCollectionExtension
    {
        [Obsolete("Use 'AddMessaging()'.")]
        public static void AddInfrastructure(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<CommandDispatcher>();
            serviceCollection.AddSingleton<QueryDispatcher>();
            serviceCollection.AddSingleton<EventDispatcher>();

            serviceCollection.AddSingleton<ICommandDispatcher, CommandDispatcher>(provider => provider.GetRequiredService<CommandDispatcher>());
            serviceCollection.AddSingleton<IQueryDispatcher, QueryDispatcher>(provider => provider.GetRequiredService<QueryDispatcher>());
            serviceCollection.AddSingleton<IEventDispatcher, EventDispatcher>(provider => provider.GetRequiredService<EventDispatcher>());

            serviceCollection.AddSingleton<INonGenericCommandDispatcher, CommandDispatcher>(provider => provider.GetRequiredService<CommandDispatcher>());
            serviceCollection.AddSingleton<INonGenericQueryDispatcher, QueryDispatcher>(provider => provider.GetRequiredService<QueryDispatcher>());
            serviceCollection.AddSingleton<INonGenericEventDispatcher, EventDispatcher>(provider => provider.GetRequiredService<EventDispatcher>());
        }

        public static IMessagingBuilder AddMessaging(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSingleton<CommandDispatcher>();
            services.AddSingleton<QueryDispatcher>();
            services.AddSingleton<EventDispatcher>();

            services.AddSingleton<ICommandDispatcher, CommandDispatcher>(provider => provider.GetRequiredService<CommandDispatcher>());
            services.AddSingleton<IQueryDispatcher, QueryDispatcher>(provider => provider.GetRequiredService<QueryDispatcher>());
            services.AddSingleton<IEventDispatcher, EventDispatcher>(provider => provider.GetRequiredService<EventDispatcher>());

            services.AddSingleton<INonGenericCommandDispatcher, CommandDispatcher>(provider => provider.GetRequiredService<CommandDispatcher>());
            services.AddSingleton<INonGenericQueryDispatcher, QueryDispatcher>(provider => provider.GetRequiredService<QueryDispatcher>());
            services.AddSingleton<INonGenericEventDispatcher, EventDispatcher>(provider => provider.GetRequiredService<EventDispatcher>());

            return new MessagingBuilder(services);
        }

        public static IMessagingBuilder AddMessaging(this IServiceCollection services, Action<MessagingOptions> configuration)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var builder = services.AddMessaging();
            builder.Services.Configure(configuration);
            return builder;
        }
    }

    public class MessagingOptions
    {

    }

    public interface IMessagingBuilder
    {
        IServiceCollection Services { get; }
    }

    internal class MessagingBuilder : IMessagingBuilder
    {
        private readonly IServiceCollection _services;

        public MessagingBuilder(IServiceCollection services)
        {
            Debug.Assert(services != null);

            _services = services;
        }

        public IServiceCollection Services => _services;
    }

    public static class MessagingBuilderExtension
    {
        public static IMessagingBuilder Configure(this IMessagingBuilder messagingBuilder, Action<MessagingOptions> configuration)
        {
            if (messagingBuilder == null)
                throw new ArgumentNullException(nameof(messagingBuilder));

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            messagingBuilder.Services.Configure(configuration);

            return messagingBuilder;
        }
    }
}
