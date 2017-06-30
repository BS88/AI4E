/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        QueryDispatcher.cs 
 * Types:           (1) AI4E.Integration.QueryDispatcher
 *                  (2) AI4E.Integration.QueryDispatcher'2
 *                  (3) AI4E.Integration.QueryAuthorizationVerifyer
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   01.06.2017 
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
using System.Collections.Concurrent;
using System.Threading.Tasks;
using AI4E.Async;
using Microsoft.Extensions.DependencyInjection;

namespace AI4E.Integration
{
    /// <summary>
    /// Represents a query dispatcher that dispatches queries to query handlers.
    /// </summary>
    public sealed class QueryDispatcher : IQueryDispatcher, ISecureQueryDispatcher, INonGenericQueryDispatcher
    {
        private readonly ConcurrentDictionary<(Type, Type), object> _typedDispatcher = new ConcurrentDictionary<(Type, Type), object>();
        private readonly IServiceProvider _serviceProvider;
        private readonly IQueryAuthorizationVerifyer _authorizationVerifyer;

        /// <summary>
        /// Creates a new instance of the <see cref="QueryDispatcher"/> type.
        /// </summary>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> used to obtain services.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="serviceProvider"/> is null.</exception>
        public QueryDispatcher(IServiceProvider serviceProvider) : this(serviceProvider, QueryAuthorizationVerifyer.Default) { }

        /// <summary>
        /// Creates a new instance of the <see cref="QueryDispatcher"/> type.
        /// </summary>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> used to obtain services.</param>
        /// <param name="authorizationVerifyer">A <see cref="IQueryAuthorizationVerifyer"/> that controls authorization or <see cref="QueryAuthorizationVerifyer.Default"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if either <paramref name="serviceProvider"/> or <paramref name="authorizationVerifyer"/> is null.</exception>
        public QueryDispatcher(IServiceProvider serviceProvider, IQueryAuthorizationVerifyer authorizationVerifyer)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            if (authorizationVerifyer == null)
                throw new ArgumentNullException(nameof(authorizationVerifyer));

            _serviceProvider = serviceProvider;
            _authorizationVerifyer = authorizationVerifyer;
        }

        /// <summary>
        /// Asynchronously registers a query handler.
        /// </summary>
        /// <typeparam name="TQuery">The type of query.</typeparam>
        /// <typeparam name="TResult">The type of result.</typeparam>
        /// <param name="queryHandlerFactory">The query handler to register.</param>
        /// <returns>
        /// A <see cref="IHandlerRegistration"/> representing the asynchronous operation.
        /// The <see cref="IHandlerRegistration"/> cancels the handler registration if completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="queryHandlerFactory"/> is null.</exception>
        public Task<IHandlerRegistration<IQueryHandler<TQuery, TResult>>> RegisterAsync<TQuery, TResult>(IHandlerFactory<IQueryHandler<TQuery, TResult>> queryHandlerFactory) // TODO: Correct xml-comments
        {
            if (queryHandlerFactory == null)
                throw new ArgumentNullException(nameof(queryHandlerFactory));

            if (!_authorizationVerifyer.AuthorizeHandlerRegistry())
                throw new UnauthorizedAccessException();

            return GetTypedDispatcher<TQuery, TResult>().RegisterAsync(queryHandlerFactory);
        }

        /// <summary>
        /// Returns a typed query dispatcher for the specified query and result types.
        /// </summary>
        /// <typeparam name="TQuery">The type of query.</typeparam>
        /// <typeparam name="TResult">The type of result.</typeparam>
        /// <returns>
        /// A typed query dispatcher for queries of type <typeparamref name="TQuery"/> 
        /// and results of type <typeparamref name="TResult"/>.
        /// </returns>
        public IQueryDispatcher<TQuery, TResult> GetTypedDispatcher<TQuery, TResult>()
        {
            return _typedDispatcher.GetOrAdd((typeof(TQuery), typeof(TResult)),
                                             t => new QueryDispatcher<TQuery, TResult>(_serviceProvider, _authorizationVerifyer))
                                             as IQueryDispatcher<TQuery, TResult>;
        }

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
        /// <exception cref="UnauthorizedAccessException">Thrown if the access is unauthorized.</exception>
        public ICovariantAwaitable<TResult> QueryAsync<TQuery, TResult>(TQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            if (!_authorizationVerifyer.AuthorizeQuery<TQuery, TResult>(query))
                throw new UnauthorizedAccessException();

            return GetTypedDispatcher<TQuery, TResult>().QueryAsync(query);
        }

        /// <summary>
        /// Returns a boolean value indicating whether registering the specified query handler is authorized.
        /// </summary>
        /// <typeparam name="TQuery">The type of query.</typeparam>
        /// <typeparam name="TResult">The type of result.</typeparam>
        /// <param name="queryHandlerFactory">The query handler that shall be registered.</param>
        /// <returns>True if registering <paramref name="queryHandlerFactory"/> is authorized, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="queryHandlerFactory"/> is null.</exception>
        public bool IsRegistrationAuthorized<TQuery, TResult>(IHandlerFactory<IQueryHandler<TQuery, TResult>> queryHandlerFactory)
        {
            if (queryHandlerFactory == null)
                throw new ArgumentNullException(nameof(queryHandlerFactory));

            return _authorizationVerifyer.AuthorizeHandlerRegistry();
        }

        /// <summary>
        /// Returns a boolean value indicating whether dispatching the specified query handler is authorized.
        /// </summary>
        /// <typeparam name="TQuery">The type of query.</typeparam>
        /// <typeparam name="TResult">The type of result.</typeparam>
        /// <param name="query">The query that shall be dispatched.</param>
        /// <returns>True if dispatching <paramref name="query"/> is authorized, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
        public bool IsDispatchAuthorized<TQuery, TResult>(TQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return _authorizationVerifyer.AuthorizeQuery<TQuery, TResult>(query);
        }

        Task<object> INonGenericQueryDispatcher.QueryAsync(Type queryType, Type resultType, object query)
        {
            throw new NotImplementedException();
        }

        ITypedNonGenericQueryDispatcher INonGenericQueryDispatcher.GetTypedDispatcher(Type queryType, Type resultType)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Represents a typed query dispatcher that dispatches queries to query handlers.
    /// </summary>
    /// <typeparam name="TQuery">The type of query.</typeparam>
    /// <typeparam name="TResult">The type of result.</typeparam>
    public sealed class QueryDispatcher<TQuery, TResult> : IQueryDispatcher<TQuery, TResult>, ISecureQueryDispatcher<TQuery, TResult> // TODO: Implement ITypedNonGenericQueryDispatcher
    {
        private readonly IAsyncSingleHandlerRegistry<IQueryHandler<TQuery, TResult>> _handlerRegistry
            = new AsyncSingleHandlerRegistry<IQueryHandler<TQuery, TResult>>();
        private readonly IServiceProvider _serviceProvider;
        private readonly IQueryAuthorizationVerifyer _authorizationVerifyer;

        /// <summary>
        /// Creates a new instance of the <see cref="QueryDispatcher{TQuery, TResult}"/> type.
        /// </summary>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> used to obtain services.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="serviceProvider"/> is null.</exception>
        public QueryDispatcher(IServiceProvider serviceProvider) : this(serviceProvider, QueryAuthorizationVerifyer.Default) { }

        /// <summary>
        /// Creates a new instance of the <see cref="QueryDispatcher{TQuery, TResult}"/> type.
        /// </summary>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> used to obtain services.</param>
        /// <param name="authorizationVerifyer">A <see cref="IQueryAuthorizationVerifyer"/> that controls authorization or <see cref="QueryAuthorizationVerifyer.Default"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if either <paramref name="serviceProvider"/> or <paramref name="authorizationVerifyer"/> is null.</exception>
        public QueryDispatcher(IServiceProvider serviceProvider, IQueryAuthorizationVerifyer authorizationVerifyer)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            if (authorizationVerifyer == null)
                throw new ArgumentNullException(nameof(authorizationVerifyer));

            _serviceProvider = serviceProvider;
            _authorizationVerifyer = authorizationVerifyer;
        }

        /// <summary>
        /// Asynchronously registers a query handler.
        /// </summary>
        /// <param name="queryHandlerFactory">The query handler to register.</param>
        /// <returns>
        /// A <see cref="IHandlerRegistration"/> representing the asynchronous operation.
        /// The <see cref="IHandlerRegistration"/> cancels the handler registration if completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="queryHandlerFactory"/> is null.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if the access is unauthorized.</exception>
        public Task<IHandlerRegistration<IQueryHandler<TQuery, TResult>>> RegisterAsync(IHandlerFactory<IQueryHandler<TQuery, TResult>> queryHandlerFactory) // TODO: Correct xml-comments
        {
            if (queryHandlerFactory == null)
                throw new ArgumentNullException(nameof(queryHandlerFactory));

            if (!_authorizationVerifyer.AuthorizeHandlerRegistry())
                throw new UnauthorizedAccessException();

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
        /// <exception cref="UnauthorizedAccessException">Thrown if the access is unauthorized.</exception>
        public ICovariantAwaitable<TResult> QueryAsync(TQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            if (!_authorizationVerifyer.AuthorizeQuery<TQuery, TResult>(query))
                throw new UnauthorizedAccessException();

            if (_handlerRegistry.TryGetHandler(out var handler))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    return handler.GetHandler(scope.ServiceProvider).HandleAsync(query);
                }
            }

            return CovariantAwaitable.FromResult(default(TResult));
        }

        /// <summary>
        /// Returns a boolean value indicating whether registering the specified query handler is authorized.
        /// </summary>
        /// <param name="queryHandler">The query handler that shall be registered.</param>
        /// <returns>True if registering <paramref name="queryHandler"/> is authorized, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="queryHandler"/> is null.</exception>
        public bool IsRegistrationAuthorized(IHandlerFactory<IQueryHandler<TQuery, TResult>> queryHandler)
        {
            if (queryHandler == null)
                throw new ArgumentNullException(nameof(queryHandler));

            return _authorizationVerifyer.AuthorizeHandlerRegistry();
        }

        /// <summary>
        /// Returns a boolean value indicating whether dispatching the specified query handler is authorized.
        /// </summary>
        /// <param name="query">The query that shall be dispatched.</param>
        /// <returns>True if dispatching <paramref name="query"/> is authorized, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
        public bool IsDispatchAuthorized(TQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return _authorizationVerifyer.AuthorizeQuery<TQuery, TResult>(query);
        }
    }

    public sealed class QueryAuthorizationVerifyer : IQueryAuthorizationVerifyer
    {
        public static QueryAuthorizationVerifyer Default { get; } = new QueryAuthorizationVerifyer();

        private QueryAuthorizationVerifyer() { }

        public bool AuthorizeHandlerRegistry()
        {
            return true;
        }

        public bool AuthorizeQuery<TQuery, TResult>(TQuery query)
        {
            return true;
        }
    }
}
