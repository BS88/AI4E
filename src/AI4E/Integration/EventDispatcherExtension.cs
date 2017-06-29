/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        EventDispatcherExtension.cs 
 * Types:           AI4E.Integration.EventDispatcherExtension
 *                  AI4E.Integration.EventDispatcherExtension.AnonymousEventHandler'1
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
using System.Diagnostics;
using System.Threading.Tasks;

namespace AI4E.Integration
{
    /// <summary>
    /// Defines extensions for the <see cref="IEventDispatcher"/> interface.
    /// </summary>
    public static class EventDispatcherExtension
    {
        /// <summary>
        ///  Asynchronously registers an anonymous event handler for the specified type of event.
        /// </summary>
        /// <typeparam name="TEvent">The type of event.</typeparam>
        /// <param name="eventDispatcher">The event dispatcher.</param>
        /// <param name="handler">The event handler that shall be registered.</param>
        /// <returns>
        /// A <see cref="IHandlerRegistration"/> representing the asynchronous operation.
        /// The <see cref="IHandlerRegistration"/> cancels the handler registration if completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if either <paramref name="eventDispatcher"/> or <paramref name="handler"/> is null.</exception>
        public static Task<IHandlerRegistration<IEventHandler<TEvent>>> OnEvent<TEvent>(this IEventDispatcher eventDispatcher, Func<TEvent, Task> handler) // TODO: Correct xml-comments
        {
            if (eventDispatcher == null)
                throw new ArgumentNullException(nameof(eventDispatcher));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            return eventDispatcher.RegisterAsync(new AnonymousEventHandler<TEvent>(handler));
        }

        private class AnonymousEventHandler<TEvent> : IEventHandler<TEvent>, IHandlerFactory<IEventHandler<TEvent>>
        {
            private readonly Func<TEvent, Task> _handler;

            internal AnonymousEventHandler(Func<TEvent, Task> handler)
            {
                Debug.Assert(handler != null);

                _handler = handler;
            }

            public Task HandleAsync(TEvent evt)
            {
                if (evt == null)
                    throw new ArgumentNullException(nameof(evt));

                return _handler(evt);
            }

            public IEventHandler<TEvent> GetHandler(IServiceProvider serviceProvider)
            {
                return this;
            }
        }

        public static Task<IHandlerRegistration<IEventHandler<TEvent>>> RegisterAsync<TEvent, TEventHandler>(this IEventDispatcher eventDispatcher)
            where TEventHandler : IEventHandler<TEvent>
        {
            if (eventDispatcher == null)
                throw new ArgumentNullException(nameof(eventDispatcher));

            return eventDispatcher.RegisterAsync((IHandlerFactory<IEventHandler<TEvent>>)(IHandlerFactory<TEventHandler>)new DefaultHandlerFactory<TEventHandler>());
        }

        public static Task<IHandlerRegistration<IEventHandler<TEvent>>> RegisterAsync<TEvent>(this IEventDispatcher eventDispatcher, IEventHandler<TEvent> singletonHandler)
        {
            if (eventDispatcher == null)
                throw new ArgumentNullException(nameof(eventDispatcher));

            return eventDispatcher.RegisterAsync(new DefaultHandlerFactory<IEventHandler<TEvent>>(singletonHandler));
        }
    }
}
