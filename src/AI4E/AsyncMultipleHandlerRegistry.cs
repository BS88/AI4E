/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        AsyncMultipleHandlerRegistry.cs 
 * Types:           AI4E.AsyncMultipleHandlerRegistry'1
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   19.06.2017 
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
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace AI4E
{
    /// <summary>
    /// Represents an asychronous registry with multiple handlers activated at once.
    /// </summary>
    /// <typeparam name="THandler">The type of handler.</typeparam>
    public sealed class AsyncMultipleHandlerRegistry<THandler> : IAsyncMultipleHandlerRegistry<THandler>
    {
        private readonly AsyncLock _lock = new AsyncLock();
        private readonly IDispatchForwarding _dispatchForwarding;

        private volatile ImmutableHashSet<IHandlerFactory<THandler>> _handlers = ImmutableHashSet<IHandlerFactory<THandler>>.Empty;

        /// <summary>
        /// Creates a new instance of the <see cref="AsyncMultipleHandlerRegistry{THandler}"/> type.
        /// </summary>
        public AsyncMultipleHandlerRegistry() : this(DispatchForwarding.None) { }

        public AsyncMultipleHandlerRegistry(IDispatchForwarding dispatchForwarding)
        {
            if (dispatchForwarding == null)
                throw new ArgumentNullException(nameof(dispatchForwarding));

            _dispatchForwarding = dispatchForwarding;
        }

        /// <summary>
        /// Asynchronously registers a handler.
        /// </summary>
        /// <param name="handlerFactory">The handler to register.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="handlerFactory"/> is null.</exception>
        public async Task RegisterAsync(IHandlerFactory<THandler> handlerFactory)
        {
            if (handlerFactory == null)
                throw new ArgumentNullException(nameof(handlerFactory));

            Debug.Assert(_handlers != null);
            Debug.Assert(_lock != null);

            using (await _lock.LockAsync())
            {
                var handlers = _handlers.Add(handlerFactory);

                if (handlers == _handlers)
                    return;

                if (_handlers.IsEmpty)
                {
                    await _dispatchForwarding.RegisterForwardingAsync();
                }

                if (_handlers.Count == 1 && _handlers.First() is IDeactivationNotifyable deactivationNotifyable)
                {
                    await deactivationNotifyable.NotifyDeactivationAsync();
                }

                _handlers = handlers;

                if (handlers.Count == 1 && handlerFactory is IActivationNotifyable activationNotifyable)
                {
                    await activationNotifyable.NotifyActivationAsync();
                }
            }
        }

        /// <summary>
        /// Asynchronously deregisters a handler.
        /// </summary>
        /// <param name="handlerFactory">The handler to deregister.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The value of the <see cref="Task{TResult}.Result"/> parameter contains a boolean value
        /// indicating whether the handler was actually found and deregistered.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="handlerFactory"/> is null.</exception>
        public async Task<bool> DeregisterAsync(IHandlerFactory<THandler> handlerFactory)
        {
            if (handlerFactory == null)
                throw new ArgumentNullException(nameof(handlerFactory));

            Debug.Assert(_handlers != null);
            Debug.Assert(_lock != null);

            using (await _lock.LockAsync())
            {
                if (!_handlers.Contains(handlerFactory))
                    return false;

                var handlers = _handlers.Remove(handlerFactory);

                if (handlers.IsEmpty)
                {
                    await _dispatchForwarding.UnregisterForwardingAsync();
                }

                if (handlers.IsEmpty && handlerFactory is IDeactivationNotifyable deactivationNotifyable)
                {
                    await deactivationNotifyable.NotifyDeactivationAsync();
                }

                _handlers = handlers;

                if (_handlers.Count == 1 && _handlers.First() is IActivationNotifyable activationNotifyable)
                {
                    await activationNotifyable.NotifyActivationAsync();
                }
                return true;
            }
        }

        /// <summary>
        /// Returns a collection if activated handlers.
        /// </summary>
        /// <returns>The collection of activated handlers.</returns>
        public IEnumerable<IHandlerFactory<THandler>> GetHandlerFactories()
        {
            return _handlers; // Volatile read op
        }
    }
}
