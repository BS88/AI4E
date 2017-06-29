/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        IDataStore.cs
 * Types:           AI4E.IQueryableDataStore
 *                  AI4E.IDataStore
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   13.05.2017 
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AI4E.Storage
{
    /// <summary>
    /// Represents a queryable data store.
    /// </summary>
    public interface IQueryableDataStore : IDisposable
    {
        /// <summary>
        /// Asynchronously performs a query.
        /// </summary>
        /// <typeparam name="TData">The type of source query.</typeparam>
        /// <typeparam name="TResult">The type of result.</typeparam>
        /// <param name="queryShaper">The query shaper.</param>
        /// <param name="cancellation">A <see cref="CancellationToken"/> used to cancel the asynchronous operation or <see cref="CancellationToken.None"/>.</param>
        /// <returns>
        /// An async enumerable representing the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="queryShaper"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        IAsyncEnumerable<TResult> QueryAsync<TData, TResult>(Func<IQueryable<TData>, IQueryable<TResult>> queryShaper, CancellationToken cancellation = default(CancellationToken));
    }

    /// <summary>
    /// Represents a data store.
    /// </summary>
    public interface IDataStore : IQueryableDataStore
    {
        /// <summary>
        /// Gets a boolean value indicating whether any changes are pending.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        bool ChangesPending { get; }

        /// <summary>
        /// Adds an object to the store.
        /// </summary>
        /// <typeparam name="TData">The type of data.</typeparam>
        /// <param name="data">The object to store.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="data"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        void Add<TData>(TData data);

        /// <summary>
        /// Updates an object in the store.
        /// </summary>
        /// <typeparam name="TData">The type of data.</typeparam>
        /// <param name="data">The object to update.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="data"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        void Update<TData>(TData data);

        /// <summary>
        /// Removes an object from the store.
        /// </summary>
        /// <typeparam name="TData">The type of data.</typeparam>
        /// <param name="data">The object to remove.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="data"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        void Remove<TData>(TData data);

        /// <summary>
        /// Dicards all pending changes.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        void DiscardChanges();

        /// <summary>
        /// Asynchronously saves all pending changes.
        /// </summary>
        /// <param name="cancellation">A <see cref="CancellationToken"/> used to cancel the asynchronous operation or <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        Task SaveChangesAsync(CancellationToken cancellation = default(CancellationToken));
    }  
}
