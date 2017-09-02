using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AI4E.Integration;
using Nito.AsyncEx;

namespace AI4E.Modularity.Integration
{
    public sealed class HostQueryDispatcher : IHostQueryDispatcher
    {
        private static readonly Type _typedDispatcherType = typeof(HostQueryDispatcher<>);

        private readonly ConcurrentDictionary<Type, ITypedHostQueryDispatcher> _typedDispatcher
            = new ConcurrentDictionary<Type, ITypedHostQueryDispatcher>();
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly IMessageEndPoint _messageEndPoint;

        public HostQueryDispatcher(IQueryDispatcher queryDispatcher, IMessageEndPoint messageEndPoint)
        {
            if (queryDispatcher == null)
                throw new ArgumentNullException(nameof(queryDispatcher));

            if (messageEndPoint == null)
                throw new ArgumentNullException(nameof(messageEndPoint));

            _queryDispatcher = queryDispatcher;
            _messageEndPoint = messageEndPoint;
        }

        public Task RegisterForwardingAsync(Type queryType)
        {
            if (queryType == null)
                throw new ArgumentNullException(nameof(queryType));

            return GetTypedDispatcher(queryType).RegisterForwardingAsync();
        }

        public Task UnregisterForwardingAsync(Type queryType)
        {
            if (queryType == null)
                throw new ArgumentNullException(nameof(queryType));

            return GetTypedDispatcher(queryType).UnregisterForwardingAsync();
        }

        public Task<IQueryResult> DispatchAsync(Type queryType, object query)
        {
            if (queryType == null)
                throw new ArgumentNullException(nameof(queryType));

            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return GetTypedDispatcher(queryType).DispatchAsync(query);
        }

        public ITypedHostQueryDispatcher GetTypedDispatcher(Type queryType)
        {
            if (queryType == null)
                throw new ArgumentNullException(nameof(queryType));

            return _typedDispatcher.GetOrAdd(queryType, p =>
            {
                var dispatcher = Activator.CreateInstance(_typedDispatcherType.MakeGenericType(queryType), _queryDispatcher, _messageEndPoint);

                Debug.Assert(dispatcher != null);

                return dispatcher as ITypedHostQueryDispatcher;
            });
        }
    }

    public sealed class HostQueryDispatcher<TQuery> : ITypedHostQueryDispatcher
    {
        private readonly IQueryDispatcher<TQuery> _queryDispatcher;
        private readonly AsyncLock _lock = new AsyncLock();
        private readonly IMessageEndPoint _messageEndPoint;
        private IHandlerRegistration<IQueryHandler<TQuery>> _proxyRegistration;

        public HostQueryDispatcher(IQueryDispatcher queryDispatcher, IMessageEndPoint messageEndPoint)
        {
            if (queryDispatcher == null)
                throw new ArgumentNullException(nameof(queryDispatcher));

            if (messageEndPoint == null)
                throw new ArgumentNullException(nameof(messageEndPoint));

            _queryDispatcher = queryDispatcher.GetTypedDispatcher<TQuery>();
            _messageEndPoint = messageEndPoint;
        }

        public HostQueryDispatcher(IQueryDispatcher<TQuery> queryDispatcher, IMessageEndPoint messageEndPoint)
        {
            if (queryDispatcher == null)
                throw new ArgumentNullException(nameof(queryDispatcher));

            if (messageEndPoint == null)
                throw new ArgumentNullException(nameof(messageEndPoint));

            _queryDispatcher = queryDispatcher;
            _messageEndPoint = messageEndPoint;
        }

        public async Task RegisterForwardingAsync()
        {
            using (await _lock.LockAsync())
            {
                IHandlerProvider<IQueryHandler<TQuery>> proxy;

                if (_proxyRegistration != null)
                {
                    proxy = _proxyRegistration.Handler;
                }
                else
                {
                    proxy = new QueryHandlerProxy<TQuery>(_messageEndPoint);
                }

                _proxyRegistration = await _queryDispatcher.RegisterAsync(proxy);
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

        public async Task<IQueryResult> DispatchAsync(object query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            if (!(query is TQuery typedQuery))
            {
                throw new ArgumentException("The argument is not of the specified query type or a derived type.", nameof(query));
            }

            return await _queryDispatcher.QueryAsync(typedQuery);
        }

        Type ITypedHostQueryDispatcher.QueryType => typeof(TQuery);
    }
}
