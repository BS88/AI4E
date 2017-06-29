/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        AsyncSingleHandlerRegistry.cs 
 * Types:           AI4E.AsyncSingleHandlerRegistry'1
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   18.06.2017 
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
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace AI4E
{
    /// <summary>
    /// Represents an asynchronous handler registry with only a single handler activated at once.
    /// </summary>
    /// <typeparam name="THandler">The type of handler.</typeparam>
    public sealed class AsyncSingleHandlerRegistry<THandler> : IAsyncSingleHandlerRegistry<THandler>
    {
        private readonly AsyncLock _lock = new AsyncLock();
        private readonly IDispatchForwarding _dispatchForwarding;

        // Cannot use ImmutableStack here, because stack does not allow to remove elements that are not on top of stack.
        private volatile ImmutableList<IHandlerFactory<THandler>> _handlerStack = ImmutableList<IHandlerFactory<THandler>>.Empty;

        /// <summary>
        /// Creates a new instance of the <see cref="AsyncSingleHandlerRegistry{THandler}"/> type.
        /// </summary>
        public AsyncSingleHandlerRegistry() : this(DispatchForwarding.None) { }

        public AsyncSingleHandlerRegistry(IDispatchForwarding dispatchForwarding)
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

            Debug.Assert(_handlerStack != null);
            Debug.Assert(_lock != null);

            using (await _lock.LockAsync())
            {
                if (_handlerStack.IsEmpty)
                {
                    await PushHandler(handlerFactory);
                    await _dispatchForwarding.RegisterForwardingAsync();
                }
                else
                {
                    var tos = _handlerStack.Last();

                    Debug.Assert(tos != null);

                    if (handlerFactory.Equals(tos))
                    {
                        await _dispatchForwarding.RegisterForwardingAsync();
                        return;
                    }

                    if (tos is IDeactivationNotifyable notifyable)
                        await notifyable.NotifyDeactivationAsync();

                    _handlerStack = _handlerStack.Remove(handlerFactory);
                    await PushHandler(handlerFactory);
                    await _dispatchForwarding.RegisterForwardingAsync();
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

            Debug.Assert(_handlerStack != null);
            Debug.Assert(_lock != null);

            using (await _lock.LockAsync())
            {
                if (_handlerStack.IsEmpty)
                {
                    return false;
                }

                var tos = _handlerStack.Last();

                Debug.Assert(tos != null);

                if (handlerFactory.Equals(tos))
                {
                    if (handlerFactory is IDeactivationNotifyable deactivationNotifyable)
                        await deactivationNotifyable.NotifyDeactivationAsync();

                    // Handler stack will be empty afterwards
                    if (_handlerStack.Count == 1)
                    {
                        await _dispatchForwarding.UnregisterForwardingAsync();
                    }
                    else if (_handlerStack[_handlerStack.Count - 2] is IActivationNotifyable activationNotifable)
                    {
                        await activationNotifable.NotifyActivationAsync();
                    }

                    _handlerStack = _handlerStack.RemoveAt(_handlerStack.Count - 1);

                    return true;
                }

                var newStack = _handlerStack.Remove(handlerFactory);

                if (newStack == _handlerStack)
                    return false;

                _handlerStack = newStack;
                return true;
            }
        }

        /// <summary>
        /// Tries to retrieve the currently activated handler.
        /// </summary>
        /// <param name="handlerFactory">Contains the handler if true is returned, otherwise the value is undefined.</param>
        /// <returns>True if a handler was found, false otherwise.</returns>
        public bool TryGetHandler(out IHandlerFactory<THandler> handlerFactory)
        {
            var handlerStack = _handlerStack;

            Debug.Assert(handlerStack != null);

            if (handlerStack.IsEmpty)
            {
                handlerFactory = null;
                return false;
            }

            handlerFactory = handlerStack.Last();
            return true;
        }

        private Task PushHandler(IHandlerFactory<THandler> handlerFactory)
        {
            _handlerStack = _handlerStack.Add(handlerFactory);

            if (handlerFactory is IActivationNotifyable notifyable)
            {
                return notifyable.NotifyActivationAsync();
            }

            return Task.CompletedTask;
        }
    }
}
