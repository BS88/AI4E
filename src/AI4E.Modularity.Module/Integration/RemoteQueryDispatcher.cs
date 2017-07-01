using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using AI4E.Async;
using AI4E.Integration;
using Microsoft.Extensions.DependencyInjection;

namespace AI4E.Modularity.Integration
{
    public sealed class RemoteQueryDispatcher : IRemoteQueryDispatcher, INonGenericRemoteQueryDispatcher
    {
        private static readonly Type _typedDispatcherType = typeof(RemoteQueryDispatcher<,>);

        private readonly IQueryMessageTranslator _queryMessageTranslator;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<(Type queryType, Type commandType), ITypedNonGenericRemoteQueryDispatcher> _typedDispatchers
            = new ConcurrentDictionary<(Type queryType, Type commandType), ITypedNonGenericRemoteQueryDispatcher>();

        public RemoteQueryDispatcher(IQueryMessageTranslator queryMessageTranslator, IServiceProvider serviceProvider)
        {
            if (queryMessageTranslator == null)
                throw new ArgumentNullException(nameof(queryMessageTranslator));

            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            _queryMessageTranslator = queryMessageTranslator;
            _serviceProvider = serviceProvider;
        }

        public Task<IHandlerRegistration<IQueryHandler<TQuery, TResult>>> RegisterAsync<TQuery, TResult>(IHandlerFactory<IQueryHandler<TQuery, TResult>> queryHandlerFactory)
        {
            if (queryHandlerFactory == null)
                throw new ArgumentNullException(nameof(queryHandlerFactory));

            return GetTypedDispatcher<TQuery, TResult>().RegisterAsync(queryHandlerFactory);
        }

        public IRemoteQueryDispatcher<TQuery, TResult> GetTypedDispatcher<TQuery, TResult>()
        {
            return _typedDispatchers.GetOrAdd((typeof(TQuery), typeof(TResult)), p => new RemoteQueryDispatcher<TQuery, TResult>(_queryMessageTranslator, _serviceProvider)) as IRemoteQueryDispatcher<TQuery, TResult>;
        }

        IQueryDispatcher<TQuery, TResult> IQueryDispatcher.GetTypedDispatcher<TQuery, TResult>()
        {
            return GetTypedDispatcher<TQuery, TResult>();
        }

        public ITypedNonGenericRemoteQueryDispatcher GetTypedDispatcher(Type queryType, Type resultType)
        {
            if (queryType == null)
                throw new ArgumentNullException(nameof(queryType));

            if (resultType == null)
                throw new ArgumentNullException(nameof(resultType));

            return _typedDispatchers.GetOrAdd((queryType, resultType), p =>
            {
                var result = Activator.CreateInstance(_typedDispatcherType.MakeGenericType(queryType, resultType), _queryMessageTranslator, _serviceProvider);

                Debug.Assert(result != null);

                return result as ITypedNonGenericRemoteQueryDispatcher;
            });
        }

        ITypedNonGenericQueryDispatcher INonGenericQueryDispatcher.GetTypedDispatcher(Type queryType, Type resultType)
        {
            return GetTypedDispatcher(queryType, resultType);
        }

        public ICovariantAwaitable<TResult> QueryAsync<TQuery, TResult>(TQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return GetTypedDispatcher<TQuery, TResult>().QueryAsync(query);
        }

        public Task<object> QueryAsync(Type queryType, Type resultType, object query)
        {
            if (queryType == null)
                throw new ArgumentNullException(nameof(queryType));

            if (resultType == null)
                throw new ArgumentNullException(nameof(resultType));

            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return GetTypedDispatcher(queryType, resultType).QueryAsync(query);
        }

        public Task<TResult> RemoteDispatchAsync<TQuery, TResult>(TQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return GetTypedDispatcher<TQuery, TResult>().RemoteDispatchAsync(query);
        }

        public Task<object> RemoteDispatchAsync(Type queryType, Type resultType, object query)
        {
            if (queryType == null)
                throw new ArgumentNullException(nameof(queryType));

            if (resultType == null)
                throw new ArgumentNullException(nameof(resultType));

            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return GetTypedDispatcher(queryType, resultType).RemoteDispatchAsync(query);
        }

        public void NotifyForwardingActive<TQuery, TResult>()
        {
            GetTypedDispatcher<TQuery, TResult>().NotifyForwardingActive();
        }

        public void NotifyForwardingInactive<TQuery, TResult>()
        {
            GetTypedDispatcher<TQuery, TResult>().NotifyForwardingInactive();
        }

        void INonGenericRemoteQueryDispatcher.NotifyForwardingActive(Type queryType, Type resultType)
        {
            if (queryType == null)
                throw new ArgumentNullException(nameof(queryType));

            if (resultType == null)
                throw new ArgumentNullException(nameof(resultType));

            GetTypedDispatcher(queryType, resultType).NotifyForwardingActive();
        }

        void INonGenericRemoteQueryDispatcher.NotifyForwardingInactive(Type queryType, Type resultType)
        {
            if (queryType == null)
                throw new ArgumentNullException(nameof(queryType));

            if (resultType == null)
                throw new ArgumentNullException(nameof(resultType));

            GetTypedDispatcher(queryType, resultType).NotifyForwardingInactive();
        }
    }

    public sealed class RemoteQueryDispatcher<TQuery, TResult> : IRemoteQueryDispatcher<TQuery, TResult>, ITypedNonGenericRemoteQueryDispatcher
    {
        private readonly IQueryMessageTranslator _queryMessageTranslator;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAsyncSingleHandlerRegistry<IQueryHandler<TQuery, TResult>> _handlerRegistry;

        private bool _isForwardingActive;

        public RemoteQueryDispatcher(IQueryMessageTranslator queryMessageTranslator, IServiceProvider serviceProvider)
        {
            if (queryMessageTranslator == null)
                throw new ArgumentNullException(nameof(queryMessageTranslator));

            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            _queryMessageTranslator = queryMessageTranslator;
            _serviceProvider = serviceProvider;

            _handlerRegistry = new AsyncSingleHandlerRegistry<IQueryHandler<TQuery, TResult>>(new DispatchForwarding(this));
        }

        Type ITypedNonGenericQueryDispatcher.QueryType => typeof(TQuery);

        Type ITypedNonGenericQueryDispatcher.ResultType => typeof(TResult);

        public bool IsForwardingActive => _isForwardingActive;

        public Task<IHandlerRegistration<IQueryHandler<TQuery, TResult>>> RegisterAsync(IHandlerFactory<IQueryHandler<TQuery, TResult>> queryHandlerFactory)
        {
            if (queryHandlerFactory == null)
                throw new ArgumentNullException(nameof(queryHandlerFactory));

            return HandlerRegistration.CreateRegistrationAsync(_handlerRegistry, queryHandlerFactory);
        }

        public ICovariantAwaitable<TResult> QueryAsync(TQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            if (_isForwardingActive && _handlerRegistry.TryGetHandler(out var handler))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    return handler.GetHandler(scope.ServiceProvider).HandleAsync(query);
                }
            }

            return CovariantAwaitable.FromTask(_queryMessageTranslator.DispatchAsync<TQuery, TResult>(query));
        }

        public async Task<object> QueryAsync(object query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            if (!(query is TQuery typedQuery))
            {
                throw new ArgumentException("The argument is not of the specified query type or a derived type.", nameof(query));
            }

            return await QueryAsync(typedQuery);
        }

        public async Task<TResult> RemoteDispatchAsync(TQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            if (_handlerRegistry.TryGetHandler(out var handler))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    return await handler.GetHandler(scope.ServiceProvider).HandleAsync(query); // TODO: Use Task in QueryDispatcher.QueryAsync and IQueryHandler.HandleAsync
                }
            }

            return default(TResult);
        }

        public async Task<object> RemoteDispatchAsync(object query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            if (!(query is TQuery typedQuery))
            {
                throw new ArgumentException("The argument is not of the specified query type or a derived type.", nameof(query));
            }

            return await RemoteDispatchAsync(typedQuery);
        }

        public void NotifyForwardingActive()
        {
            _isForwardingActive = true;
        }

        public void NotifyForwardingInactive()
        {
            _isForwardingActive = false;
        }

        private sealed class DispatchForwarding : IDispatchForwarding
        {
            private readonly RemoteQueryDispatcher<TQuery, TResult> _queryDispatcher;

            public DispatchForwarding(RemoteQueryDispatcher<TQuery, TResult> queryDispatcher)
            {
                Debug.Assert(queryDispatcher != null);

                _queryDispatcher = queryDispatcher;
            }

            public Task RegisterForwardingAsync()
            {
                if (_queryDispatcher._isForwardingActive)
                {
                    return Task.CompletedTask;
                }

                return _queryDispatcher._queryMessageTranslator.RegisterForwardingAsync<TQuery, TResult>();
            }

            public Task UnregisterForwardingAsync()
            {
                return _queryDispatcher._queryMessageTranslator.UnregisterForwardingAsync<TQuery, TResult>();
            }
        }
    }
}
