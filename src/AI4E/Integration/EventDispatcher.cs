/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        EventDispatcher.cs 
 * Types:           (1) AI4E.Integration.EventDispatcher
 *                  (2) AI4E.Integration.EventDispatcher'1
 *                  (3) AI4E.Integration.EventAuthorizationVerifyer
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

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AI4E.Integration
{
    /// <summary>
    /// Represents an event dispatcher that events to event handlers.
    /// </summary>
    public sealed class EventDispatcher : IEventDispatcher, ISecureEventDispatcher, INonGenericEventDispatcher
    {
        private static readonly Type _typedDispatcherType = typeof(EventDispatcher<>);

        private readonly ConcurrentDictionary<Type, ITypedNonGenericEventDispatcher> _typedDispatchers = new ConcurrentDictionary<Type, ITypedNonGenericEventDispatcher>();
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventAuthorizationVerifyer _authorizationVerifyer;

        /// <summary>
        /// Creates a new instance of the <see cref="EventDispatcher"/> type.
        /// </summary>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> used to obtain services.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="serviceProvider"/> is null.</exception>
        public EventDispatcher(IServiceProvider serviceProvider) : this(EventAuthorizationVerifyer.Default, serviceProvider) { }

        /// <summary>
        /// Creates a new instance of the <see cref="EventDispatcher"/> type.
        /// </summary>
        /// <param name="authorizationVerifyer">An <see cref="IEventAuthorizationVerifyer"/> that controls authorization or <see cref="EventAuthorizationVerifyer.Default"/>.</param>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> used to obtain services.</param>
        /// <exception cref="ArgumentNullException">Thrown if either <paramref name="serviceProvider"/> or <paramref name="authorizationVerifyer"/> is null.</exception>
        public EventDispatcher(IEventAuthorizationVerifyer authorizationVerifyer, IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            if (authorizationVerifyer == null)
                throw new ArgumentNullException(nameof(authorizationVerifyer));

            _serviceProvider = serviceProvider;
            _authorizationVerifyer = authorizationVerifyer;
        }

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
        /// <exception cref="UnauthorizedAccessException">Thrown if the access is unauthorized.</exception>
        public Task<IHandlerRegistration<IEventHandler<TEvent>>> RegisterAsync<TEvent>(IHandlerFactory<IEventHandler<TEvent>> eventHandlerFactory) // TODO: Correct xml-comments
        {
            if (eventHandlerFactory == null)
                throw new ArgumentNullException(nameof(eventHandlerFactory));

            if (!_authorizationVerifyer.AuthorizeHandlerRegistry())
                throw new UnauthorizedAccessException();

            return GetTypedDispatcher<TEvent>().RegisterAsync(eventHandlerFactory);
        }

        /// <summary>
        /// Returns a typed event dispatcher for the specified event type.
        /// </summary>
        /// <typeparam name="TEvent">The type of event.</typeparam>
        /// <returns>A typed event dispatcher for events of type <typeparamref name="TEvent"/>.</returns>
        public IEventDispatcher<TEvent> GetTypedDispatcher<TEvent>()
        {
            return _typedDispatchers.GetOrAdd(typeof(TEvent), t => new EventDispatcher<TEvent>(_authorizationVerifyer, _serviceProvider)) as IEventDispatcher<TEvent>;
        }

        public ITypedNonGenericEventDispatcher GetTypedDispatcher(Type eventType)
        {
            if (eventType == null)
                throw new ArgumentNullException(nameof(eventType));

            return _typedDispatchers.GetOrAdd(eventType, type =>
            {
                var wrapper = Activator.CreateInstance(_typedDispatcherType.MakeGenericType(eventType), _authorizationVerifyer, _serviceProvider);
                Debug.Assert(wrapper != null);
                return wrapper as ITypedNonGenericEventDispatcher;
            });
        }

        private bool TryGetTypedDispatcher(Type type, out ITypedNonGenericEventDispatcher typedDispatcher)
        {
            Debug.Assert(type != null);

            var result = _typedDispatchers.TryGetValue(type, out typedDispatcher);

            Debug.Assert(!result || typedDispatcher != null);
            Debug.Assert(!result || typedDispatcher.EventType == type);
            return result;
        }

        /// <summary>
        /// Asynchronously dispatches an event.
        /// </summary>
        /// <typeparam name="TEvent">The type of event.</typeparam>
        /// <param name="evt">The event to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="evt"/> is null.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if the access is unauthorized.</exception>
        public Task NotifyAsync<TEvent>(TEvent evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            if (!_authorizationVerifyer.AuthorizeEventNotification(evt)) // TODO: This is done twice. One time here and in the typed dispatcher.
                throw new UnauthorizedAccessException();

            // We cannot just call the typed dispatcher for TEvent here, like the other dispatchers do.
            // The reason is the different semantic. An event can be dispatched to multiple receivers.
            // A receiver can be registered for an event type that is lower in the inheritence hierarchy than TEvent.
            // So we have to dispatch the event to all typed dispatchers that are available!! the whole type hierarchy
            // till we reach System.Object.

            return NotifyAsync(typeof(TEvent), evt);
        }

        public Task NotifyAsync(Type eventType, object evt)
        {
            if (eventType == null)
                throw new ArgumentNullException(nameof(eventType));

            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            // Note: The access check is done by the typed dispatcher.

            var currType = eventType;
            var tasks = new List<Task>();

            do
            {
                Debug.Assert(currType != null);

                // This is checked by the typed dispatcher
                // Debug.Assert(currType.GetTypeInfo().IsAssignableFrom(evt.GetType()));

                if (TryGetTypedDispatcher(currType, out var dispatcher))
                {
                    tasks.Add(dispatcher.NotifyAsync(evt));
                }
            }
            while (!currType.GetTypeInfo().IsInterface &&
                   (currType = currType.GetTypeInfo().BaseType) != null);

            if (tasks.Count > 0)
            {
                return Task.WhenAll(tasks);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns a boolean value indicating whether registering the specified event handler is authorized.
        /// </summary>
        /// <typeparam name="TEvent">The type of event.</typeparam>
        /// <param name="eventHandlerFactory">The event handler that shall be registered.</param>
        /// <returns>True if registering <paramref name="eventHandlerFactory"/> is authorized, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="eventHandlerFactory"/> is null.</exception>
        public bool IsRegistrationAuthorized<TEvent>(IHandlerFactory<IEventHandler<TEvent>> eventHandlerFactory)
        {
            if (eventHandlerFactory == null)
                throw new ArgumentNullException(nameof(eventHandlerFactory));

            return _authorizationVerifyer.AuthorizeHandlerRegistry();
        }


        /// <summary>
        /// Returns a boolean value indicating whether dispatching the specified event handler is authorized.
        /// </summary>
        /// <typeparam name="TEvent">The type of event.</typeparam>
        /// <param name="evt">The event that shall be dispatched.</param>
        /// <returns>True if dispatching <paramref name="evt"/> is authorized, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="evt"/> is null.</exception>
        public bool IsDispatchAuthorized<TEvent>(TEvent evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            return _authorizationVerifyer.AuthorizeEventNotification(evt);
        }
    }

    /// <summary>
    /// Represents a typed event dispatcher that events to event handlers.
    /// </summary>
    /// <typeparam name="TEvent">The type of event.</typeparam>
    public sealed class EventDispatcher<TEvent> : IEventDispatcher<TEvent>, ISecureEventDispatcher<TEvent>, ITypedNonGenericEventDispatcher
    {
        private readonly IAsyncMultipleHandlerRegistry<IEventHandler<TEvent>> _handlerRegistry
            = new AsyncMultipleHandlerRegistry<IEventHandler<TEvent>>();
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventAuthorizationVerifyer _authorizationVerifyer;

        /// <summary>
        /// Creates a new instance of the <see cref="EventDispatcher{TEvent}"/> type.
        /// </summary>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> used to obtain services.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="serviceProvider"/> is null.</exception>
        public EventDispatcher(IServiceProvider serviceProvider) : this(EventAuthorizationVerifyer.Default, serviceProvider) { }

        /// <summary>
        /// Creates a new instance of the <see cref="EventDispatcher{TEvent}"/> type.
        /// </summary>
        /// <param name="authorizationVerifyer">An <see cref="IEventAuthorizationVerifyer"/> that controls authorization or <see cref="EventAuthorizationVerifyer.Default"/>.</param>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> used to obtain services.</param>
        /// <exception cref="ArgumentNullException">Thrown if either <paramref name="serviceProvider"/> or <paramref name="authorizationVerifyer"/> is null.</exception>
        public EventDispatcher(IEventAuthorizationVerifyer authorizationVerifyer, IServiceProvider serviceProvider) 
        {
            // Remark: If you modify/delete this constructors arguments, remember to also change the non-generic GetTypedDispatcher() method above

            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            if (authorizationVerifyer == null)
                throw new ArgumentNullException(nameof(authorizationVerifyer));

            _serviceProvider = serviceProvider;
            _authorizationVerifyer = authorizationVerifyer;
        }

        /// <summary>
        /// Asynchronously registers an event handler.
        /// </summary>
        /// <param name="eventHandlerFactory">The event handler to register.</param>
        /// <returns>
        /// A <see cref="IHandlerRegistration"/> representing the asynchronous operation.
        /// The <see cref="IHandlerRegistration"/> cancels the handler registration if completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="eventHandlerFactory"/> is null.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if the access is unauthorized.</exception>
        public Task<IHandlerRegistration<IEventHandler<TEvent>>> RegisterAsync(IHandlerFactory<IEventHandler<TEvent>> eventHandlerFactory) // TODO: Correct xml-comments
        {
            if (eventHandlerFactory == null)
                throw new ArgumentNullException(nameof(eventHandlerFactory));

            if (!_authorizationVerifyer.AuthorizeHandlerRegistry())
                throw new UnauthorizedAccessException();

            return HandlerRegistration.CreateRegistrationAsync(_handlerRegistry, eventHandlerFactory);
        }

        /// <summary>
        /// Asynchronously dispatches an event.
        /// </summary>
        /// <param name="evt">The event to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="evt"/> is null.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if the access is unauthorized.</exception>
        public Task NotifyAsync(TEvent evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            if (!_authorizationVerifyer.AuthorizeEventNotification(evt))
                throw new UnauthorizedAccessException();

            var handlers = _handlerRegistry.GetHandlerFactories();

            async Task InternalInvokeSingleHandler(IHandlerFactory<IEventHandler<TEvent>> handlerFactory)
            {
                Debug.Assert(handlerFactory != null);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var handler = handlerFactory.GetHandler(scope.ServiceProvider);

                    await handler.HandleAsync(evt);
                }
            }
            var tasks = handlers.Select(p => InternalInvokeSingleHandler(p)).ToList();

            if (tasks.Count > 0)
            {
                return Task.WhenAll(tasks);
            }

            return Task.CompletedTask;
        }

        public Task NotifyAsync(object evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            if (!(evt is TEvent typedEvent))
                throw new ArgumentException($"The event must be of type '{typeof(TEvent).FullName}' or a derived type.");

            return NotifyAsync(typedEvent);
        }

        Type ITypedNonGenericEventDispatcher.EventType => typeof(TEvent);

        /// <summary>
        /// Returns a boolean value indicating whether registering the specified event handler is authorized.
        /// </summary>
        /// <param name="eventHandlerFactory">The event handler that shall be registered.</param>
        /// <returns>True if registering <paramref name="eventHandlerFactory"/> is authorized, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="eventHandlerFactory"/> is null.</exception>
        public bool IsRegistrationAuthorized(IHandlerFactory<IEventHandler<TEvent>> eventHandlerFactory)
        {
            if (eventHandlerFactory == null)
                throw new ArgumentNullException(nameof(eventHandlerFactory));

            return _authorizationVerifyer.AuthorizeHandlerRegistry();
        }

        /// <summary>
        /// Returns a boolean value indicating whether dispatching the specified event handler is authorized.
        /// </summary>
        /// <param name="evt">The event that shall be dispatched.</param>
        /// <returns>True if dispatching <paramref name="evt"/> is authorized, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="evt"/> is null.</exception>
        public bool IsDispatchAuthorized(TEvent evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            return _authorizationVerifyer.AuthorizeEventNotification(evt);
        }
    }

    public sealed class EventAuthorizationVerifyer : IEventAuthorizationVerifyer
    {
        public static EventAuthorizationVerifyer Default { get; } = new EventAuthorizationVerifyer();

        private EventAuthorizationVerifyer() { }

        public bool AuthorizeHandlerRegistry()
        {
            return true;
        }

        public bool AuthorizeEventNotification<TEvent>(TEvent evt)
        {
            return true;
        }
    }
}
