/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        QueryDispatcherExtension.cs 
 * Types:           AI4E.Integration.QueryDispatcherExtension
 *                  AI4E.Integration.QueryDispatcherExtension.AnonymousQueryHandler'2
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   11.05.2017 
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
using System.Diagnostics;
using System.Threading.Tasks;
using AI4E.Async;

namespace AI4E.Integration
{
    /// <summary>
    /// Defines extensions for the <see cref="IQueryDispatcher"/> interface.
    /// </summary>
    public static class QueryDispatcherExtension
    {
        /// <summary>
        /// Asynchronously dispatches a query of type <see cref="Query{TResult}"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of result.</typeparam>
        /// <param name="queryDispatcher">The query dispatcher.</param>
        /// <param name="query">The query to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains the query result 
        /// or the default value of <typeparamref name="TResult"/> if nothing was found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either <paramref name="queryDispatcher"/> or <paramref name="query"/> is null.
        /// </exception>
        public static Task<TResult> QueryAsync<TResult>(this IQueryDispatcher queryDispatcher, Query<TResult> query)
        {
            if (queryDispatcher == null)
                throw new ArgumentNullException(nameof(queryDispatcher));

            return queryDispatcher.QueryAsync<Query<TResult>, TResult>(query);
        }

        /// <summary>
        /// Asynchronously queries a result without any conditions.
        /// </summary>
        /// <typeparam name="TResult">The type of result.</typeparam>
        /// <param name="queryDispatcher">The query dispatcher.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains the query result 
        /// or the default value of <typeparamref name="TResult"/> if nothing was found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="queryDispatcher"/> is null.
        /// </exception>
        public static Task<TResult> QueryAsync<TResult>(this IQueryDispatcher queryDispatcher)
        {
            if (queryDispatcher == null)
                throw new ArgumentNullException(nameof(queryDispatcher));

            return queryDispatcher.QueryAsync<Query<TResult>, TResult>(new Query<TResult>());
        }

        /// <summary>
        /// Asynchronously dispatches a query of type <see cref="ByIdQuery{TId, TResult}"/>.
        /// </summary>
        /// <typeparam name="TId">The type of id.</typeparam>
        /// <typeparam name="TResult">The type of result.</typeparam>
        /// <param name="queryDispatcher">The query dispatcher.</param>
        /// <param name="query">The query to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains the query result 
        /// or the default value of <typeparamref name="TResult"/> if nothing was found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either <paramref name="queryDispatcher"/> or <paramref name="query"/> is null.
        /// </exception>
        public static Task<TResult> QueryAsync<TId, TResult>(this IQueryDispatcher queryDispatcher, ByIdQuery<TId, TResult> query)
            where TId : struct, IEquatable<TId>
        {
            if (queryDispatcher == null)
                throw new ArgumentNullException(nameof(queryDispatcher));

            return queryDispatcher.QueryAsync<ByIdQuery<TId, TResult>, TResult>(query);
        }

        /// <summary>
        /// Asynchronously queries a result that is identified by id.
        /// </summary>
        /// <typeparam name="TId">The type of id.</typeparam>
        /// <typeparam name="TResult">The type of result.</typeparam>
        /// <param name="queryDispatcher">The query dispatcher.</param>
        /// <param name="id">The id that identifies the result.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains the query result 
        /// or the default value of <typeparamref name="TResult"/> if nothing was found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="queryDispatcher"/> is null.
        /// </exception>
        public static Task<TResult> ByIdAsync<TId, TResult>(this IQueryDispatcher queryDispatcher, TId id)
                    where TId : struct, IEquatable<TId>
        {
            if (queryDispatcher == null)
                throw new ArgumentNullException(nameof(queryDispatcher));

            return queryDispatcher.QueryAsync<ByIdQuery<TId, TResult>, TResult>(new ByIdQuery<TId, TResult>(id));
        }

        /// <summary>
        /// Asynchronously dispatches a query of type <see cref="ByIdQuery{TResult}"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of result.</typeparam>
        /// <param name="queryDispatcher">The query dispatcher.</param>
        /// <param name="query">The query to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains the query result 
        /// or the default value of <typeparamref name="TResult"/> if nothing was found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either <paramref name="queryDispatcher"/> or <paramref name="query"/> is null.
        /// </exception>
        public static Task<TResult> QueryAsync<TResult>(this IQueryDispatcher queryDispatcher, ByIdQuery<TResult> query)
        {
            if (queryDispatcher == null)
                throw new ArgumentNullException(nameof(queryDispatcher));

            return queryDispatcher.QueryAsync<ByIdQuery<TResult>, TResult>(query);
        }

        /// <summary>
        /// Asynchronously queries a result that is identified by id.
        /// </summary>
        /// <typeparam name="TResult">The type of result.</typeparam>
        /// <param name="queryDispatcher">The query dispatcher.</param>
        /// <param name="id">The id that identifies the result.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains the query result 
        /// or the default value of <typeparamref name="TResult"/> if nothing was found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="queryDispatcher"/> is null.
        /// </exception>
        public static Task<TResult> ByIdAsync<TResult>(this IQueryDispatcher queryDispatcher, Guid id)
        {
            if (queryDispatcher == null)
                throw new ArgumentNullException(nameof(queryDispatcher));

            return queryDispatcher.QueryAsync<ByIdQuery<TResult>, TResult>(new ByIdQuery<TResult>(id));
        }

        /// <summary>
        /// Asynchronously dispatches a query of type <see cref="ByParentQuery{TId, TParent, TResult}"/>.
        /// </summary>
        /// <typeparam name="TId">The type of id.</typeparam>
        /// <typeparam name="TParent">The type of parent.</typeparam>
        /// <typeparam name="TResult">The type of result.</typeparam>
        /// <param name="queryDispatcher">The query dispatcher.</param>
        /// <param name="query">The query to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains the query result 
        /// or the default value of <typeparamref name="TResult"/> if nothing was found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either <paramref name="queryDispatcher"/> or <paramref name="query"/> is null.
        /// </exception>
        public static Task<TResult> QueryAsync<TId, TParent, TResult>(this IQueryDispatcher queryDispatcher, ByParentQuery<TId, TParent, TResult> query)
            where TId : struct, IEquatable<TId>
        {
            if (queryDispatcher == null)
                throw new ArgumentNullException(nameof(queryDispatcher));

            return queryDispatcher.QueryAsync<ByParentQuery<TId, TParent, TResult>, TResult>(query);
        }

        /// <summary>
        /// Asynchronously queries a result that is identified by its parent.
        /// </summary>
        /// <typeparam name="TId">The type of id.</typeparam>
        /// <typeparam name="TParent">The type of parent.</typeparam>
        /// <typeparam name="TResult">The type of result.</typeparam>
        /// <param name="queryDispatcher">The query dispatcher.</param>
        /// <param name="parentId">The id that identifies the results parent.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains the query result 
        /// or the default value of <typeparamref name="TResult"/> if nothing was found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="queryDispatcher"/> is null.
        /// </exception>
        public static Task<TResult> ByParent<TId, TParent, TResult>(this IQueryDispatcher queryDispatcher, TId parentId)
                    where TId : struct, IEquatable<TId>
        {
            if (queryDispatcher == null)
                throw new ArgumentNullException(nameof(queryDispatcher));

            return queryDispatcher.QueryAsync<ByParentQuery<TId, TParent, TResult>, TResult>(new ByParentQuery<TId, TParent, TResult>(parentId));
        }

        /// <summary>
        /// Asynchronously dispatches a query of type <see cref="ByParentQuery{TParent, TResult}"/>.
        /// </summary>
        /// <typeparam name="TParent">The type of parent.</typeparam>
        /// <typeparam name="TResult">The type of result.</typeparam>
        /// <param name="queryDispatcher">The query dispatcher.</param>
        /// <param name="query">The query to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains the query result 
        /// or the default value of <typeparamref name="TResult"/> if nothing was found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either <paramref name="queryDispatcher"/> or <paramref name="query"/> is null.
        /// </exception>
        public static Task<TResult> QueryAsync<TParent, TResult>(this IQueryDispatcher queryDispatcher, ByParentQuery<TParent, TResult> query)
        {
            if (queryDispatcher == null)
                throw new ArgumentNullException(nameof(queryDispatcher));

            return queryDispatcher.QueryAsync<ByParentQuery<TParent, TResult>, TResult>(query);
        }

        /// <summary>
        /// Asynchronously queries a result that is identified by its parent.
        /// </summary>
        /// <typeparam name="TParent">The type of parent.</typeparam>
        /// <typeparam name="TResult">The type of result.</typeparam>
        /// <param name="queryDispatcher">The query dispatcher.</param>
        /// <param name="parentId">The id that identifies the results parent.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="ICovariantAwaitable{TResult}.Result"/> contains the query result 
        /// or the default value of <typeparamref name="TResult"/> if nothing was found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="queryDispatcher"/> is null.
        /// </exception>
        public static Task<TResult> ByParent<TParent, TResult>(this IQueryDispatcher queryDispatcher, Guid parentId)
        {
            if (queryDispatcher == null)
                throw new ArgumentNullException(nameof(queryDispatcher));

            return queryDispatcher.QueryAsync<ByParentQuery<TParent, TResult>, TResult>(new ByParentQuery<TParent, TResult>(parentId));
        }

        /// <summary>
        /// Asynchronously registers an anonymous query handler for the specified type of query and result.
        /// </summary>
        /// <typeparam name="TQuery">The type of query.</typeparam>
        /// <typeparam name="TResult">The type of result.</typeparam>
        /// <param name="queryDispatcher">The query dispatcher.</param>
        /// <param name="handler">The query handler that shall be registered.</param>
        /// <returns>
        /// A <see cref="IHandlerRegistration"/> representing the asynchronous operation.
        /// The <see cref="IHandlerRegistration"/> cancels the handler registration if completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either <paramref name="queryDispatcher"/> or <paramref name="handler"/> is null.
        /// </exception>
        public static Task<IHandlerRegistration<IQueryHandler<TQuery, TResult>>> OnQuery<TQuery, TResult>(this IQueryDispatcher queryDispatcher, Func<TQuery, Task<TResult>> handler) // TODO: Correct xml-comments
        {
            if (queryDispatcher == null)
                throw new ArgumentNullException(nameof(queryDispatcher));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            return queryDispatcher.RegisterAsync(new AnonymousQueryHandler<TQuery, TResult>(handler));
        }

        private class AnonymousQueryHandler<TQuery, TResult> : IQueryHandler<TQuery, TResult>, IHandlerFactory<IQueryHandler<TQuery, TResult>>
        {
            private readonly Func<TQuery, Task<TResult>> _handler;

            internal AnonymousQueryHandler(Func<TQuery, Task<TResult>> handler)
            {
                Debug.Assert(handler != null);

                _handler = handler;
            }

            public Task<TResult> HandleAsync(TQuery query)
            {
                if (query == null)
                    throw new ArgumentNullException(nameof(query));

                return _handler(query);
            }

            public IQueryHandler<TQuery, TResult> GetHandler(IServiceProvider serviceProvider)
            {
                return this;
            }
        }

        public static Task<IHandlerRegistration<IQueryHandler<TQuery, TResult>>> RegisterAsync<TQuery, TResult, TQueryHandler>(this IQueryDispatcher queryDispatcher)
            where TQueryHandler : IQueryHandler<TQuery, TResult>
        {
            if (queryDispatcher == null)
                throw new ArgumentNullException(nameof(queryDispatcher));

            return queryDispatcher.RegisterAsync((IHandlerFactory<IQueryHandler<TQuery, TResult>>)(IHandlerFactory<TQueryHandler>)new DefaultHandlerFactory<TQueryHandler>());
        }
    }
}
