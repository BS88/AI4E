/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        EntityStore.cs 
 * Types:           AI4E.Storage.EntityStore'3
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AI4E.Storage
{
    /// <summary>
    /// Represents an event-sourced entity data store.
    /// </summary>
    /// <typeparam name="TId">The type of entity identifier.</typeparam>
    /// <typeparam name="TEventBase">The event layer supertype.</typeparam>
    /// <typeparam name="TEntityBase">The entity layer supertype.</typeparam>
    public sealed class EntityStore<TId, TEventBase, TEventPublisher, TEntityBase> : IEntityStore<TId, TEventBase, TEventPublisher, TEntityBase>
        where TId : struct, IEquatable<TId>
        where TEventPublisher : class, new()
    {
        private readonly IEventStore<TId, TEventBase> _eventStore;
        private readonly EntityCache<TId, TEventBase, TEventPublisher, TEntityBase> _entityCache;
        private readonly IEntityAccessor<TId, TEventPublisher, TEntityBase> _entityAccessor;
        private readonly Dictionary<TId, (TEntityBase obj, TId commit)> _identityMap = new Dictionary<TId, (TEntityBase obj, TId commit)>();
        private readonly IEventProcessor<TEventBase, TEntityBase> _eventProcessor;
        private readonly IEventProcessorBinder<TEventBase, TEventPublisher, TEntityBase> _eventProcessorBinder;
        private bool _isDisposed;

        #region C'tor

        public EntityStore(IEventStore<TId, TEventBase> eventStore, // Singleton
                           EntityCache<TId, TEventBase, TEventPublisher, TEntityBase> entityCache, // Singleton
                           IEntityAccessor<TId, TEventPublisher, TEntityBase> entityAccessor, // Singleton (or scoped or transient)
                           IEventAccessor<TId, TEventBase> eventAccessor, // Singleton (or scoped or transient)
                           IEventProcessor<TEventBase, TEntityBase> eventProcessor, // Scoped
                           IEventProcessorBinder<TEventBase, TEventPublisher, TEntityBase> eventProcessorBinder) // Singleton (or scoped or tranient)
        {
            if (eventStore == null)
                throw new ArgumentNullException(nameof(eventStore));

            if (entityCache == null)
                throw new ArgumentNullException(nameof(entityCache));

            if (entityAccessor == null)
                throw new ArgumentNullException(nameof(entityAccessor));

            if (eventAccessor == null)
                throw new ArgumentNullException(nameof(eventAccessor));

            if (eventProcessor == null)
                throw new ArgumentNullException(nameof(eventProcessor));

            if (eventProcessorBinder == null)
                throw new ArgumentNullException(nameof(eventProcessorBinder));

            _eventStore = eventStore;
            _entityCache = entityCache;
            _entityAccessor = entityAccessor;
            _eventProcessor = eventProcessor;
            _eventProcessorBinder = eventProcessorBinder;
        }

        #endregion

        /// <summary>
        /// Gets a boolean value indicating whether any changes are pending.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        public bool ChangesPending => ThrowIfDisposed(_eventProcessor.UpdatedEntities.Any());

        public TEventPublisher EventPublisher
        {
            get
            {
                var eventPublisher = new TEventPublisher();
                _eventProcessorBinder.BindProcessor(eventPublisher, _eventProcessor);
                return eventPublisher;
            }
        }

        /// <summary>
        /// Asynchronously saves all pending changes.
        /// </summary>
        /// <param name="cancellation">A <see cref="CancellationToken"/> used to cancel the asynchronous operation or <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        public async Task SaveChangesAsync(TId expectedCommit = default, CancellationToken cancellation = default)
        {
            ThrowIfDisposed();

            // Save all events to the event-store and publish them (push them to the event-dispatcher)
            var commit = await _eventStore.PushAsync(_eventProcessor.UncommittedEvents, expectedCommit, cancellation);

            // Update the entities in the identity-map.
            foreach (var entity in _eventProcessor.UpdatedEntities)
            {
                var entityId = _entityAccessor.GetId(entity);

                // It is unknown whether a concurrency issue occured that could be resolved or no concurrency conflict occured.
                // This implied that we do not know whether the entity has the state of the revision 'commit'.
                // As a result we save the entity in the id map with unkown state so it will not be put in the 
                // entity-pool when destroying the entity-store.
                _identityMap[entityId] = (entity, default(TId));

                // TODO: Can we guarantee that the entity is in a specific revision?
                // We had to inspect the entities history. For that to work, event processors must store not only uncommited but also commited events.
                // When we hand back and forth entities to and from the cache, the processor has to inherit to others events for the specific event-stream.
            }

            // Reset the event-processor for all updated entities.
            _eventProcessor.Commit();

            Debug.Assert(!_eventProcessor.UncommittedEvents.Any());
            Debug.Assert(!_eventProcessor.UpdatedEntities.Any());
        }

        public async Task<(TEntity entity, bool found)> TryGetByIdAsync<TEntity>(TId id, CancellationToken cancellation = default)
            where TEntity : TEntityBase
        {
            ThrowIfDisposed();

            // The entity is present in the id-map.
            if (_identityMap.TryGetValue(id, out var value))
            {
                return ((TEntity)value.obj, true);
            }

            var cacheResult = await _entityCache.TryGetByIdAsync<TEntity>(id, cancellation);

            if (cacheResult.found)
            {
                _identityMap.Add(id, (cacheResult.entity, default(TId))); // TODO: revision

                _eventProcessorBinder.BindProcessor(_entityAccessor.GetEventPublisher(cacheResult.entity), _eventProcessor);

                return cacheResult;
            }

            // If no events are available, the entity is not existing, but maybe the consumer created it just before.
            // Perform a lookup in the event-processor.
            // Compiler bug: https://github.com/dotnet/roslyn/issues/19122
            var entityBase = _eventProcessor.UpdatedEntities.FirstOrDefault(p => _entityAccessor.GetId(p).Equals(id));

            if (entityBase is TEntity createdEntity)
                return (createdEntity, true);

            // The entity is just not existing
            return (default(TEntity), false);
        }

        /// <summary>
        /// Asynchronously retrieves an entity by its identifier and with the specified commit revision.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity.</typeparam>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellation">A <see cref="CancellationToken"/> used to cancel the asynchronous operation or <see cref="CancellationToken.None"/>.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains the found entity or the default value of <typeparamref name="TEntity"/> if no entity was found. 
        /// </returns>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        public async Task<TEntity> GetByIdAsync<TEntity>(TId id, CancellationToken cancellation = default)
            where TEntity : TEntityBase
        {
            var result = await TryGetByIdAsync<TEntity>(id, cancellation);

            if (result.found)
                return result.entity;

            return default;
        }

        /// <summary>
        /// Disposes of the current instance.
        /// </summary>
        /// <remarks>
        /// After disposal, no members of the object may be called any more. 
        /// Doing this may result in an <see cref="ObjectDisposedException"/>.
        /// </remarks>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            foreach (var entry in _identityMap.Values)
            {
                if (entry.commit.Equals(default))
                    continue;

                _entityCache.PutAsync(entry.obj, entry.commit);
            }
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        private T ThrowIfDisposed<T>(T t)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            return t;
        }
    }
}
