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
        private static readonly Type _typedDispatcherType = typeof(HostQueryDispatcher<,>);

        private readonly ConcurrentDictionary<(Type queryType, Type resultType), ITypedHostQueryDispatcher> _typedDispatcher
            = new ConcurrentDictionary<(Type queryType, Type resultType), ITypedHostQueryDispatcher>();
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

        public Task RegisterForwardingAsync(Type queryType, Type resultType)
        {
            if (queryType == null)
                throw new ArgumentNullException(nameof(queryType));

            if (resultType == null)
                throw new ArgumentNullException(nameof(resultType));

            return GetTypedDispatcher(queryType, resultType).RegisterForwardingAsync();
        }

        public Task UnregisterForwardingAsync(Type queryType, Type resultType)
        {
            if (queryType == null)
                throw new ArgumentNullException(nameof(queryType));

            if (resultType == null)
                throw new ArgumentNullException(nameof(resultType));

            return GetTypedDispatcher(queryType, resultType).UnregisterForwardingAsync();
        }

        public Task<object> DispatchAsync(Type queryType, Type resultType, object query)
        {
            if (queryType == null)
                throw new ArgumentNullException(nameof(queryType));

            if (resultType == null)
                throw new ArgumentNullException(nameof(resultType));

            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return GetTypedDispatcher(queryType, resultType).DispatchAsync(query);
        }

        public ITypedHostQueryDispatcher GetTypedDispatcher(Type queryType, Type resultType)
        {
            if (queryType == null)
                throw new ArgumentNullException(nameof(queryType));

            if (resultType == null)
                throw new ArgumentNullException(nameof(resultType));

            return _typedDispatcher.GetOrAdd((queryType, resultType), p =>
            {
                var dispatcher = Activator.CreateInstance(_typedDispatcherType.MakeGenericType(queryType, resultType), _queryDispatcher, _messageEndPoint);

                Debug.Assert(dispatcher != null);

                return dispatcher as ITypedHostQueryDispatcher;
            });
        }
    }

    public sealed class HostQueryDispatcher<TQuery, TResult> : ITypedHostQueryDispatcher
    {
        private readonly IQueryDispatcher<TQuery, TResult> _queryDispatcher;
        private readonly AsyncLock _lock = new AsyncLock();
        private readonly IMessageEndPoint _messageEndPoint;
        private  IHandlerRegistration<IQueryHandler<TQuery, TResult>> _proxyRegistration;

        public HostQueryDispatcher(IQueryDispatcher queryDispatcher, IMessageEndPoint messageEndPoint)
        {
            if (queryDispatcher == null)
                throw new ArgumentNullException(nameof(queryDispatcher));

            if (messageEndPoint == null)
                throw new ArgumentNullException(nameof(messageEndPoint));

            _queryDispatcher = queryDispatcher.GetTypedDispatcher<TQuery, TResult>();
            _messageEndPoint = messageEndPoint;
        }

        public HostQueryDispatcher(IQueryDispatcher<TQuery, TResult> queryDispatcher, IMessageEndPoint messageEndPoint)
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
                IHandlerFactory<IQueryHandler<TQuery, TResult>> proxy;

                if (_proxyRegistration != null)
                {
                    proxy = _proxyRegistration.Handler;
                }
                else
                {
                    proxy = new QueryHandlerProxy<TQuery, TResult>(_messageEndPoint);
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

        public async Task<object> DispatchAsync(object query)
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

        Type ITypedHostQueryDispatcher.ResultType => typeof(TResult);
    }
}
