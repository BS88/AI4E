/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        IEventAccessor.cs
 * Types:           AI4E.IEventAccessor'2
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   29.04.2017 
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

namespace AI4E
{
    /// <summary>
    /// Represents an accessor for event related data.
    /// </summary>
    /// <typeparam name="TId">The type of id used to identify event streams.</typeparam>
    /// <typeparam name="TEventBase">The event layer supertype.</typeparam>
    public interface IEventAccessor<TId, TEventBase>
        where TId : struct, IEquatable<TId>
    {
        /// <summary>
        /// Returns the event stream of the specified event.
        /// </summary>
        /// <param name="evt">The event whose stream shall be retrieved.</param>
        /// <returns>An identifier for the event stream.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="evt"/> is null.</exception>
        TId GetEventStream(TEventBase evt);
    }
}
