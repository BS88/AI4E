/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        RemoteQueryDispatcher.cs 
 * Types:           (1) AI4E.Modularity.Integration.RemoteQueryDispatcher
 *                  (2) AI4E.Modularity.Integration.RemoteQueryDispatcher'2
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   01.07.2017 
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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using AI4E.Async;
using AI4E.Integration;
using Microsoft.Extensions.DependencyInjection;

namespace AI4E.Modularity.Integration
{
    /// <summary>
    /// Represents a remote query dispatcher that dispatches queries to query handler.
    /// </summary>
    public sealed class RemoteQueryDispatcher : IRemoteQueryDispatcher, INonGenericRemoteQueryDispatcher
    {
        private static readonly Type _typedDispatcherType = typeof(RemoteQueryDispatcher<,>);

        private readonly IQueryMessageTranslator _queryMessageTranslator;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<(Type queryType, Type commandType), ITypedNonGenericRemoteQueryDispatcher> _typedDispatchers
            = new ConcurrentDictionary<(Type queryType, Type commandType), ITypedNonGenericRemoteQueryDispatcher>();

        /// <summary>
        /// Creates a new instance of the <see cref="RemoteQueryDispatcher"/> type.
        /// </summary>
        /// <param name="queryMessageTranslator">The query translator that translates the messages sent to the host dispatcher.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> that is used to resolve services.</param>
        /// <exception cref="ArgumentNullException">Thrown is either <paramref name="queryMessageTranslator"/> or <paramref name="serviceProvider"/> is null.</exception>
        public RemoteQueryDispatcher(IQueryMessageTranslator queryMessageTranslator, IServiceProvider serviceProvider)
        {
            if (queryMessageTranslator == null)
                throw new ArgumentNullException(nameof(queryMessageTranslator));

            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            _queryMessageTranslator = queryMessageTranslator;
            _serviceProvider = serviceProvider;
        }

        // TODO: Xml-comment
        public Task<IHandlerRegistration<IQueryHandler<TQuery, TResult>>> RegisterAsync<TQuery, TResult>(IHandlerFactory<IQueryHandler<TQuery, TResult>> queryHandlerFactory)
        {
            if (queryHandlerFactory == null)
                throw new ArgumentNullException(nameof(queryHandlerFactory));

            return GetTypedDispatcher<TQuery, TResult>().RegisterAsync(queryHandlerFactory);
        }

        #region GetTypedDispatcher

        /// <summary>
        /// Returns a typed query handler for the specified query and result type.
        /// </summary>
        /// <typeparam name="TQuery">The type of query.</typeparam>
        /// <typeparam name="TResult">The type of result.</typeparam>
        /// <returns>A typed query dispatcher.</returns>
        public IRemoteQueryDispatcher<TQuery, TResult> GetTypedDispatcher<TQuery, TResult>()
        {
            return _typedDispatchers.GetOrAdd((typeof(TQuery), typeof(TResult)), p => new RemoteQueryDispatcher<TQuery, TResult>(_queryMessageTranslator, _serviceProvider)) as IRemoteQueryDispatcher<TQuery, TResult>;
        }

        IQueryDispatcher<TQuery, TResult> IQueryDispatcher.GetTypedDispatcher<TQuery, TResult>()
        {
            return GetTypedDispatcher<TQuery, TResult>();
        }

        /// <summary>
        /// Returns a non-generic typed remote query handler for the specified type of query and result.
        /// </summary>
        /// <param name="queryType">The type of query.</param>
        /// <param name="resultType">The type of result.</param>
        /// <returns>A non-generic typed remote query dispatcher.</returns>
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

        #endregion

        #region DispatchAsync

        /// <summary>
        /// Asynchronously dispatches a query.
        /// </summary>
        /// <typeparam name="TQuery">The type of query.</typeparam>
        /// <typeparam name="TResult">The type of result.</typeparam>
        /// <param name="query">The query to dispatch.</param>
        /// <returns>
        /// A covariant awaitable representing the asynchronous operation.
        /// The <see cref="ICovariantAwaitable{TResult}.Result"/> contains the query result 
        /// or the default value of <typeparamref name="TResult"/> if nothing was found.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
        public ICovariantAwaitable<TResult> QueryAsync<TQuery, TResult>(TQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return GetTypedDispatcher<TQuery, TResult>().QueryAsync(query);
        }

        /// <summary>
        /// Asynchronously dispatches a query.
        /// </summary>
        /// <param name="queryType">The type of query.</param>
        /// <param name="resultType">The type of result.</param>
        /// <param name="query">The query to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Taske{Object}.Result"/> contains the query result or null if nothing was found.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if any of <paramref name="queryType"/>, <paramref name="resultType"/> or <paramref name="query"/> is null.</exception>
        /// <exception cref="ArgumentException">Throw is <paramref name="query"/> is not of type <paramref name="queryType"/> or a derived type.</exception>
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

        /// <summary>
        /// Asynchronously dispatches a query locally only.
        /// </summary>
        /// <typeparam name="TQuery">The type of query.</typeparam>
        /// <typeparam name="TResult">The type of result.</typeparam>
        /// <param name="query">The query to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains the result of the query operation
        /// or the default value of <typeparamref name="TResult"/> if the query could not be handled.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
        public Task<TResult> LocalDispatchAsync<TQuery, TResult>(TQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return GetTypedDispatcher<TQuery, TResult>().LocalDispatchAsync(query);
        }

        /// <summary>
        /// Asynchronously dispatched a query locally only.
        /// </summary>
        /// <param name="queryType">The type of query.</param>
        /// <param name="resultType">The type of result.</param>
        /// <param name="query">The query to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains the result of the query operation
        /// or the default value of <paramref name="resultType"/> if the query could not be dispatched.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if any of <paramref name="queryType"/>, <paramref name="resultType"/> or <paramref name="query"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="query"/> is not of type <paramref name="queryType"/> or a derived type.</exception>
        public Task<object> LocalDispatchAsync(Type queryType, Type resultType, object query)
        {
            if (queryType == null)
                throw new ArgumentNullException(nameof(queryType));

            if (resultType == null)
                throw new ArgumentNullException(nameof(resultType));

            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return GetTypedDispatcher(queryType, resultType).LocalDispatchAsync(query);
        }

        #endregion

        // TODO: Xml-comment
        public void NotifyForwardingActive<TQuery, TResult>()
        {
            GetTypedDispatcher<TQuery, TResult>().NotifyForwardingActive();
        }

        // TODO: Xml-comment
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

    /// <summary>
    /// Represnts a typed remote query dispatcher that dispatched queries to query handler.
    /// </summary>
    /// <typeparam name="TQuery">The type of query.</typeparam>
    /// <typeparam name="TResult">The type of result.</typeparam>
    public sealed class RemoteQueryDispatcher<TQuery, TResult> : IRemoteQueryDispatcher<TQuery, TResult>, ITypedNonGenericRemoteQueryDispatcher
    {
        private readonly IQueryMessageTranslator _queryMessageTranslator;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAsyncSingleHandlerRegistry<IQueryHandler<TQuery, TResult>> _handlerRegistry;

        private bool _isForwardingActive;

        /// <summary>
        /// Creates a new instance of the <see cref="RemoteQueryDispatcher{TQuery, TResult}"/> type.
        /// </summary>
        /// <param name="queryMessageTranslator">The query translator that translates the messages sent to the host dispatcher.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> that is used to resolve services.</param>
        /// <exception cref="ArgumentNullException">Thrown is either <paramref name="queryMessageTranslator"/> or <paramref name="serviceProvider"/> is null.</exception>
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

        // TODO: Xml-comment
        public bool IsForwardingActive => _isForwardingActive;

        // TODO: Xml-comment
        public Task<IHandlerRegistration<IQueryHandler<TQuery, TResult>>> RegisterAsync(IHandlerFactory<IQueryHandler<TQuery, TResult>> queryHandlerFactory)
        {
            if (queryHandlerFactory == null)
                throw new ArgumentNullException(nameof(queryHandlerFactory));

            return HandlerRegistration.CreateRegistrationAsync(_handlerRegistry, queryHandlerFactory);
        }

        /// <summary>
        /// Asynchronously dispatches a query.
        /// </summary>
        /// <param name="query">The query to dispatch.</param>
        /// <returns>
        /// A covariant awaitable representing the asynchronous operation.
        /// The <see cref="ICovariantAwaitable{TResult}.Result"/> contains the query result 
        /// or the default value of <typeparamref name="TResult"/> if nothing was found.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
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

        /// <summary>
        /// Asynchronously dispatches a query. 
        /// </summary>
        /// <param name="query">The query to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{Object}.Result"/> contains the query result or null if nothing was found.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
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

        /// <summary>
        /// Asynchronously dispatches a query locally only.
        /// </summary>
        /// <param name="query">The query to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains the result of the query operation
        /// or the default value of <typeparamref name="TResult"/> if the query could not be dispatched.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
        public async Task<TResult> LocalDispatchAsync(TQuery query)
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

        /// <summary>
        /// Asynchronously dispatched a query locally only.
        /// </summary>
        /// <param name="queryType">The type of query.</param>
        /// <param name="resultType">The type of result.</param>
        /// <param name="query">The query to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains the result of the query operation
        /// or the default value of <paramref name="resultType"/> if the query could not be dispatched.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="query"/> is not of type <see cref="ITypedNonGenericQueryDispatcher.QueryType"/> or a derived type.</exception>
        public async Task<object> LocalDispatchAsync(object query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            if (!(query is TQuery typedQuery))
            {
                throw new ArgumentException("The argument is not of the specified query type or a derived type.", nameof(query));
            }

            return await LocalDispatchAsync(typedQuery);
        }

        // TODO: Xml-comment
        public void NotifyForwardingActive()
        {
            _isForwardingActive = true;
        }

        // TODO: Xml-comment
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
