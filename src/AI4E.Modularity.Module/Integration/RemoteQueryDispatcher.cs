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
using AI4E;
using AI4E.QueryResults;
using Microsoft.Extensions.DependencyInjection;

namespace AI4E.Modularity.Integration
{
    /// <summary>
    /// Represents a remote query dispatcher that dispatches queries to query handler.
    /// </summary>
    public sealed class RemoteQueryDispatcher : IRemoteQueryDispatcher, INonGenericRemoteQueryDispatcher
    {
        private static readonly Type _typedDispatcherType = typeof(RemoteQueryDispatcher<>);

        private readonly IQueryMessageTranslator _queryMessageTranslator;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<Type, ITypedNonGenericRemoteQueryDispatcher> _typedDispatchers
            = new ConcurrentDictionary<Type, ITypedNonGenericRemoteQueryDispatcher>();

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
        public Task<IHandlerRegistration<IQueryHandler<TQuery>>> RegisterAsync<TQuery>(IContextualProvider<IQueryHandler<TQuery>> queryHandlerFactory)
        {
            if (queryHandlerFactory == null)
                throw new ArgumentNullException(nameof(queryHandlerFactory));

            return GetTypedDispatcher<TQuery>().RegisterAsync(queryHandlerFactory);
        }

        #region GetTypedDispatcher

        /// <summary>
        /// Returns a typed query handler for the specified query and result type.
        /// </summary>
        /// <typeparam name="TQuery">The type of query.</typeparam>
        /// <returns>A typed query dispatcher.</returns>
        public IRemoteQueryDispatcher<TQuery> GetTypedDispatcher<TQuery>()
        {
            return _typedDispatchers.GetOrAdd(typeof(TQuery), p => new RemoteQueryDispatcher<TQuery>(_queryMessageTranslator, _serviceProvider)) as IRemoteQueryDispatcher<TQuery>;
        }

        IQueryDispatcher<TQuery> IQueryDispatcher.GetTypedDispatcher<TQuery>()
        {
            return GetTypedDispatcher<TQuery>();
        }

        /// <summary>
        /// Returns a non-generic typed remote query handler for the specified type of query and result.
        /// </summary>
        /// <param name="queryType">The type of query.</param>
        /// <returns>A non-generic typed remote query dispatcher.</returns>
        public ITypedNonGenericRemoteQueryDispatcher GetTypedDispatcher(Type queryType)
        {
            if (queryType == null)
                throw new ArgumentNullException(nameof(queryType));

            return _typedDispatchers.GetOrAdd(queryType, p =>
            {
                var result = Activator.CreateInstance(_typedDispatcherType.MakeGenericType(queryType), _queryMessageTranslator, _serviceProvider);

                Debug.Assert(result != null);

                return result as ITypedNonGenericRemoteQueryDispatcher;
            });
        }

        ITypedNonGenericQueryDispatcher INonGenericQueryDispatcher.GetTypedDispatcher(Type queryType)
        {
            return GetTypedDispatcher(queryType);
        }

        #endregion

        #region DispatchAsync

        /// <summary>
        /// Asynchronously dispatches a query.
        /// </summary>
        /// <typeparam name="TQuery">The type of query.</typeparam>
        /// <param name="query">The query to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
        public Task<IQueryResult> QueryAsync<TQuery>(TQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return GetTypedDispatcher<TQuery>().QueryAsync(query);
        }

        /// <summary>
        /// Asynchronously dispatches a query.
        /// </summary>
        /// <param name="queryType">The type of query.</param>
        /// <param name="query">The query to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Taske{Object}.Result"/> contains the query result or null if nothing was found.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if either <paramref name="queryType"/> or <paramref name="query"/> is null.</exception>
        /// <exception cref="ArgumentException">Throw is <paramref name="query"/> is not of type <paramref name="queryType"/> or a derived type.</exception>
        public Task<IQueryResult> QueryAsync(Type queryType, object query)
        {
            if (queryType == null)
                throw new ArgumentNullException(nameof(queryType));

            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return GetTypedDispatcher(queryType).QueryAsync(query);
        }

        /// <summary>
        /// Asynchronously dispatches a query locally only.
        /// </summary>
        /// <typeparam name="TQuery">The type of query.</typeparam>
        /// <param name="query">The query to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
        public Task<IQueryResult> LocalDispatchAsync<TQuery>(TQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return GetTypedDispatcher<TQuery>().LocalDispatchAsync(query);
        }

        /// <summary>
        /// Asynchronously dispatched a query locally only.
        /// </summary>
        /// <param name="queryType">The type of query.</param>
        /// <param name="query">The query to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if either <paramref name="queryType"/> or <paramref name="query"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="query"/> is not of type <paramref name="queryType"/> or a derived type.</exception>
        public Task<IQueryResult> LocalDispatchAsync(Type queryType, object query)
        {
            if (queryType == null)
                throw new ArgumentNullException(nameof(queryType));

            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return GetTypedDispatcher(queryType).LocalDispatchAsync(query);
        }

        #endregion

        // TODO: Xml-comment
        public void NotifyForwardingActive<TQuery>()
        {
            GetTypedDispatcher<TQuery>().NotifyForwardingActive();
        }

        // TODO: Xml-comment
        public void NotifyForwardingInactive<TQuery>()
        {
            GetTypedDispatcher<TQuery>().NotifyForwardingInactive();
        }

        void INonGenericRemoteQueryDispatcher.NotifyForwardingActive(Type queryType)
        {
            if (queryType == null)
                throw new ArgumentNullException(nameof(queryType));

            GetTypedDispatcher(queryType).NotifyForwardingActive();
        }

        void INonGenericRemoteQueryDispatcher.NotifyForwardingInactive(Type queryType)
        {
            if (queryType == null)
                throw new ArgumentNullException(nameof(queryType));

            GetTypedDispatcher(queryType).NotifyForwardingInactive();
        }
    }

    /// <summary>
    /// Represnts a typed remote query dispatcher that dispatched queries to query handler.
    /// </summary>
    /// <typeparam name="TQuery">The type of query.</typeparam>
    public sealed class RemoteQueryDispatcher<TQuery> : IRemoteQueryDispatcher<TQuery>, ITypedNonGenericRemoteQueryDispatcher
    {
        private readonly IQueryMessageTranslator _queryMessageTranslator;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAsyncSingleHandlerRegistry<IQueryHandler<TQuery>> _handlerRegistry;

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

            _handlerRegistry = new AsyncSingleHandlerRegistry<IQueryHandler<TQuery>>(new DispatchForwarding(this));
        }

        Type ITypedNonGenericQueryDispatcher.QueryType => typeof(TQuery);

        // TODO: Xml-comment
        public bool IsForwardingActive => _isForwardingActive;

        // TODO: Xml-comment
        public Task<IHandlerRegistration<IQueryHandler<TQuery>>> RegisterAsync(IContextualProvider<IQueryHandler<TQuery>> queryHandlerFactory)
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
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains the query result 
        /// or the default value of <typeparamref name="TResult"/> if nothing was found.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
        public Task<IQueryResult> QueryAsync(TQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            if (_isForwardingActive && _handlerRegistry.TryGetHandler(out var handler))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    return handler.ProvideInstance(scope.ServiceProvider).HandleAsync(query);
                }

            }

            return _queryMessageTranslator.DispatchAsync(query);
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
        public async Task<IQueryResult> QueryAsync(object query)
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
        public Task<IQueryResult> LocalDispatchAsync(TQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            if (_handlerRegistry.TryGetHandler(out var handler))
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        return handler.ProvideInstance(scope.ServiceProvider).HandleAsync(query);
                    }
                }
                catch (Exception exc)
                {
                    return Task.FromResult<IQueryResult>(new FailureQueryResult(exc.ToString()));
                }
            }

            return Task.FromResult<IQueryResult>(FailureQueryResult.UnknownFailure); // TODO
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
        public async Task<IQueryResult> LocalDispatchAsync(object query)
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
            private readonly RemoteQueryDispatcher<TQuery> _queryDispatcher;

            public DispatchForwarding(RemoteQueryDispatcher<TQuery> queryDispatcher)
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

                return _queryDispatcher._queryMessageTranslator.RegisterForwardingAsync<TQuery>();
            }

            public Task UnregisterForwardingAsync()
            {
                return _queryDispatcher._queryMessageTranslator.UnregisterForwardingAsync<TQuery>();
            }
        }
    }
}
