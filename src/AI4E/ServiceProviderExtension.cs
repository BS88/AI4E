using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using AI4E.Integration;
using AI4E.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace AI4E
{
    [Obsolete]
    public static class ServiceProviderExtension
    {
        [Obsolete]
        public static /*IEnumerable<IHandlerRegistration>*/ void UseEventReplayer<TEventBase, TEntityBase>(this IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            //var result = new List<IHandlerRegistration>();
            var assembly = Assembly.GetCallingAssembly();
            var eventReplayerRegistry = serviceProvider.GetRequiredService<IEventReplayerRegistry<Guid, TEventBase, TEntityBase>>();

            foreach (var type in assembly.GetTypes().Where(p => p.GetTypeInfo().IsClass && !p.GetTypeInfo().IsAbstract || p.GetTypeInfo().IsValueType && !p.GetTypeInfo().IsEnum))
            {
                var ifaces = type.GetTypeInfo().GetInterfaces().Where(p => p.GetTypeInfo().IsGenericType && p.GetGenericTypeDefinition() == typeof(IEventReplayer<,,,,>));

                if (!ifaces.Any())
                {
                    continue;
                }

                var factory = Activator.CreateInstance(typeof(DefaultHandlerFactory<>).MakeGenericType(type));

                foreach (var iface in ifaces)
                {
                    var eventType = iface.GetTypeInfo().GetGenericArguments()[3];
                    var entityType = iface.GetTypeInfo().GetGenericArguments()[4];

                    var registerMethodDefinition = typeof(IEventReplayerRegistry<Guid, TEventBase, TEntityBase>)
                                                   .GetTypeInfo()
                                                   .GetMethods()
                                                   .First(p => p.Name == nameof(IEventReplayerRegistry<Guid, TEventBase, TEntityBase>.RegisterAsync));

                    var registerMethod = registerMethodDefinition.MakeGenericMethod(eventType, entityType);

                    //result.Add(((dynamic)
                    registerMethod.Invoke(eventReplayerRegistry, new[] { factory });
                    //).Result); // TODO
                }
            }

            //return result;
        }
    }
}
