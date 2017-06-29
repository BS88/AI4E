/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        IEventConflictResolver.cs
 * Types:           AI4E.IEventConflictResolver'1
 *                  AI4E.INonGenericEventDispatcher
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

namespace AI4E
{
    /// <summary>
    /// Represents a conflict resolver for the specified type of events.
    /// </summary>
    /// <typeparam name="TEventBase">The event layer supertype.</typeparam>
    public interface IEventConflictResolver<TEventBase>
    {
        /// <summary>
        /// Returns a boolean value indicating whether the soecified event conflicts with another event.
        /// </summary>
        /// <param name="evt">The event that shall be checked for a conflict.</param>
        /// <param name="other">The existing event.</param>
        /// <returns>True if <paramref name="evt"/> conflicts with <paramref name="other"/>, false otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if either<paramref name="evt"/> or <paramref name="other"/> is null.</exception>
        bool DoConflict(TEventBase evt, TEventBase other);
    }
}
