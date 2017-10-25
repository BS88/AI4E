/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        QueryableDataStoreExtensions.cs 
 * Types:           AI4E.Storage.QueryableDataStoreExtensions
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   10.05.2017 
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
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AI4E.Storage
{
    /// <summary>
    /// Defines extensions for the <see cref="IQueryableDataStore"/> type.
    /// </summary>
    public static class QueryableDataStoreExtensions
    {
        public static IAsyncEnumerable<TData> GetAllAsync<TData>(this IQueryableDataStore dataStore, 
                                                                 CancellationToken cancellation = default)
        {
            if (dataStore == null)
                throw new ArgumentNullException(nameof(dataStore));

            return dataStore.QueryAsync<TData, TData>(p => p, cancellation);
        }

        public static IAsyncEnumerable<TResult> GetAllAsync<TData, TResult>(this IQueryableDataStore dataStore,
                                                                            Expression<Func<TData, TResult>> projection,
                                                                            CancellationToken cancellation = default)
        {
            if (dataStore == null)
                throw new ArgumentNullException(nameof(dataStore));

            if (projection == null)
                throw new ArgumentNullException(nameof(projection));

            return dataStore.QueryAsync<TData, TResult>(p => p.Select(projection), cancellation);
        }

        public static IAsyncEnumerable<TData> GetByAsync<TData>(this IQueryableDataStore dataStore,
                                                                Expression<Func<TData, bool>> predicate,
                                                                CancellationToken cancellation = default)
        {
            if (dataStore == null)
                throw new ArgumentNullException(nameof(dataStore));

            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return dataStore.QueryAsync<TData, TData>(p => p.Where(predicate), cancellation);
        }

        public static IAsyncEnumerable<TResult> GetByAsync<TData, TResult>(this IQueryableDataStore dataStore,
                                                                           Expression<Func<TData, bool>> predicate,
                                                                           Expression<Func<TData, TResult>> projection,
                                                                           CancellationToken cancellation = default)
        {
            if (dataStore == null)
                throw new ArgumentNullException(nameof(dataStore));

            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            if (projection == null)
                throw new ArgumentNullException(nameof(projection));

            return dataStore.QueryAsync<TData, TResult>(p => p.Where(predicate).Select(projection), cancellation);
        }

        public static Task<TData> GetSingleAsync<TData>(this IQueryableDataStore dataStore,
                                                        Expression<Func<TData, bool>> predicate,
                                                        CancellationToken cancellation = default)
        {
            if (dataStore == null)
                throw new ArgumentNullException(nameof(dataStore));

            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return dataStore.QueryAsync<TData, TData>(p => p.Where(predicate), cancellation).FirstOrDefault();
        }

        public static Task<TResult> GetSingleAsync<TData, TResult>(this IQueryableDataStore dataStore,
                                                                   Expression<Func<TData, bool>> predicate,
                                                                   Expression<Func<TData, TResult>> projection,
                                                                   CancellationToken cancellation = default)
        {
            if (dataStore == null)
                throw new ArgumentNullException(nameof(dataStore));

            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            if (projection == null)
                throw new ArgumentNullException(nameof(projection));

            return dataStore.QueryAsync<TData, TResult>(p => p.Where(predicate).Select(projection), cancellation).FirstOrDefault();
        }
    }
}
