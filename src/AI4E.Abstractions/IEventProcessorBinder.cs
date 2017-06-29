/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        IEventProcessorBinder.cs
 * Types:           AI4E.IEventProcessorBinder'3
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

namespace AI4E
{
    /// <summary>
    /// Represents an accesor for event publishers.
    /// </summary>
    /// <typeparam name="TEventBase">The event layer supertype.</typeparam>
    /// <typeparam name="TEntityBase">The entity layer supertype.</typeparam>
    public interface IEventProcessorBinder<TEventBase, TEventPublisher, TEntityBase>
        where TEventPublisher : class, new()
    {
        /// <summary>
        /// Binds an event-processor to the specified entity.
        /// </summary>
        /// <param name="eventPublisher">The event-publisher.</param>
        /// <param name="eventProcessor">The event processor that shall be bound.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="eventProcessor"/> is null.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if <paramref name="eventPublisher"/> is already bound to an event-processor.</exception>
        void BindProcessor(TEventPublisher eventPublisher, IEventProcessor<TEventBase, TEntityBase> eventProcessor);

        /// <summary>
        /// Unbinds a bound event-processor from the specified entity.
        /// </summary>
        /// <param name="eventPublisher">The event-publisher.</param>
        void UnbindProcessor(TEventPublisher eventPublisher);
    }
}
