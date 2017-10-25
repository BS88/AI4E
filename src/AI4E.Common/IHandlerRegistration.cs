/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        HandlerRegistration.cs 
 * Types:           AI4E.HandlerRegistration
 *                  AI4E.HandlerRegistrationSource
 *                  AI4E.HandlerRegistrationSource'1
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
using System.Threading;
using System.Threading.Tasks;
using AI4E.Async;

namespace AI4E
{
    public interface IHandlerRegistration : IAsyncCompletion { }

    public interface IHandlerRegistration<THandler> : IHandlerRegistration
    {
        IContextualProvider<THandler> Handler { get; }
    }

    internal sealed class HandlerRegistration<THandler> : IHandlerRegistration<THandler>
    {
        private readonly IAsyncHandlerRegistry<THandler> _handlerRegistry;
        private readonly IContextualProvider<THandler> _handlerFactory;
        private readonly TaskCompletionSource<object> _completionSource = new TaskCompletionSource<object>();
        private int _isCompleting = 0;

        public HandlerRegistration(IAsyncHandlerRegistry<THandler> handlerRegistry,
                                         IContextualProvider<THandler> handlerFactory)

        {
            if (handlerRegistry == null)
                throw new ArgumentNullException(nameof(handlerRegistry));

            if (handlerFactory == null)
                throw new ArgumentNullException(nameof(handlerFactory));

            _handlerRegistry = handlerRegistry;
            _handlerFactory = handlerFactory;

            Initialization = _handlerRegistry.RegisterAsync(_handlerFactory);
        }

        public Task Initialization { get; }

        public Task Completion => _completionSource.Task;

        public IContextualProvider<THandler> Handler => _handlerFactory;

        public async void Complete()
        {
            if (Interlocked.Exchange(ref _isCompleting, 1) != 0)
                return;

            try
            {
                await _handlerRegistry.DeregisterAsync(_handlerFactory);
                _completionSource.SetResult(null);
            }
            catch (TaskCanceledException)
            {
                _completionSource.SetCanceled();
            }
            catch (Exception exc)
            {
                _completionSource.SetException(exc);
            }
        }
    }

    public static class HandlerRegistration
    {
        public static async Task<IHandlerRegistration<THandler>> CreateRegistrationAsync<THandler>(IAsyncHandlerRegistry<THandler> handlerRegistry, IContextualProvider<THandler> handlerFactory)
        {
            var registration = new HandlerRegistration<THandler>(handlerRegistry, handlerFactory);

            await registration.Initialization;

            return registration;
        }
    }
}