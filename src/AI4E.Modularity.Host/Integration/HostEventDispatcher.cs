using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AI4E;
using Nito.AsyncEx;

namespace AI4E.Modularity.Integration
{
    public sealed class HostEventDispatcher : IHostEventDispatcher
    {
        private static readonly Type _typedDispatcherType = typeof(HostEventDispatcher<>);

        private readonly IEventDispatcher _eventDispatcher;
        private readonly IMessageEndPoint _messageEndPoint;
        private readonly ConcurrentDictionary<Type, ITypedHostEventDispatcher> _typedDispatcher = new ConcurrentDictionary<Type, ITypedHostEventDispatcher>();

        public HostEventDispatcher(IEventDispatcher eventDispatcher, IMessageEndPoint messageEndPoint)
        {
            if (eventDispatcher == null)
                throw new ArgumentNullException(nameof(eventDispatcher));

            if (messageEndPoint == null)
                throw new ArgumentNullException(nameof(messageEndPoint));

            _eventDispatcher = eventDispatcher;
            _messageEndPoint = messageEndPoint;
        }

        public Task RegisterForwardingAsync(Type eventType)
        {
            if (eventType == null)
                throw new ArgumentNullException(nameof(eventType));

            return GetTypedDispatcher(eventType).RegisterForwardingAsync();
        }

        public Task UnregisterForwardingAsync(Type eventType)
        {
            if (eventType == null)
                throw new ArgumentNullException(nameof(eventType));

            return GetTypedDispatcher(eventType).UnregisterForwardingAsync();
        }

        public Task<IAggregateEventResult> DispatchAsync(Type eventType, object evt)
        {
            if (eventType == null)
                throw new ArgumentNullException(nameof(eventType));

            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            return GetTypedDispatcher(eventType).DispatchAsync(evt);
        }

        public ITypedHostEventDispatcher GetTypedDispatcher(Type eventType)
        {
            if (eventType == null)
                throw new ArgumentNullException(nameof(eventType));

            return _typedDispatcher.GetOrAdd(eventType, type =>
            {
                var dispatcher = Activator.CreateInstance(_typedDispatcherType.MakeGenericType(eventType), _eventDispatcher, _messageEndPoint);

                Debug.Assert(dispatcher != null);

                return dispatcher as ITypedHostEventDispatcher;
            });
        }
    }

    public sealed class HostEventDispatcher<TEvent> : ITypedHostEventDispatcher
    {
        private readonly IEventDispatcher<TEvent> _eventDispatcher;
        private readonly AsyncLock _lock = new AsyncLock();
        private readonly IMessageEndPoint _messageEndPoint;
        private IHandlerRegistration<IEventHandler<TEvent>> _proxyRegistration;

        public HostEventDispatcher(IEventDispatcher<TEvent> eventDispatcher, IMessageEndPoint messageEndPoint)
        {
            if (eventDispatcher == null)
                throw new ArgumentNullException(nameof(eventDispatcher));

            if (messageEndPoint == null)
                throw new ArgumentNullException(nameof(messageEndPoint));

            _eventDispatcher = eventDispatcher;
            _messageEndPoint = messageEndPoint;
        }

        public HostEventDispatcher(IEventDispatcher eventDispatcher, IMessageEndPoint messageEndPoint)
        {
            if (eventDispatcher == null)
                throw new ArgumentNullException(nameof(eventDispatcher));

            if (messageEndPoint == null)
                throw new ArgumentNullException(nameof(messageEndPoint));

            _eventDispatcher = eventDispatcher.GetTypedDispatcher<TEvent>();
            _messageEndPoint = messageEndPoint;
        }

        public async Task RegisterForwardingAsync()
        {
            using (await _lock.LockAsync())
            {
                IContextualProvider<IEventHandler<TEvent>> proxy;

                if (_proxyRegistration != null)
                {
                    proxy = _proxyRegistration.Handler;
                }
                else
                {
                    proxy = new EventHandlerProxy<TEvent>(_messageEndPoint);
                }

                _proxyRegistration = await _eventDispatcher.RegisterAsync(proxy);
            }
        }

        public async Task UnregisterForwardingAsync()
        {
            using (await _lock.LockAsync())
            {
                if (_proxyRegistration != null)
                {
                    _proxyRegistration.Complete();
                    await _proxyRegistration.Completion;
                    _proxyRegistration = null;
                }
            }
        }

        public Task<IAggregateEventResult> DispatchAsync(object evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            if (!(evt is TEvent typedEvent))
            {
                throw new ArgumentException("The argument is not of the specified event type or a derived type.", nameof(evt));
            }

            return _eventDispatcher.NotifyAsync(typedEvent);
        }

        Type ITypedHostEventDispatcher.EventType => typeof(TEvent);
    }
}
