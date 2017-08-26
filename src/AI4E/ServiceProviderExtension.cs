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
        [Obsolete("Use 'UseMessaging()'")]
        public static /*IEnumerable<IHandlerRegistration>*/void UseQueryHandlers(this IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            //var result = new List<IHandlerRegistration>();

            var assembly = Assembly.GetCallingAssembly();
            var queryDispatcher = serviceProvider.GetRequiredService<IQueryDispatcher>();

            foreach (var type in assembly.GetTypes().Where(p => p.GetTypeInfo().IsClass && !p.GetTypeInfo().IsAbstract || p.GetTypeInfo().IsValueType && !p.GetTypeInfo().IsEnum))
            {
                var ifaces = type.GetTypeInfo().GetInterfaces().Where(p => p.GetTypeInfo().IsGenericType && p.GetGenericTypeDefinition() == typeof(IQueryHandler<>));

                if (!ifaces.Any())
                {
                    continue;
                }

                var factory = Activator.CreateInstance(typeof(DefaultHandlerFactory<>).MakeGenericType(type));
                foreach (var iface in ifaces)
                {
                    var queryType = iface.GetTypeInfo().GetGenericArguments()[0];
                    var resultType = iface.GetTypeInfo().GetGenericArguments()[1];

                    var registerMethodDefinition = typeof(IQueryDispatcher).GetTypeInfo().GetMethods().First(p => p.Name == nameof(ICommandDispatcher.RegisterAsync));
                    var registerMethod = registerMethodDefinition.MakeGenericMethod(queryType, resultType);
                    Debug.Assert(registerMethod != null);

                    //result.Add(((dynamic)
                    registerMethod.Invoke(queryDispatcher, new[] { factory });
                    //).Result); // TODO
                }
            }

            //return result;
        }

        [Obsolete("Use 'UseMessaging()'")]
        public static /*IEnumerable<IHandlerRegistration>*/void UseEventHandlers(this IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            //var result = new List<IHandlerRegistration>();
            var assembly = Assembly.GetCallingAssembly();
            var eventDispatcher = serviceProvider.GetRequiredService<IEventDispatcher>();

            foreach (var type in assembly.GetTypes().Where(p => p.GetTypeInfo().IsClass && !p.GetTypeInfo().IsAbstract || p.GetTypeInfo().IsValueType && !p.GetTypeInfo().IsEnum))
            {
                var ifaces = type.GetTypeInfo().GetInterfaces().Where(p => p.GetTypeInfo().IsGenericType && p.GetGenericTypeDefinition() == typeof(IEventHandler<>));

                if (!ifaces.Any())
                {
                    continue;
                }

                var factory = Activator.CreateInstance(typeof(DefaultHandlerFactory<>).MakeGenericType(type));
                foreach (var iface in ifaces)
                {
                    var eventType = iface.GetTypeInfo().GetGenericArguments().First();

                    var registerMethodDefinition = typeof(IEventDispatcher).GetTypeInfo().GetMethods().First(p => p.Name == nameof(IEventDispatcher.RegisterAsync));
                    var registerMethod = registerMethodDefinition.MakeGenericMethod(eventType);
                    Debug.Assert(registerMethod != null);

                    //result.Add(((dynamic)
                    registerMethod.Invoke(eventDispatcher, new[] { factory });
                    //).Result); // TODO
                }
            }

            //return result;
        }

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
