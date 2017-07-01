/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        IQueryHandler.cs
 * Types:           AI4E.Integration.IQueryHandler'2
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   01.07.2017 
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

using System.Threading.Tasks;

namespace AI4E.Integration
{
    /// <summary>
    /// Represents a query handler.
    /// </summary>
    /// <typeparam name="TQuery">The type of query.</typeparam>
    /// <typeparam name="TResult">The type of result.</typeparam>
    public interface IQueryHandler<in TQuery, TResult>
    {
        /// <summary>
        /// Asynchrnously handles a query.
        /// </summary>
        /// <param name="query">The query that shall be handled.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains the query result or the default value of <typeparamref name="TResult"/> if nothing was found.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
        Task<TResult> HandleAsync(TQuery query);
    }
}
