/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        DomainEvent.cs 
 * Types:           (1) AI4E.Domain.DomainEvent
 *                  (2) AI4E.Domain.DomainEventConflictAttribute
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
using System.Collections.Generic;
using System.Reflection;

namespace AI4E.Domain
{
    public abstract class DomainEvent : IDomainEvent
    {
        private readonly Lazy<Type> _eventType;
        private readonly Lazy<IEnumerable<DomainEventConflictAttribute>> _conflictAttributes;

        protected DomainEvent(Guid stream)
        {
            Stream = stream;

            _eventType = new Lazy<Type>(() => GetType());
            _conflictAttributes = new Lazy<IEnumerable<DomainEventConflictAttribute>>(() => _eventType.Value.GetCustomAttributes<DomainEventConflictAttribute>());
        }

        public Guid Stream { get; }

        public virtual bool ConflictsWith(IDomainEvent<Guid> other)
        {
            if (other.Stream != Stream)
                return false;

            var currentHierarchyDiff = -1;
            var conflict = false;

            foreach(var attribute in _conflictAttributes.Value)
            {
                var hierarchyDiff = GetHierarchyDifference(other.GetType(), attribute.EventType);

                if(currentHierarchyDiff == -1 || hierarchyDiff < currentHierarchyDiff)
                {
                    currentHierarchyDiff = hierarchyDiff;
                    conflict = attribute.Conflict;
                }
            }

            return conflict;
        }

        private int GetHierarchyDifference(Type type, Type baseType)
        {
            for(var result = 0; type != null && type != typeof(object); result++)
            {
                if (type == baseType)
                    return result;

                type = type.BaseType;
            }

            return -1;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class DomainEventConflictAttribute : Attribute
    {
        public DomainEventConflictAttribute(Type eventType)
        {
            if (eventType == null)
                throw new ArgumentNullException(nameof(eventType));

            if (!typeof(IDomainEvent).IsAssignableFrom(eventType))
                throw new ArgumentException("The type does not specify an event.", nameof(eventType));

            EventType = eventType;
        }

        public Type EventType { get; }

        public bool Conflict { get; set; }
    }
}
