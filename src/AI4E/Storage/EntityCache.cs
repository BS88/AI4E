/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        EntityCache.cs
 * Types:           AI4E.Storage.EntityCache'4
 *                  AI4E.Storage.EntityCache'4.EventProcessor
 *                  AI4E.Storage.EntityCache'4.EventHandler
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   17.05.2017 
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AI4E.Async;
using AI4E.Integration;
using AI4E.Integration.EventResults;
using Microsoft.Extensions.DependencyInjection;

namespace AI4E.Storage
{
    public sealed class EntityCache<TId, TEventBase, TEventPublisher, TEntityBase> :
        IReadOnlyEntityStore<TId, TEventBase, TEventPublisher, TEntityBase>,
        IAsyncCompletion
        where TId : struct, IEquatable<TId>
        where TEventPublisher : class, new()
    {
        private readonly IEventProcessorBinder<TEventBase, TEventPublisher, TEntityBase> _eventProcessorBinder;
        private readonly IEventStore<TId, TEventBase> _eventStore;
        private readonly EventProcessor _eventProcessor;
        private readonly ConcurrentDictionary<TId, (ConcurrentStack<TEntityBase> cache, TId commit)> _pool = new ConcurrentDictionary<TId, (ConcurrentStack<TEntityBase>, TId)>();
        private readonly IContextualProvider<IEventReplayerResolver<TId, TEventBase, TEntityBase>> _eventReplayerGenerator;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly Task<IHandlerRegistration<IEventHandler<TEventBase>>> _handlerRegistration;
        private readonly TaskCompletionSource<object> _completionSource = new TaskCompletionSource<object>();
        private int _isCompleted = 0;
        private readonly IEventAccessor<TId, TEventBase> _eventAccessor;
        private readonly IEntityAccessor<TId, TEventPublisher, TEntityBase> _entityAccessor;

        public EntityCache(IEventStore<TId, TEventBase> eventStore,
                           IEventDispatcher eventDispatcher,
                           IContextualProvider<IEventReplayerResolver<TId, TEventBase, TEntityBase>> eventReplayerGenerator,
                           IEventProcessorBinder<TEventBase, TEventPublisher, TEntityBase> eventProcessorBinder,
                           IEntityAccessor<TId, TEventPublisher, TEntityBase> entityAccessor,
                           IEventAccessor<TId, TEventBase> eventAccessor)
        {
            if (eventStore == null)
                throw new ArgumentNullException(nameof(eventStore));

            if (eventDispatcher == null)
                throw new ArgumentNullException(nameof(eventDispatcher));

            if (eventReplayerGenerator == null)
                throw new ArgumentNullException(nameof(eventReplayerGenerator));

            if (eventProcessorBinder == null)
                throw new ArgumentNullException(nameof(eventProcessorBinder));

            if (entityAccessor == null)
                throw new ArgumentNullException(nameof(entityAccessor));

            if (eventAccessor == null)
                throw new ArgumentNullException(nameof(eventAccessor));

            _eventStore = eventStore;
            _eventDispatcher = eventDispatcher;
            _eventReplayerGenerator = eventReplayerGenerator;
            _eventProcessorBinder = eventProcessorBinder;
            _entityAccessor = entityAccessor;
            _eventAccessor = eventAccessor;

            _eventProcessor = new EventProcessor();

            _handlerRegistration = _eventDispatcher.RegisterAsync(new EventHandler(this));
        }

        public async Task<(TEntity entity, bool found)> TryGetByIdAsync<TEntity>(TId id, CancellationToken cancellation = default)
            where TEntity : TEntityBase
        {
            await _handlerRegistration;

            if (!_pool.TryGetValue(id, out var entry))
            {
                var commit = await _eventStore.GetRevisionAsync(id, cancellation);
                entry = _pool.GetOrAdd(id, p => (new ConcurrentStack<TEntityBase>(), commit));
            }

            var cache = entry.cache;

            // It is an instance with the id cached.
            if (cache.TryPop(out var entityBase))
            {
                // The cached entity is of the requested type.
                if (entityBase is TEntity entity)
                    return (entity, true);

                // The cached entity is not of the requested type.
                // Push the removed entity back to the pool.
                cache.Push(entityBase);
                return (default(TEntity), false);
            }

            // No instance cached => Create one.
            // Try to load the entity history from the event store.
            var events = _eventStore.GetEventsAsync(id, cancellation);

            using (var enumerator = events.GetEnumerator())
            {
                if (!await enumerator.MoveNext(cancellation))
                    return (default(TEntity), false);

                // TODO: Design this configurable
                var serviceCollection = new ServiceCollection();
                serviceCollection.AddSingleton<IReadOnlyEntityStore<TId, TEventBase, TEventPublisher, TEntityBase>>(this);
                var provider = serviceCollection.BuildServiceProvider();
                var eventReplayerResolver = _eventReplayerGenerator.Generate(provider);

                var ok = true;
                var entity = default(TEntity);
                var commit = default(TId);

                do
                {
                    if (enumerator.Current.evt == null)
                        throw new InvalidOperationException("The event store provided a sequence that contains null events."); // TODO: Is this the correct exception type?

                    (entity, ok) = await HandleSingleEvent(entity, enumerator.Current.evt, eventReplayerResolver);

                    // Type mismatch.
                    if (!ok)
                    {
                        return (default(TEntity), false);
                    }

                    commit = enumerator.Current.commit;
                }
                while (await enumerator.MoveNext(cancellation));

                _eventProcessor.Commit(entity);
                _eventProcessorBinder.UnbindProcessor(_entityAccessor.GetEventPublisher(entity));

                return (entity, true);

                // TODO: Is this save for reference recursion? A needs B needs A
            }
        }

        public async Task<TEntity> GetByIdAsync<TEntity>(TId id, CancellationToken cancellation = default) where TEntity : TEntityBase
        {
            var result = await TryGetByIdAsync<TEntity>(id, cancellation);

            if (result.found)
                return result.entity;

            return default;
        }

        public async Task PutAsync(TEntityBase entity, TId commit, CancellationToken cancellation = default)
        {
            await _handlerRegistration;

            var id = _entityAccessor.GetId(entity);

            if (!_pool.TryGetValue(id, out var entry))
            {
                var c = await _eventStore.GetRevisionAsync(id, cancellation);
                entry = _pool.GetOrAdd(id, p => (new ConcurrentStack<TEntityBase>(), c));
            }

            var cache = entry.cache;

            if (!entry.commit.Equals(commit))
                return;

            _eventProcessorBinder.UnbindProcessor(_entityAccessor.GetEventPublisher(entity));

            cache.Push(entity);
        }

        private async Task<(TEntity entity, bool ok)> HandleSingleEvent<TEntity>(TEntity entity, TEventBase evt, IEventReplayerResolver<TId, TEventBase, TEntityBase> eventReplayerResolver)
            where TEntity : TEntityBase
        {
            Debug.Assert(evt != null);

            var replayer = entity == null ?
                eventReplayerResolver.Resolve(evt.GetType()) :
                eventReplayerResolver.Resolve(evt.GetType(), entity.GetType());

            if (replayer == null)
                throw new InvalidOperationException($"The event '{evt.GetType().FullName}' cannot be replayed. It is no replayer available.");

            if (!(await replayer.ReplayAsync(evt, entity) is TEntity resultEntity))
            {
                return (default(TEntity), false);
            }

            return (resultEntity, true);
        }

        public TEventPublisher EventPublisher
        {
            get
            {
                var eventPublisher = new TEventPublisher();
                _eventProcessorBinder.BindProcessor(eventPublisher, _eventProcessor);
                return eventPublisher;
            }
        }

        private sealed class EventProcessor : IEventProcessor<TEventBase, TEntityBase>
        {
            public void RegisterEvent<TEvent>(TEntityBase entity, TEvent evt) where TEvent : TEventBase { }

            public IEnumerable<TEventBase> GetUncommittedEvents(TEntityBase entity)
            {
                return Enumerable.Empty<TEventBase>();
            }

            public IEnumerable<TEntityBase> UpdatedEntities => Enumerable.Empty<TEntityBase>();

            public IEnumerable<TEventBase> UncommittedEvents => Enumerable.Empty<TEventBase>();

            public void Commit() { }

            public void Commit(TEntityBase entity) { }
        }

        private sealed class EventHandler : IEventHandler<TEventBase>, IHandlerProvider<IEventHandler<TEventBase>>
        {
            private readonly EntityCache<TId, TEventBase, TEventPublisher, TEntityBase> _entityCache;

            public EventHandler(EntityCache<TId, TEventBase, TEventPublisher, TEntityBase> entityCache)
            {
                Debug.Assert(entityCache != null);
                _entityCache = entityCache;
            }

            public IEventHandler<TEventBase> GetHandler(IServiceProvider serviceProvider)
            {
                return this;
            }

            public Task<IEventResult> HandleAsync(TEventBase evt)
            {
                var eventStream = _entityCache._eventAccessor.GetEventStream(evt);

                _entityCache._pool.TryRemove(eventStream, out _);

                return Task.FromResult<IEventResult>(SuccessEventResult.Default);
            }
        }

        public Task Completion => _completionSource.Task;

        public void Complete()
        {
            if (Interlocked.Exchange(ref _isCompleted, 1) != 0)
                return;

            DoCompletion();
        }

        private async void DoCompletion()
        {
            try
            {
                var handlerRegistration = await _handlerRegistration;

                handlerRegistration.Complete();
                await handlerRegistration.Completion;
            }
            catch (Exception exc)
            {
                _completionSource.SetException(exc);
            }

            _completionSource.SetResult(null);
        }
    }
}
