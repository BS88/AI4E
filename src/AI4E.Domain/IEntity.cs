/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        IEntity.cs 
 * Types:           (1) AI4E.Domain.IEntity'1
 *                  (2) AI4E.Domain.IEntity
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   18.10.2017 
 * --------------------------------------------------------------------------------------------------------------------
 */

/* License
 * --------------------------------------------------------------------------------------------------------------------
 * This file is part of the AI4E distribution.
 *   (https://github.com/AndreasTruetschel/AI4E)
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

namespace AI4E.Domain
{
    public interface IEntity<TId, TAggregateRootId> : IEquatable<IEntity<TId, TAggregateRootId>>
        where TId : struct, IEquatable<TId>
        where TAggregateRootId : struct, IEquatable<TAggregateRootId>
    {
        TId Id { get; }

        Type EntityType { get; }

        IAggregateRoot<TAggregateRootId> AggregateRoot { get; }
    }

    public interface IEntity : IEntity<Guid, Guid>, IEquatable<IEntity> { }
}
