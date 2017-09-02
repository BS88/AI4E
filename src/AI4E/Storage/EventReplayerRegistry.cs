/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        EventReplayerRegistry.cs 
 * Types:           AI4E.Storage.EventReplayerRegistry'3
 *                  AI4E.Storage.EventReplayerRegistry'3.EventReplayerInvoker'2
 *                  AI4E.Storage.EventReplayerRegistry'3.EventReplayerFactory'2
 *                  AI4E.Storage.EventReplayerRegistry'3.Resolver
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   11.05.2017 
 * Status:          Ready
 * --------------------------------------------------------------------------------------------------------------------
 */

/* License
 * --------------------------------------------------------------------------------------------------------------------
 * This file is part of the AI4E distribution.
 *   (https://gitlab.com/EnterpriseApplicationEquipment/AI4E)
 * Copyright (c) 2017 Andreas Trütschel.
 * 
 * AI4E is free software: you can redistribute it and/or modify  
 * it under the terms of the GNU Lesser General Public License as   
 * published by the Free Software Foundation, version 3.
 *
 * AI4E is distributed in the hope that it will be useful, but 
 * WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU 
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 * --------------------------------------------------------------------------------------------------------------------
 */

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AI4E.Storage
{
    public sealed class EventReplayerRegistry<TId, TEventBase, TEntityBase> :
        IEventReplayerRegistry<TId, TEventBase, TEntityBase>,
        IContextualProvider<IEventReplayerResolver<TId, TEventBase, TEntityBase>>
        where TId : struct, IEquatable<TId>
    {
        private volatile ImmutableDictionary<Type, ImmutableDictionary<Type, IAsyncSingleHandlerRegistry<IEventReplayer<TId, TEventBase, TEntityBase>>>> _replayer
            = ImmutableDictionary<Type, ImmutableDictionary<Type, IAsyncSingleHandlerRegistry<IEventReplayer<TId, TEventBase, TEntityBase>>>>.Empty;

        public EventReplayerRegistry() { }

        public async Task<IHandlerRegistration> RegisterAsync<TEvent, TEntity>(IContextualProvider<IEventReplayer<TId, TEventBase, TEntityBase, TEvent, TEntity>> eventReplayerFactory)
            where TEvent : TEventBase
            where TEntity : TEntityBase
        {
            if (eventReplayerFactory == null)
                throw new ArgumentNullException(nameof(eventReplayerFactory));

            IAsyncSingleHandlerRegistry<IEventReplayer<TId, TEventBase, TEntityBase>> handlerRegistry;

            ImmutableDictionary<Type, ImmutableDictionary<Type, IAsyncSingleHandlerRegistry<IEventReplayer<TId, TEventBase, TEntityBase>>>> current = _replayer,
                                                                                                                                            start,
                                                                                                                                            desired;
            do
            {
                start = current;

                if (start.TryGetValue(typeof(TEvent), out var perEventRegistry) &&
                    perEventRegistry.TryGetValue(typeof(TEntity), out handlerRegistry))
                {
                    break;
                }

                desired = start.Remove(typeof(TEvent));
                handlerRegistry = new AsyncSingleHandlerRegistry<IEventReplayer<TId, TEventBase, TEntityBase>>();
                perEventRegistry = (perEventRegistry ?? ImmutableDictionary<Type, IAsyncSingleHandlerRegistry<IEventReplayer<TId, TEventBase, TEntityBase>>>.Empty)
                                   .Add(typeof(TEntity), handlerRegistry);
                desired = desired.Add(typeof(TEvent), perEventRegistry);
                current = Interlocked.CompareExchange(ref _replayer, desired, start);
            }
            while (current != start);

            var untypedFactory = new EventReplayerFactory<TEvent, TEntity>(eventReplayerFactory);
            return await HandlerRegistration.CreateRegistrationAsync(handlerRegistry, untypedFactory);
        }

        public IEventReplayerResolver<TId, TEventBase, TEntityBase> GetResolver(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            return new Resolver(this, serviceProvider);
        }

        IEventReplayerResolver<TId, TEventBase, TEntityBase> IContextualProvider<IEventReplayerResolver<TId, TEventBase, TEntityBase>>.GetInstance(IServiceProvider serviceProvider)
        {
            return GetResolver(serviceProvider);
        }

        private sealed class EventReplayerInvoker<TEvent, TEntity> : IEventReplayer<TId, TEventBase, TEntityBase>
            where TEvent : TEventBase
            where TEntity : TEntityBase
        {
            private readonly IEventReplayer<TId, TEventBase, TEntityBase, TEvent, TEntity> _eventReplayer;

            public EventReplayerInvoker(IEventReplayer<TId, TEventBase, TEntityBase, TEvent, TEntity> eventReplayer)
            {
                if (eventReplayer == null)
                    throw new ArgumentNullException(nameof(eventReplayer));

                _eventReplayer = eventReplayer;
            }

            public async ValueTask<TEntityBase> ReplayAsync(TEventBase evt, TEntityBase entity)
            {
                if (evt == null)
                    throw new ArgumentNullException(nameof(evt));

                if (!(evt is TEvent typedEvent))
                    throw new ArgumentException("The specified event is not of the expected type.");

                if (!(entity is TEntity typedEntity))
                {
                    if (entity != null)
                        throw new ArgumentException("The specified entity is not of the specified type.");

                    typedEntity = default;
                }

                return await _eventReplayer.ReplayAsync(typedEvent, typedEntity);
            }

            public Type EntityType => typeof(TEntity);

            public Type EventType => typeof(TEvent);
        }

        private sealed class EventReplayerFactory<TEvent, TEntity> : IContextualProvider<IEventReplayer<TId, TEventBase, TEntityBase>>
            where TEvent : TEventBase
            where TEntity : TEntityBase
        {
            private readonly IContextualProvider<IEventReplayer<TId, TEventBase, TEntityBase, TEvent, TEntity>> _eventReplayerFactory;

            public EventReplayerFactory(IContextualProvider<IEventReplayer<TId, TEventBase, TEntityBase, TEvent, TEntity>> eventReplayerFactory)
            {
                if (eventReplayerFactory == null)
                    throw new ArgumentNullException(nameof(eventReplayerFactory));

                _eventReplayerFactory = eventReplayerFactory;
            }

            public IEventReplayer<TId, TEventBase, TEntityBase> GetInstance(IServiceProvider serviceProvider)
            {
                if (serviceProvider == null)
                    throw new ArgumentNullException(nameof(serviceProvider));

                // The service-provider must be scoped already.
                var actualReplayer = _eventReplayerFactory.GetInstance(serviceProvider);

                return new EventReplayerInvoker<TEvent, TEntity>(actualReplayer);
            }
        }

        private sealed class Resolver : IEventReplayerResolver<TId, TEventBase, TEntityBase>
        {
            private readonly EventReplayerRegistry<TId, TEventBase, TEntityBase> _registry;
            private readonly IServiceProvider _serviceProvider;

            public Resolver(EventReplayerRegistry<TId, TEventBase, TEntityBase> registry, IServiceProvider serviceProvider)
            {
                Debug.Assert(registry != null);
                Debug.Assert(serviceProvider != null);

                _registry = registry;
                _serviceProvider = serviceProvider;
            }

            public IEventReplayer<TId, TEventBase, TEntityBase> Resolve(Type eventType)
            {
                if (_registry._replayer.TryGetValue(eventType, out var perEventRegistry))
                {
                    var handlerRegistry = perEventRegistry.Values.FirstOrDefault();

                    if (handlerRegistry != null && handlerRegistry.TryGetHandler(out var replayer))
                    {
                        Debug.Assert(replayer != null);

                        return replayer.GetInstance(_serviceProvider);
                    }
                }

                return null;
            }

            public IEventReplayer<TId, TEventBase, TEntityBase> Resolve(Type eventType, Type entityType)
            {
                if (_registry._replayer.TryGetValue(eventType, out var perEventRegistry))
                {
                    for (var currEntityType = entityType;
                         currEntityType != null && typeof(TEntityBase).GetTypeInfo().IsAssignableFrom(currEntityType);
                         currEntityType = currEntityType.GetTypeInfo().BaseType)
                    {
                        if (perEventRegistry.TryGetValue(currEntityType, out var handlerRegistry) &&
                            handlerRegistry.TryGetHandler(out var replayer))
                        {
                            Debug.Assert(replayer != null);

                            return replayer.GetInstance(_serviceProvider);
                        }
                    }
                }

                return null;
            }
        }
    }
}
