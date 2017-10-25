/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        IEventProcessor.cs
 * Types:           AI4E.IEventProcessor'2
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

using System.Collections.Generic;

namespace AI4E
{
    /// <summary>
    /// Represents a processor for events (A unit of work on events).
    /// </summary>
    /// <typeparam name="TEntityBase">The entity layer supertype.</typeparam>
    /// <typeparam name="TEventBase">The event layer supertype.</typeparam>
    public interface IEventProcessor<TEventBase, TEntityBase>
    {
        /// <summary>
        /// Registers an event with the specified event-source.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to register.</typeparam>
        /// <param name="entity">The entity that registers the event (The event-source.)</param>
        /// <param name="evt">The event to register.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if either <paramref name="entity"/> or <paramref name="evt"/> is null.</exception>
        void RegisterEvent<TEvent>(TEntityBase entity, TEvent evt) where TEvent : TEventBase;

        /// <summary>
        /// Returns a collection of uncommitted domain events of the specified entity.
        /// </summary>
        /// <param name="entity">The entity to retrieve all uncommitted domain events of.</param>
        /// <returns>A collection of domain events.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="entity"/> is null.</exception>
        IEnumerable<TEventBase> GetUncommittedEvents(TEntityBase entity);

        /// <summary>
        /// Gets a collection of all entites the processor contains uncommitted events for.
        /// </summary>
        IEnumerable<TEntityBase> UpdatedEntities { get; }

        /// <summary>
        /// Gets a collection of all uncommited events in the order of aggregation.
        /// </summary>
        IEnumerable<TEventBase> UncommittedEvents { get; }

        /// <summary>
        /// Commits all uncommitted event of the specified entity.
        /// </summary>
        void Commit();

        /// <summary>
        /// Commits all uncommitted event of the specified entity.
        /// </summary>
        /// <param name="entity">The entity whose uncommitted domain events shall be committed.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="entity"/> is null.</exception>
        void Commit(TEntityBase entity);
    }
}
