/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        IEntityStore.cs
 * Types:           AI4E.Storage.IReadOnlyEntityDataStore'2
 *                  AI4E.Storage.IEntityDataStore'2
 *                  AI4E.Storage.IQueryableEntityDataStore'2
 *                  AI4E.Storage.IEntityStore'4
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   16.05.2017 
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
using System.Threading;
using System.Threading.Tasks;

namespace AI4E.Storage
{
    /// <summary>
    /// Represents a read-only entity data store.
    /// </summary>
    /// <typeparam name="TId">The type of entity identifier.</typeparam>
    /// <typeparam name="TEntityBase">The entity layer supertype.</typeparam>
    public interface IReadOnlyEntityStore<TId, TEntityBase> : IDisposable
        where TId : struct, IEquatable<TId>
    {
        /// <summary>
        /// Asynchronously retrieves an entity by its identifier.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity.</typeparam>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellation">A <see cref="CancellationToken"/> used to cancel the asynchronous operation or <see cref="CancellationToken.None"/>.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains the found entity or the default value of <typeparamref name="TEntity"/> if no entity was found. 
        /// </returns>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        Task<TEntity> GetByIdAsync<TEntity>(TId id, CancellationToken cancellation = default(CancellationToken))
            where TEntity : TEntityBase;
    }

    /// <summary>
    /// Represents an entity data store.
    /// </summary>
    /// <typeparam name="TId">The type of entity identifier.</typeparam>
    /// <typeparam name="TEntityBase">The entity layer supertype.</typeparam>
    public interface IEntityStore<TId, TEntityBase> : IReadOnlyEntityStore<TId, TEntityBase>
        where TId : struct, IEquatable<TId>
    {
        /// <summary>
        /// Gets a boolean value indicating whether any changes are pending.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        bool ChangesPending { get; }

        /// <summary>
        /// Adds an entity to the store.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity.</typeparam>
        /// <param name="entity">The entity to store.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entity"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        void Add<TEntity>(TEntity entity)
            where TEntity : TEntityBase;

        /// <summary>
        /// Updates an entity in the store.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity.</typeparam>
        /// <param name="entity">The entity to update.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entity"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        void Update<TEntity>(TEntity entity)
            where TEntity : TEntityBase;

        /// <summary>
        /// Removes an entity from the store.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity.</typeparam>
        /// <param name="entity">The entity to remove.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entity"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        void Remove<TEntity>(TEntity entity)
            where TEntity : TEntityBase;

        /// <summary>
        /// Dicards all pending changes.
        /// </summary>
        void DiscardChanges();

        /// <summary>
        /// Asynchronously saves all pending changes.
        /// </summary>
        /// <param name="cancellation">A <see cref="CancellationToken"/> used to cancel the asynchronous operation or <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        Task SaveChangesAsync(CancellationToken cancellation = default(CancellationToken));
    }

    /// <summary>
    /// Represents a queryable entity data store.
    /// </summary>
    /// <typeparam name="TId">The type of entity identifier.</typeparam>
    /// <typeparam name="TEntityBase">The entity layer supertype.</typeparam>
    public interface IQueryableEntityStore<TId, TEntityBase> : IEntityStore<TId, TEntityBase>, IQueryableDataStore
        where TId : struct, IEquatable<TId>
    { }

    /// <summary>
    /// Represents a read-only view of an event-sourced entity store.
    /// </summary>
    /// <typeparam name="TId">The type of entity identifier.</typeparam>
    /// <typeparam name="TEventBase">The event layer supertype.</typeparam>
    /// <typeparam name="TEventPublisher">The type of event publisher.</typeparam>
    /// <typeparam name="TEntityBase">The entity layer supertype.</typeparam>
    public interface IReadOnlyEntityStore<TId, TEventBase, TEventPublisher, TEntityBase>
        where TId : struct, IEquatable<TId>
        where TEventPublisher : new()
    {
        /// <summary>
        /// Asynchronously retrieves an entity by its identifier.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity.</typeparam>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellation">A <see cref="CancellationToken"/> used to cancel the asynchronous operation or <see cref="CancellationToken.None"/>.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains the found entity or the default value of <typeparamref name="TEntity"/> if no entity was found. 
        /// </returns>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        Task<TEntity> GetByIdAsync<TEntity>(TId id, CancellationToken cancellation = default(CancellationToken))
            where TEntity : TEntityBase;

        /// <summary>
        /// Asynchronously tries to retrieve an entity by its identifier.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity.</typeparam>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellation">A <see cref="CancellationToken"/> used to cancel the asynchronous operation or <see cref="CancellationToken.None"/>.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains the found entity or the default value of <typeparamref name="TEntity"/> if no entity was found. 
        /// </returns>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        Task<(TEntity entity, bool found)> TryGetByIdAsync<TEntity>(TId id, CancellationToken cancellation = default(CancellationToken))
            where TEntity : TEntityBase;

        /// <summary>
        /// Gets the event publisher that can be used to publish domain events.
        /// </summary>
        TEventPublisher EventPublisher { get; }
    }

    /// <summary>
    /// Represents an event-sourced entity store.
    /// </summary>
    /// <typeparam name="TId">The type of entity identifier.</typeparam>
    /// <typeparam name="TEventBase">The event layer supertype.</typeparam>
    /// <typeparam name="TEventPublisher">The type of event publisher.</typeparam>
    /// <typeparam name="TEntityBase">The entity layer supertype.</typeparam>
    public interface IEntityStore<TId, TEventBase, TEventPublisher, TEntityBase> : IReadOnlyEntityStore<TId, TEventBase, TEventPublisher, TEntityBase>
        where TId : struct, IEquatable<TId>
        where TEventPublisher : new()
    {
        /// <summary>
        /// Gets a boolean value indicating whether any changes are pending.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        bool ChangesPending { get; }

        /// <summary>
        /// Asynchronously saves all pending changes.
        /// </summary>
        /// <param name="expectedCommit">The identifier of the commit, the business transaction started with.</param>
        /// <param name="cancellation">A <see cref="CancellationToken"/> used to cancel the asynchronous operation or <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        Task SaveChangesAsync(TId expectedCommit = default(TId), CancellationToken cancellation = default(CancellationToken));
    }
}
