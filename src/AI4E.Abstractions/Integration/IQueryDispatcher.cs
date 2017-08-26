/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        IQueryDispatcher.cs 
 * Types:           (1) AI4E.Integration.IQueryDispatcher
 *                  (2) AI4E.Integration.IQueryDispatcher'2
 *                  (3) AI4E.Integration.ISecureQueryDispatcher
 *                  (4) AI4E.Integration.ISecureQueryDispatcher'2
 *                  (5) AI4E.Integration.INonGenericQueryDispatcher
 *                  (6) AI4E.Integration.ITypedNonGenericQueryDispatcher
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   26.08.2017 
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
using System.Threading.Tasks;

namespace AI4E.Integration
{
    /// <summary>
    /// Represents a query dispatcher that dispatches queries to query handlers.
    /// </summary>
    public interface IQueryDispatcher : INonGenericQueryDispatcher
    {
        /// <summary>
        /// Asynchronously registers a query handler.
        /// </summary>
        /// <typeparam name="TQuery">The type of query.</typeparam>
        /// <param name="queryHandlerFactory">The query handler to register.</param>
        /// <returns>
        /// TODO
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="queryHandlerFactory"/> is null.</exception>
        Task<IHandlerRegistration<IQueryHandler<TQuery>>> RegisterAsync<TQuery>(IHandlerFactory<IQueryHandler<TQuery>> queryHandlerFactory); // TODO: Correct xml-comments

        /// <summary>
        /// Returns a typed query dispatcher for the specified query and result types.
        /// </summary>
        /// <typeparam name="TQuery">The type of query.</typeparam>
        /// <returns>
        /// A typed query dispatcher for queries of type <typeparamref name="TQuery"/>.
        /// </returns>
        IQueryDispatcher<TQuery> GetTypedDispatcher<TQuery>();

        /// <summary>
        /// Asynchronously dispatches a query.
        /// </summary>
        /// <typeparam name="TQuery">The type of query.</typeparam>
        /// <param name="query">The query to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
        Task<IQueryResult> QueryAsync<TQuery>(TQuery query);
    }

    /// <summary>
    /// Represents a typed query dispatcher that dispatches queries to query handlers.
    /// </summary>
    /// <typeparam name="TQuery">The type of query.</typeparam>
    public interface IQueryDispatcher<TQuery> : ITypedNonGenericQueryDispatcher
    {
        /// <summary>
        /// Asynchronously registers a query handler.
        /// </summary>
        /// <param name="queryHandlerFactory">The query handler to register.</param>
        /// <returns>
        /// TODO
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="queryHandlerFactory"/> is null.</exception>
        Task<IHandlerRegistration<IQueryHandler<TQuery>>> RegisterAsync(IHandlerFactory<IQueryHandler<TQuery>> queryHandlerFactory); // TODO: Correct xml-comments

        /// <summary>
        /// Asynchronously dispatches a query.
        /// </summary>
        /// <param name="query">The query to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
        Task<IQueryResult> QueryAsync(TQuery query);
    }

    /// <summary>
    /// Represents a query dispatcher that controls access.
    /// </summary>
    public interface ISecureQueryDispatcher : IQueryDispatcher
    {
        /// <summary>
        /// Returns a boolean value indicating whether registering the specified query handler is authorized.
        /// </summary>
        /// <typeparam name="TQuery">The type of query.</typeparam>
        /// <typeparam name="TResult">The type of result.</typeparam>
        /// <param name="queryHandlerFactory">The query handler that shall be registered.</param>
        /// <returns>True if registering <paramref name="queryHandlerFactory"/> is authorized, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="queryHandlerFactory"/> is null.</exception>
        bool IsRegistrationAuthorized<TQuery>(IHandlerFactory<IQueryHandler<TQuery>> queryHandlerFactory);

        /// <summary>
        /// Returns a boolean value indicating whether dispatching the specified query handler is authorized.
        /// </summary>
        /// <typeparam name="TQuery">The type of query.</typeparam>
        /// <typeparam name="TResult">The type of result.</typeparam>
        /// <param name="query">The query that shall be dispatched.</param>
        /// <returns>True if dispatching <paramref name="query"/> is authorized, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
        bool IsDispatchAuthorized<TQuery>(TQuery query);
    }

    /// <summary>
    /// Represents a query dispatcher that controls access.
    /// </summary>
    /// <typeparam name="TQuery">The type of query.</typeparam>
    public interface ISecureQueryDispatcher<TQuery> : IQueryDispatcher<TQuery>
    {
        /// <summary>
        /// Returns a boolean value indicating whether registering the specified query handler is authorized.
        /// </summary>
        /// <param name="queryHandlerFactory">The query handler that shall be registered.</param>
        /// <returns>True if registering <paramref name="queryHandlerFactory"/> is authorized, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="queryHandlerFactory"/> is null.</exception>
        bool IsRegistrationAuthorized(IHandlerFactory<IQueryHandler<TQuery>> queryHandlerFactory);

        /// <summary>
        /// Returns a boolean value indicating whether dispatching the specified query handler is authorized.
        /// </summary>
        /// <param name="query">The query that shall be dispatched.</param>
        /// <returns>True if dispatching <paramref name="query"/> is authorized, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
        bool IsDispatchAuthorized(TQuery query);
    }

    /// <summary>
    /// Represents a non-generic query dispatcher that dispatches queries to query handlers.
    /// </summary>
    public interface INonGenericQueryDispatcher
    {
        /// <summary>
        /// Returns a typed non-generic query dispatcher for the specified query type.
        /// </summary>
        /// <param name="queryType">The type of query.</param>
        /// <returns>
        /// A typed non-generic query dispatcher for queries of type <paramref name="queryType"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="queryType"/> is null.</exception>
        ITypedNonGenericQueryDispatcher GetTypedDispatcher(Type queryType);

        /// <summary>
        /// Asynchronously dispatches a query.
        /// </summary>
        /// <param name="queryType">The type of query.</param>
        /// <param name="query">The query to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if either <paramref name="queryType"/> or <paramref name="query"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="query"/> is not of type <paramref name="queryType"/> or a derived type.</exception>
        Task<IQueryResult> QueryAsync(Type queryType, object query);
    }

    /// <summary>
    /// Represents a typed non-generic query dispatcher that dispatches queries to query handler.
    /// </summary>
    public interface ITypedNonGenericQueryDispatcher
    {
        /// <summary>
        /// Asynchronously dispatches a query. 
        /// </summary>
        /// <param name="query">The query to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
        Task<IQueryResult> QueryAsync(object query);

        /// <summary>
        /// Gets the type of query.
        /// </summary>
        Type QueryType { get; }
    }
}
