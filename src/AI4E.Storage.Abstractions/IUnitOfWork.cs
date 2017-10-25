/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        IUnitOfWork.cs
 * Types:           AI4E.Storage.IUnitOfWork'1
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   07.05.2017 
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
using System.Threading;
using System.Threading.Tasks;

namespace AI4E.Storage
{
    /// <summary>
    /// Represents a unit of work.
    /// </summary>
    /// <typeparam name="T">The type of data the unit of work handles.</typeparam>
    public interface IUnitOfWork<T> : IDisposable
    {
        /// <summary>
        /// Gets a boolean value inidacting whether any changes are pending.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        bool ChangesPending { get; }

        /// <summary>
        /// Gets the collection of items registered as being new.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        IReadOnlyCollection<T> Inserted { get; } // TODO: Adjust name with RegisterNew(T)

        /// <summary>
        /// Gets the collection of items registered as being updated.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        IReadOnlyCollection<T> Updated { get; }

        /// <summary>
        /// Gets the collection of items registered as being deleted.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        IReadOnlyCollection<T> Deleted { get; }

        /// <summary>
        /// Registeres an object as beeing new.
        /// </summary>
        /// <param name="obj">The object to register.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="obj"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        void RegisterNew(T obj);

        /// <summary>
        /// Registeres an object as beeing updated.
        /// </summary>
        /// <param name="obj">The object to register.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="obj"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        void RegisterUpdated(T obj);

        /// <summary>
        /// Registeres an object as beeing deleted.
        /// </summary>
        /// <param name="obj">The object to register.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="obj"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        void RegisterDeleted(T obj);

        /// <summary>
        /// Deregisters an object.
        /// </summary>
        /// <param name="obj">The object to deregister.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="obj"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        void Deregister(T obj);

        EntityState GetState(T obj);

        void SetState(T obj, EntityState state);

        /// <summary>
        /// Rolls back the unit of work.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        void Rollback();

        /// <summary>
        /// Asynchronously commits the unit of work.
        /// </summary>
        /// <param name="cancellation">A <see cref="CancellationToken"/> used to cancel the asynchronous operation or <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        Task CommitAsync(CancellationToken cancellation);
    }

    public enum EntityState
    {
        Untracked = 0,
        Created = 1,
        Updated = 2,
        Deleted = 3
    }
}
