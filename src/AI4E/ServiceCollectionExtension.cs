using AI4E.Integration;
using Microsoft.Extensions.DependencyInjection;

namespace AI4E
{
    public static class ServiceCollectionExtension
    {
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
    }
}
