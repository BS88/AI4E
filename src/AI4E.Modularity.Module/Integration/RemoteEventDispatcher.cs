using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AI4E.Integration;
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

        public Task<IHandlerRegistration<IEventHandler<TEvent>>> RegisterAsync<TEvent>(IHandlerFactory<IEventHandler<TEvent>> eventHandlerFactory)
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

        public Task NotifyAsync<TEvent>(TEvent evt)
        {
            return NotifyAsync(typeof(TEvent), evt);
        }

        public Task NotifyAsync(Type eventType, object evt)
        {
            if (eventType == null)
                throw new ArgumentNullException(nameof(eventType));

            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            var currType = eventType;
            var tasks = new List<Task>();

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

            if (tasks.Count > 0)
            {
                return Task.WhenAll(tasks);
            }

            return Task.CompletedTask;
        }

        public Task RemoteDispatchAsync<TEvent>(TEvent evt)
        {
            return GetTypedDispatcher<TEvent>().RemoteDispatchAsync(evt);
        }

        public Task RemoteDispatchAsync(Type eventType, object evt)
        {
            if (eventType == null)
                throw new ArgumentNullException(nameof(eventType));

            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            return GetTypedDispatcher(eventType).RemoteDispatchAsync(evt);
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

        public Task<IHandlerRegistration<IEventHandler<TEvent>>> RegisterAsync(IHandlerFactory<IEventHandler<TEvent>> eventHandlerFactory)
        {
            if (eventHandlerFactory == null)
                throw new ArgumentNullException(nameof(eventHandlerFactory));

            return HandlerRegistration.CreateRegistrationAsync(_handlerRegistry, eventHandlerFactory);
        }

        public Task NotifyAsync(TEvent evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            if (IsForwardingActive)
            {
                return RemoteDispatchAsync(evt);
            }

            return _eventMessageTranslator.DispatchAsync(evt);
        }

        public Task NotifyAsync(object evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            if (!(evt is TEvent typedEvent))
            {
                throw new ArgumentException("The argument is not of the specified event type or a derived type.", nameof(evt));
            }

            return NotifyAsync(typedEvent);
        }

        public Task RemoteDispatchAsync(TEvent evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            var tasks = _handlerRegistry.GetHandlerFactories().Select(handler =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    return handler.GetHandler(scope.ServiceProvider).HandleAsync(evt);
                }
            }).ToArray();

            if (tasks.Length == 0)
                return Task.CompletedTask;

            return Task.WhenAll(tasks);
        }

        public Task RemoteDispatchAsync(object evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            if (!(evt is TEvent typedEvent))
            {
                throw new ArgumentException("The argument is not of the specified event type or a derived type.", nameof(evt));
            }

            return RemoteDispatchAsync(typedEvent);
        }

        public void NotifyForwardingActive()
        {
            _forwardingActive = true;
        }

        public void NotifyForwardingInactive()
        {
            _forwardingActive = false;
        }

        Type ITypedNonGenericEventDispatcher.EventType => typeof(TEvent);

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
