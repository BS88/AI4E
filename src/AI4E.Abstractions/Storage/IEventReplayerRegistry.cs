/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        IEventReplayerRegistry.cs
 * Types:           AI4E.Storage.IEventReplayerRegistry'3
 *                  AI4E.Storage.IEventReplayerRegistry'5
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
    /// Represents a registry for event replayers.
    /// </summary>
    /// <typeparam name="TId">The type of entity identifier.</typeparam>
    /// <typeparam name="TEventBase">The event layer supertype.</typeparam>
    /// <typeparam name="TEntityBase">The entitiy layer supertype.</typeparam>
    public interface IEventReplayerRegistry<TId, TEventBase, TEntityBase>
        where TId : struct, IEquatable<TId>
    {
        /// <summary>
        /// Asynchronously registers an event replayer for the specified types of event and entity.
        /// </summary>
        /// <typeparam name="TEvent">The type of event.</typeparam>
        /// <typeparam name="TEntity">The type of entity.</typeparam>
        /// <param name="eventReplayerFactory">The event replayer to register.</param>
        /// <returns>
        /// A <see cref="IHandlerRegistration"/> representing the asynchronous operation.
        /// The <see cref="IHandlerRegistration"/> cancels the handler registration if completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="eventReplayerFactory"/> is null.</exception>
        Task<IHandlerRegistration> RegisterAsync<TEvent, TEntity>(IHandlerProvider<IEventReplayer<TId, TEventBase, TEntityBase, TEvent, TEntity>> eventReplayerFactory)
            where TEvent : TEventBase
            where TEntity : TEntityBase; // TODO: Correct xml-comments


        IEventReplayerResolver<TId, TEventBase, TEntityBase> GetResolver(IServiceProvider serviceProvider); // TODO: Does this member fit in here?
    }

    public interface IEventReplayerRegistry<TId, TEventBase, TEntityBase, TEvent, TEntity>
        where TId : struct, IEquatable<TId>
        where TEvent : TEventBase
        where TEntity : TEntityBase
    {
        Task<IHandlerRegistration> RegisterAsync(IHandlerProvider<IEventReplayer<TId, TEventBase, TEntityBase, TEvent, TEntity>> eventReplayerFactory);

        ValueTask<TEntity> ReplayAsync(TEvent evt, TEntity entity, IServiceProvider serviceProvider);
    }
}
