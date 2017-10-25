/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        IEventAuthorizationVerifyer.cs 
 * Types:           AI4E.IEventAuthorizationVerifyer
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   07.05.2017 
 * Status:          In development
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
    /// Represents a verifyer that checks for authorization of event handler 
    /// registry and dispatch.
    /// </summary>
    public interface IEventAuthorizationVerifyer
    {
        /// <summary>
        /// Checks whether handler registry is authorized.
        /// </summary>
        /// <returns>True if event handler registry is authorized, false otherwise.</returns>
        bool AuthorizeHandlerRegistry();

        /// <summary>
        /// Checks whether dispatching the specified event is authorized.
        /// </summary>
        /// <typeparam name="TEvent">The type of event.</typeparam>
        /// <param name="evt">The event that shall be dispatched.</param>
        /// <returns>True if the event dispatch is authorized, false otherwise.</returns>
        bool AuthorizeEventNotification<TEvent>(TEvent evt);
    }
}
