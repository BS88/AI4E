/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        IRemoteQueryDispatcher.cs 
 * Types:           (1) AI4E.Modularity.Integration.IRemoteQueryDispatcher
 *                  (2) AI4E.Modularity.Integration.IRemoteQueryDispatcher'2
 *                  (3) AI4E.Modularity.Integration.INonGenericRemoteQueryDispatcher
 *                  (4) AI4E.Modularity.Integration.ITypedNonGenericRemoteQueryDispatcher
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
using System.Threading.Tasks;
using AI4E;

namespace AI4E.Modularity.Integration
{
    /// <summary>
    /// Represents a remote query dispatcher that dispatches queries to query handler.
    /// </summary>
    public interface IRemoteQueryDispatcher : IQueryDispatcher
    {
        /// <summary>
        /// Returns a typed query handler for the specified query and result type.
        /// </summary>
        /// <typeparam name="TQuery">The type of query.</typeparam>
        /// <returns>A typed query dispatcher.</returns>
        new IRemoteQueryDispatcher<TQuery> GetTypedDispatcher<TQuery>();

        /// <summary>
        /// Asynchronously dispatches a query locally only.
        /// </summary>
        /// <typeparam name="TQuery">The type of query.</typeparam>
        /// <param name="query">The query to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
        Task<IQueryResult> LocalDispatchAsync<TQuery>(TQuery query);

        // TODO: Move this to a separate interface.
        void NotifyForwardingActive<TQuery>();
        void NotifyForwardingInactive<TQuery>();
    }

    /// <summary>
    /// Represents a typed remote query dispatcher that dispatches queries to query handler.
    /// </summary>
    /// <typeparam name="TQuery">The type of query.</typeparam>
    /// <typeparam name="TResult">The type of result.</typeparam>
    public interface IRemoteQueryDispatcher<TQuery> : IQueryDispatcher<TQuery>
    {
        /// <summary>
        /// Asynchronously dispatches a query locally only.
        /// </summary>
        /// <param name="query">The query to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
        Task<IQueryResult> LocalDispatchAsync(TQuery query);

        // TODO: Move this to a separate interface.
        bool IsForwardingActive { get; }

        void NotifyForwardingActive();
        void NotifyForwardingInactive();
    }

    /// <summary>
    /// Represents a non-generic remote query dispatcher that dispatches queries to query handler.
    /// </summary>
    public interface INonGenericRemoteQueryDispatcher : INonGenericQueryDispatcher
    {
        /// <summary>
        /// Returns a non-generic typed remote query handler for the specified type of query and result.
        /// </summary>
        /// <param name="queryType">The type of query.</param>
        /// <returns>A non-generic typed remote query dispatcher.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="queryType"/> is null.</exception>
        new ITypedNonGenericRemoteQueryDispatcher GetTypedDispatcher(Type queryType);

        /// <summary>
        /// Asynchronously dispatched a query locally only.
        /// </summary>
        /// <param name="queryType">The type of query.</param>
        /// <param name="query">The query to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains the result of the query operation
        /// or the default value of <paramref name="resultType"/> if the query could not be dispatched.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if either <paramref name="queryType"/> or <paramref name="query"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="query"/> is not of type <paramref name="queryType"/> or a derived type.</exception>
        Task<IQueryResult> LocalDispatchAsync(Type queryType, object query);

        // TODO: Move this to a separate interface.
        void NotifyForwardingActive(Type queryType);
        void NotifyForwardingInactive(Type queryType);
    }

    /// <summary>
    /// Represents a non-generic typed remote query dispatcher that dispatches queries to query handler.
    /// </summary>
    public interface ITypedNonGenericRemoteQueryDispatcher : ITypedNonGenericQueryDispatcher
    {
        /// <summary>
        /// Asynchronously dispatched a query locally only.
        /// </summary>
        /// <param name="queryType">The type of query.</param>
        /// <param name="query">The query to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains the result of the query operation
        /// or the default value of <paramref name="resultType"/> if the query could not be dispatched.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="query"/> is not of type <see cref="ITypedNonGenericQueryDispatcher.QueryType"/> or a derived type.</exception>
        Task<IQueryResult> LocalDispatchAsync(object query);

        // TODO: Move this to a separate interface.
        void NotifyForwardingActive();
        void NotifyForwardingInactive();

        bool IsForwardingActive { get; }
    }
}
