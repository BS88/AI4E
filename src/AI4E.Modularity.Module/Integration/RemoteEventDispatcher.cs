using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AI4E.Integration;
using AI4E.Integration.EventResults;
using Microsoft.Extensions.DependencyInjection;

namespace AI4E.Modularity.Integration
{
    public sealed class RemoteEventDispatcher : IRemoteEventDispatcher, INonGenericRemoteEventDispatcher
    {
        private static readonly Type _typedDispatcherType = typeof(RemoteEventDispatcher<>);

        private readonly IEventMessageTranslator _eventMessageTranslator;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<Type, ITypedNonGenericRemoteEventDispatcher> _typedDispatchers
            = new ConcurrentDictionary<Type, ITypedNonGenericRemoteEventDispatcher>();

        public RemoteEventDispatcher(IEventMessageTranslator eventMessageTranslator, IServiceProvider serviceProvider)
        {
            if (eventMessageTranslator == null)
                throw new ArgumentNullException(nameof(eventMessageTranslator));

            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            _eventMessageTranslator = eventMessageTranslator;
            _serviceProvider = serviceProvider;
        }

        public Task<IHandlerRegistration<IEventHandler<TEvent>>> RegisterAsync<TEvent>(IContextualProvider<IEventHandler<TEvent>> eventHandlerFactory)
        {
            if (eventHandlerFactory == null)
                throw new ArgumentNullException(nameof(eventHandlerFactory));

            return GetTypedDispatcher<TEvent>().RegisterAsync(eventHandlerFactory);
        }

        public IRemoteEventDispatcher<TEvent> GetTypedDispatcher<TEvent>()
        {
            return _typedDispatchers.GetOrAdd(typeof(TEvent), p => new RemoteEventDispatcher<TEvent>(_eventMessageTranslator, _serviceProvider)) as IRemoteEventDispatcher<TEvent>;
        }

        IEventDispatcher<TEvent> IEventDispatcher.GetTypedDispatcher<TEvent>()
        {
            return GetTypedDispatcher<TEvent>();
        }

        public ITypedNonGenericRemoteEventDispatcher GetTypedDispatcher(Type eventType)
        {
            if (eventType == null)
                throw new ArgumentNullException(nameof(eventType));

            return _typedDispatchers.GetOrAdd(eventType, type =>
            {
                var result = Activator.CreateInstance(_typedDispatcherType.MakeGenericType(eventType), _eventMessageTranslator, _serviceProvider);

                Debug.Assert(result != null);

                return result as ITypedNonGenericRemoteEventDispatcher;
            });
        }

        ITypedNonGenericEventDispatcher INonGenericEventDispatcher.GetTypedDispatcher(Type eventType)
        {
            return GetTypedDispatcher(eventType);
        }

        public Task<IAggregateEventResult> NotifyAsync<TEvent>(TEvent evt)
        {
            return NotifyAsync(typeof(TEvent), evt);
        }

        public async Task<IAggregateEventResult> NotifyAsync(Type eventType, object evt)
        {
            if (eventType == null)
                throw new ArgumentNullException(nameof(eventType));

            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            var currType = eventType;
            var tasks = new List<Task<IAggregateEventResult>>();

            do
            {
                Debug.Assert(currType != null);

                // This is checked by the typed dispatcher
                // Debug.Assert(currType.GetTypeInfo().IsAssignableFrom(evt.GetType()));

                var dispatcher = GetTypedDispatcher(currType);

                tasks.Add(dispatcher.NotifyAsync(evt));

                if (!dispatcher.IsForwardingActive)
                {
                    break;
                }
            }
            while (!currType.IsInterface &&
                   (currType = currType.BaseType) != null);

            return new AggregateEventResult(await Task.WhenAll(tasks));
        }

        public Task<IAggregateEventResult> LocalDispatchAsync<TEvent>(TEvent evt)
        {
            return GetTypedDispatcher<TEvent>().LocalDispatchAsync(evt);
        }

        public Task<IAggregateEventResult> LocalDispatchAsync(Type eventType, object evt)
        {
            if (eventType == null)
                throw new ArgumentNullException(nameof(eventType));

            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            return GetTypedDispatcher(eventType).LocalDispatchAsync(evt);
        }

        public void NotifyForwardingActive<TEvent>()
        {
            GetTypedDispatcher<TEvent>().NotifyForwardingActive();
        }

        public void NotifyForwardingInactive<TEvent>()
        {
            GetTypedDispatcher<TEvent>().NotifyForwardingInactive();
        }

        void INonGenericRemoteEventDispatcher.NotifyForwardingActive(Type eventType)
        {
            if (eventType == null)
                throw new ArgumentNullException(nameof(eventType));

            GetTypedDispatcher(eventType).NotifyForwardingActive();
        }

        void INonGenericRemoteEventDispatcher.NotifyForwardingInactive(Type eventType)
        {
            if (eventType == null)
                throw new ArgumentNullException(nameof(eventType));

            GetTypedDispatcher(eventType).NotifyForwardingInactive();
        }
    }

    public sealed class RemoteEventDispatcher<TEvent> : IRemoteEventDispatcher<TEvent>, ITypedNonGenericRemoteEventDispatcher
    {
        private readonly IEventMessageTranslator _eventMessageTranslator;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAsyncMultipleHandlerRegistry<IEventHandler<TEvent>> _handlerRegistry;

        private bool _forwardingActive;

        public RemoteEventDispatcher(IEventMessageTranslator eventMessageTranslator, IServiceProvider serviceProvider)
        {
            if (eventMessageTranslator == null)
                throw new ArgumentNullException(nameof(eventMessageTranslator));

            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            _eventMessageTranslator = eventMessageTranslator;
            _serviceProvider = serviceProvider;

            _handlerRegistry = new AsyncMultipleHandlerRegistry<IEventHandler<TEvent>>(new DispatchForwarding(this));
        }

        public bool IsForwardingActive => _forwardingActive;

        Type ITypedNonGenericEventDispatcher.EventType => typeof(TEvent);

        public Task<IHandlerRegistration<IEventHandler<TEvent>>> RegisterAsync(IContextualProvider<IEventHandler<TEvent>> eventHandlerFactory)
        {
            if (eventHandlerFactory == null)
                throw new ArgumentNullException(nameof(eventHandlerFactory));

            return HandlerRegistration.CreateRegistrationAsync(_handlerRegistry, eventHandlerFactory);
        }

        public Task<IAggregateEventResult> NotifyAsync(TEvent evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            if (IsForwardingActive)
            {
                return LocalDispatchAsync(evt);
            }

            return _eventMessageTranslator.DispatchAsync(evt);
        }

        public Task<IAggregateEventResult> NotifyAsync(object evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            if (!(evt is TEvent typedEvent))
            {
                throw new ArgumentException("The argument is not of the specified event type or a derived type.", nameof(evt));
            }

            return NotifyAsync(typedEvent);
        }

        public async Task<IAggregateEventResult> LocalDispatchAsync(TEvent evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            return new AggregateEventResult(await Task.WhenAll(_handlerRegistry.GetHandlerFactories().Select(handler => NotifySingleHandlerAsync(handler, evt)).ToArray()));
        }

        // TODO: This is a copy of the original in EventDispatcher.cs
        private async Task<IEventResult> NotifySingleHandlerAsync(IContextualProvider<IEventHandler<TEvent>> handlerFactory, TEvent evt)
        {
            Debug.Assert(handlerFactory != null);

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    return await handlerFactory.GetHandler(scope.ServiceProvider).HandleAsync(evt);
                }
            }
            catch (Exception exc)
            {
                return new FailureEventResult(exc.ToString()); // TODO
            }
        }

        public Task<IAggregateEventResult> LocalDispatchAsync(object evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            if (!(evt is TEvent typedEvent))
            {
                throw new ArgumentException("The argument is not of the specified event type or a derived type.", nameof(evt));
            }

            return LocalDispatchAsync(typedEvent);
        }

        public void NotifyForwardingActive()
        {
            _forwardingActive = true;
        }

        public void NotifyForwardingInactive()
        {
            _forwardingActive = false;
        }

        private sealed class DispatchForwarding : IDispatchForwarding
        {
            private readonly RemoteEventDispatcher<TEvent> _eventDispatcher;

            public DispatchForwarding(RemoteEventDispatcher<TEvent> eventDispatcher)
            {
                Debug.Assert(eventDispatcher != null);

                _eventDispatcher = eventDispatcher;
            }

            public Task RegisterForwardingAsync()
            {
                if (_eventDispatcher._forwardingActive)
                    return Task.CompletedTask;

                return _eventDispatcher._eventMessageTranslator.RegisterForwardingAsync<TEvent>();
            }

            public Task UnregisterForwardingAsync()
            {
                return _eventDispatcher._eventMessageTranslator.UnregisterForwardingAsync<TEvent>();
            }
        }
    }
}
