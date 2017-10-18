/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        Entity.cs 
 * Types:           (1) AI4E.Domain.Entity
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
using System.Diagnostics;

namespace AI4E.Domain
{
    public abstract class Entity : IEntity, IEquatable<Entity>
    {
        private readonly Guid _id;
        private readonly AggregateRoot _aggregateRoot;
        private readonly Lazy<Type> _entityType;

        protected Entity(Guid id, AggregateRoot aggregateRoot) : this(id)
        {
            if (aggregateRoot == null)
                throw new ArgumentNullException(nameof(aggregateRoot));

            _aggregateRoot = aggregateRoot;
        }

        internal Entity(Guid id)
        {
            if (id == default(Guid))
                throw new ArgumentException("The id must not be an empty guid.", nameof(id));

            _id = id;
            _entityType = new Lazy<Type>(() => GetType());
        }

        public Guid Id => _id;

        public Type EntityType => _entityType.Value;

        IAggregateRoot<Guid> IEntity<Guid, Guid>.AggregateRoot => AggregateRoot;

        public AggregateRoot AggregateRoot
        {
            get { return GetAggregateRoot(); }
        }

        internal virtual AggregateRoot GetAggregateRoot()
        {
            return _aggregateRoot;
        }

        protected virtual void Publish<TEvent>(TEvent evt)
            where TEvent : DomainEvent
        {
            var aggregateRoot = GetAggregateRoot();

            Debug.Assert(aggregateRoot != null);

            aggregateRoot.Publish(evt);
        }

        public bool Equals(IEntity<Guid, Guid> other)
        {
            if (ReferenceEquals(other, null))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return EntityType == other.EntityType &&
                   Id == other.Id;
        }

        public bool Equals(IEntity other)
        {
            return Equals(other as IEntity<Guid, Guid>);
        }

        public bool Equals(Entity other)
        {
            return Equals(other as IEntity<Guid, Guid>);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IEntity<Guid, Guid>);
        }

        public override int GetHashCode()
        {
            return EntityType.GetHashCode() ^ Id.GetHashCode();
        }

        public override string ToString()
        {
            return $"{{{EntityType.FullName} #{Id}}}";
        }

        public static bool operator ==(Entity left, Entity right)
        {
            if (ReferenceEquals(left, null))
                return ReferenceEquals(right, null);

            return left.Equals(right);
        }

        public static bool operator !=(Entity left, Entity right)
        {
            if (ReferenceEquals(left, null))
                return !ReferenceEquals(right, null);

            return !left.Equals(right);
        }

        public static bool operator ==(Entity left, IEntity<Guid, Guid> right)
        {
            if (ReferenceEquals(left, null))
                return ReferenceEquals(right, null);

            return left.Equals(right);
        }

        public static bool operator !=(Entity left, IEntity<Guid, Guid> right)
        {
            if (ReferenceEquals(left, null))
                return !ReferenceEquals(right, null);

            return !left.Equals(right);
        }

        public static bool operator ==(IEntity<Guid, Guid> left, Entity right)
        {
            if (ReferenceEquals(right, null))
                return ReferenceEquals(left, null);

            return right.Equals(left);
        }

        public static bool operator !=(IEntity<Guid, Guid> left, Entity right)
        {
            if (ReferenceEquals(right, null))
                return !ReferenceEquals(left, null);

            return !right.Equals(left);
        }
    }
}
