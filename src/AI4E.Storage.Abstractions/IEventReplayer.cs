/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        IEventReplayer.cs
 * Types:           AI4E.Storage.IEventReplayer'5
 *                  AI4E.Storage.IEventReplayer'3
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
using System.Threading.Tasks;

namespace AI4E.Storage
{
    /// <summary>
    /// Represents an event replayer.
    /// </summary>
    /// <typeparam name="TId">The type of entity identifier.</typeparam>
    /// <typeparam name="TEventBase">The event layer supertype.</typeparam>
    /// <typeparam name="TEntityBase">The entity layer supertype.</typeparam>
    /// <typeparam name="TEvent">The type of event.</typeparam>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    public interface IEventReplayer<TId, TEventBase, TEntityBase, TEvent, TEntity>
        where TId : struct, IEquatable<TId>
        where TEvent : TEventBase
        where TEntity : TEntityBase
    {
        /// <summary>
        /// Asynchronously replays a single event.
        /// </summary>
        /// <param name="evt">The event to replay.</param>
        /// <param name="entity">The current entity.</param>
        /// <returns>
        /// A value task representing the asynchronous operation.
        /// The <see cref="ValueTask{TResult}.Result"/> contains the result entity.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="evt"/>  is null.</exception>
        ValueTask<TEntity> ReplayAsync(TEvent evt, TEntity entity);
    }

    /// <summary>
    /// Represents an untyped event replayer.
    /// </summary>
    /// <typeparam name="TId">The type of entity identifier.</typeparam>
    /// <typeparam name="TEventBase">The event layer supertype.</typeparam>
    /// <typeparam name="TEntityBase">The entity layer supertype.</typeparam>
    public interface IEventReplayer<TId, TEventBase, TEntityBase>
        where TId : struct, IEquatable<TId>
    {
        /// <summary>
        /// Asynchronously replays a single event.
        /// </summary>
        /// <param name="evt">The event to replay.</param>
        /// <param name="entity">The current entity.</param>
        /// <returns>
        /// A value task representing the asynchronous operation.
        /// The <see cref="ValueTask{TResult}.Result"/> contains the result entity.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown if either <paramref name="evt"/>  is null.</exception>
        ValueTask<TEntityBase> ReplayAsync(TEventBase evt, TEntityBase entity);

        /// <summary>
        /// Gets the entity type.
        /// </summary>
        Type EntityType { get; }

        /// <summary>
        /// Gets the event type.
        /// </summary>
        Type EventType { get; }
    }
}
