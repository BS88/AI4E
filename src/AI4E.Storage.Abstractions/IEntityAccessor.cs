/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        IEntityAccessor.cs
 * Types:           AI4E.IEntityAccessor'3
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

namespace AI4E
{
    /// <summary>
    /// Represents an accessor for entity identity.
    /// </summary>
    /// <typeparam name="TId">The type of id used for entity identification.</typeparam>
    /// <typeparam name="TEntityBase">The entity layer supertype.</typeparam>
    public interface IEntityAccessor<TId, TEventPublisher, TEntityBase>
        where TId : struct, IEquatable<TId>
        //where TEventPublisher : class, new()
    {
        /// <summary>
        /// Returns the identifier of the specified entity.
        /// </summary>
        /// <param name="entity">The entity whose identifier is retrived.</param>
        /// <returns>The identifier of <paramref name="entity"/>.</returns>
        TId GetId(TEntityBase entity);

        TEventPublisher GetEventPublisher(TEntityBase entity);
    }
}
