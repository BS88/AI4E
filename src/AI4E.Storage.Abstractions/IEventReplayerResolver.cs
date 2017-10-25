/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        IEventReplayerResolver.cs
 * Types:           AI4E.Storage.IEventReplayerResolver'3
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   29.04.2017 
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

namespace AI4E.Storage
{
    /// <summary>
    /// Represents a resolver for dynamically typed event replayers.
    /// </summary>
    ///  <typeparam name="TId">The type of entity identifier.</typeparam>
    /// <typeparam name="TEventBase">The event layer supertype.</typeparam>
    /// <typeparam name="TEntityBase">The entitiy layer supertype.</typeparam>
    public interface IEventReplayerResolver<TId, TEventBase, TEntityBase>
        where TId : struct, IEquatable<TId>
    {
        /// <summary>
        /// Retrieves an event replayer for the specified type of event.
        /// </summary>
        /// <param name="eventType">The type of event.</param>
        /// <returns>An event replayer that can replayer events of type <paramref name="eventType"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="eventType"/> is null.</exception>
        IEventReplayer<TId, TEventBase, TEntityBase> Resolve(Type eventType);

        /// <summary>
        /// Retrieves an event replayer for the specified types of event and entity.
        /// </summary>
        /// <param name="eventType">The type of event.</param>
        /// <param name="entityType">The type of entity.</param>
        /// <returns>An event replayer that can replayer events of type <paramref name="eventType"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if either <paramref name="eventType"/> or <paramref name="entityType"/> is null.</exception>
        IEventReplayer<TId, TEventBase, TEntityBase> Resolve(Type eventType, Type entityType);
    }
}
