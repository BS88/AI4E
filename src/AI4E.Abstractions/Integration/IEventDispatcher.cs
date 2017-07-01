/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        IEventDispatcher.cs
 * Types:           (1) AI4E.Integration.IEventDispatcher
 *                  (2) AI4E.Integration.IEventDispatcher'1
 *                  (3) AI4E.Integration.Integration.ISecureEventDispatcher
 *                  (4) AI4E.Integration.Integration.ISecureEventDispatcher'1
 *                  (5) AI4E.Integration.INonGenericEventDispatcher
 *                  (6) AI4E.Integration.ITypedNonGenericEventDispatcher
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   01.07.2017 
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

namespace AI4E.Integration
{
    /// <summary>
    /// Represents an event dispatcher that dispatches events to event handlers.
    /// </summary>
    public interface IEventDispatcher
    {
        /// <summary>
        /// Asynchronously registers an event handler.
        /// </summary>
        /// <typeparam name="TEvent">The type of event.</typeparam>
        /// <param name="eventHandlerFactory">The event handler to register.</param>
        /// <returns>
        /// A <see cref="IHandlerRegistration"/> representing the asynchronous operation.
        /// The <see cref="IHandlerRegistration"/> cancels the handler registration if completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="eventHandlerFactory"/> is null.</exception>
        Task<IHandlerRegistration<IEventHandler<TEvent>>> RegisterAsync<TEvent>(IHandlerFactory<IEventHandler<TEvent>> eventHandlerFactory); // TODO: Correct xml-comments

        /// <summary>
        /// Returns a typed event dispatcher for the specified event type.
        /// </summary>
        /// <typeparam name="TEvent">The type of event.</typeparam>
        /// <returns>A typed event dispatcher for events of type <typeparamref name="TEvent"/>.</returns>
        IEventDispatcher<TEvent> GetTypedDispatcher<TEvent>();

        /// <summary>
        /// Asynchronously dispatches an event.
        /// </summary>
        /// <typeparam name="TEvent">The type of event.</typeparam>
        /// <param name="evt">The event to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="evt"/> is null.</exception>
        Task NotifyAsync<TEvent>(TEvent evt);
    }

    /// <summary>
    /// Represents a typed event dispatcher that dispatches events to event handlers.
    /// </summary>
    /// <typeparam name="TEvent">The type of event.</typeparam>
    public interface IEventDispatcher<TEvent>
    {
        /// <summary>
        /// Asynchronously registers an event handler.
        /// </summary>
        /// <param name="eventHandlerFactory">The event handler to register.</param>
        /// <returns>
        /// A <see cref="IHandlerRegistration"/> representing the asynchronous operation.
        /// The <see cref="IHandlerRegistration"/> cancels the handler registration if completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="eventHandlerFactory"/> is null.</exception>
        Task<IHandlerRegistration<IEventHandler<TEvent>>> RegisterAsync(IHandlerFactory<IEventHandler<TEvent>> eventHandlerFactory); // TODO: Correct xml-comments

        /// <summary>
        /// Asynchronously dispatches an event.
        /// </summary>
        /// <param name="evt">The event to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="evt"/> is null.</exception>
        Task NotifyAsync(TEvent evt);
    }

    /// <summary>
    /// Represents a event dispatcher that controls access.
    /// </summary>
    public interface ISecureEventDispatcher : IEventDispatcher
    {
        /// <summary>
        /// Returns a boolean value indicating whether registering the specified event handler is authorized.
        /// </summary>
        /// <typeparam name="TEvent">The type of event.</typeparam>
        /// <param name="eventHandlerFactory">The event handler that shall be registered.</param>
        /// <returns>True if registering <paramref name="eventHandlerFactory"/> is authorized, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="eventHandlerFactory"/> is null.</exception>
        bool IsRegistrationAuthorized<TEvent>(IHandlerFactory<IEventHandler<TEvent>> eventHandlerFactory);

        /// <summary>
        /// Returns a boolean value indicating whether dispatching the specified event handler is authorized.
        /// </summary>
        /// <typeparam name="TEvent">The type of event.</typeparam>
        /// <param name="evt">The event that shall be dispatched.</param>
        /// <returns>True if dispatching <paramref name="evt"/> is authorized, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="evt"/> is null.</exception>
        bool IsDispatchAuthorized<TEvent>(TEvent evt);
    }

    /// <summary>
    /// Represents a typed event dispatcher that controls access.
    /// </summary>
    public interface ISecureEventDispatcher<TEvent> : IEventDispatcher<TEvent>
    {
        /// <summary>
        /// Returns a boolean value indicating whether registering the specified event handler is authorized.
        /// </summary>
        /// <param name="eventHandlerFactory">The event handler that shall be registered.</param>
        /// <returns>True if registering <paramref name="eventHandlerFactory"/> is authorized, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="eventHandlerFactory"/> is null.</exception>
        bool IsRegistrationAuthorized(IHandlerFactory<IEventHandler<TEvent>> eventHandlerFactory);

        /// <summary>
        /// Returns a boolean value indicating whether dispatching the specified event handler is authorized.
        /// </summary>
        /// <param name="evt">The event that shall be dispatched.</param>
        /// <returns>True if dispatching <paramref name="evt"/> is authorized, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="evt"/> is null.</exception>
        bool IsDispatchAuthorized(TEvent evt);
    }

    /// <summary>
    /// Represents a non-generic event dispatcher that dispatches events to event handler.
    /// </summary>
    public interface INonGenericEventDispatcher
    {
        /// <summary>
        /// Returns a typed event dispatcher for the specified event type.
        /// </summary>
        /// <param name="eventType">The type of event.</param>
        /// <returns>A typed event dispatcher for events of type <paramref name="eventType"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="eventType"/> is null.</exception>
        ITypedNonGenericEventDispatcher GetTypedDispatcher(Type eventType);

        /// <summary>
        /// Asynchronously dispatches an event.
        /// </summary>
        /// <param name="eventType">The type of the event.</param>
        /// <param name="evt">The event to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if either <paramref name="eventType"/> or <paramref name="evt"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="evt"/> is not of the type <paramref name="eventType"/> or a derived type.</exception>
        Task NotifyAsync(Type eventType, object evt);
    }

    /// <summary>
    /// Represents a typed non-generic event dispatcher that dispatches events to event handler.
    /// </summary>
    public interface ITypedNonGenericEventDispatcher
    {
        /// <summary>
        /// Asynchronously dispatches an event.
        /// </summary>
        /// <param name="evt">The event to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="evt"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="evt"/> is not of the type <see cref="EventType"/> or a derived type.</exception>
        Task NotifyAsync(object evt);

        /// <summary>
        /// Gets the type of events the dispatcher can handle.
        /// </summary>
        Type EventType { get; }
    }
}
