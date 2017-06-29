/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        IEventStore.cs
 * Types:           AI4E.Storage.IReadOnlyEventStore'2
 *                  AI4E.Storage.IEventStore'2
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   16.05.2017 
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
    /// Represents a read-only event store.
    /// </summary>
    /// <typeparam name="TId">The type of id used to identify event streams.</typeparam>
    /// <typeparam name="TEventBase">The event layer supertype.</typeparam>
    public interface IReadOnlyEventStore<TId, TEventBase> : IDisposable
        where TId : struct, IEquatable<TId>
    {
        /// <summary>
        /// Asynchronously retrieves all events of the specified stream.
        /// </summary>
        /// <param name="stream">The event stream.</param>
        /// <param name="cancellation">
        /// A <see cref="CancellationToken"/> used to cancel the asynchronous operation
        /// or <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>
        /// An asynchronous sorted event sequence representing the asynchronous operation.
        /// </returns>
        IAsyncEnumerable<(TEventBase evt, TId commit)> GetEventsAsync(TId stream, CancellationToken cancellation = default(CancellationToken));

        /// <summary>
        /// Asynchronously retriever all events in the specified range within the specified stream.
        /// </summary>
        /// <param name="stream">The event stream.</param>
        /// <param name="start">The identifier of the commit that represents the exclusive lower bound of the event range. </param>
        /// <param name="end">The identifier of the commit that represents the inclusive upper bound of the event range.</param>
        /// <param name="cancellation">
        /// A <see cref="CancellationToken"/> used to cancel the asynchronous operation
        /// or <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>
        /// An asynchronous sorted event sequence representing the asynchronous operation.
        /// </returns>
        /// <example>
        /// Each event in an event stream is part of a commit that represents a single transactional unit.
        /// 
        ///     Time --->
        ///                      ~~ Commits ~~
        ///                #abc123    #01   #987a 
        /// Stream A ------|-o-oo-|--|-o-|--|--o-|---
        ///                |      |  |   |  |    | 
        /// Stream B ------|--o---|--|---|--|-o--|---
        ///                |______|  |___|  |____|  
        ///                
        /// When retrieving stream A with lower bound #abc123 and upper bound #987a, 
        /// all event from commit #01 and #987a within the event-stream A are returned.
        /// 
        /// </example>
        IAsyncEnumerable<(TEventBase evt, TId commit)> GetEventsAsync(TId stream, TId start = default(TId), TId end = default(TId), CancellationToken cancellation = default(CancellationToken));

        /// <summary>
        /// Asynchronously retrieves the revesion of the specified event-stream.
        /// </summary>
        /// <param name="stream">The event-stream.</param>
        /// <param name="cancellation">
        /// A <see cref="CancellationToken"/> used to cancel the asynchronous operation
        /// or <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains the revision of <paramref name="stream"/> or 
        /// the default value of <typeparamref name="TId"/> if the event stream was not found.
        /// </returns>
        Task<TId> GetRevisionAsync(TId stream, CancellationToken cancellation = default(CancellationToken));
    }

    /// <summary>
    /// Represents an event store.
    /// </summary>
    /// <typeparam name="TId">The type of id used to identify event streams.</typeparam>
    /// <typeparam name="TEventBase">The event layer supertype.</typeparam>
    public interface IEventStore<TId, TEventBase> : IReadOnlyEventStore<TId, TEventBase>
        where TId : struct, IEquatable<TId>
    {
        /// <summary>
        /// Asynchronously pushed a collection of event into the event store.
        /// </summary>
        /// <param name="events">The ordered collection of events to push to the event store.</param>
        /// <param name="expectedCommit">The identifier of the commit, the events originated from.</param>
        /// <param name="cancellation">
        /// A <see cref="CancellationToken"/> used to cancel the asynchronous operation
        /// or <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains the identifier of the commit that was created.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="events"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="events"/> contains null values.
        /// </exception>
        Task<TId> PushAsync(IEnumerable<TEventBase> events, TId expectedCommit, CancellationToken cancellation = default(CancellationToken));
    }
}
