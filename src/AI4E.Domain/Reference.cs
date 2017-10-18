/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        Reference.cs 
 * Types:           (1) AI4E.Domain.Reference'1
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

using Nito.AsyncEx;
using System;
using System.Threading.Tasks;

namespace AI4E.Domain
{
    public struct Reference<T> : IReference<T>, IEquatable<Reference<T>>
        where T : IAggregateRoot<Guid>
    {
        private readonly AsyncLazy<T> _aggregate;

        public Reference(T aggregate)
        {
            if (aggregate == null)
            {
                Id = Guid.Empty;
            }
            else
            {
                Id = aggregate.Id;
            }

            _aggregate = new AsyncLazy<T>(() => Task.FromResult(aggregate));
        }

        private Reference(Guid id, IReferenceResolver referenceResolver)
        {
            if (referenceResolver == null)
                throw new ArgumentNullException(nameof(referenceResolver));

            Id = id;
            _aggregate = new AsyncLazy<T>(async () => await referenceResolver.ResolveAsync<T, Guid>(id));
        }

        public Guid Id { get; }

        public Task<T> ResolveAsync()
        {
            return _aggregate.Task;
        }

        #region Equality

        public bool Equals(IReference<T, Guid> other)
        {
            return other != null && other.Id == Id;
        }

        public bool Equals(IReference<T> other)
        {
            return Equals((IReference<T, Guid>)other);
        }

        public bool Equals(Reference<T> other)
        {
            return other.Id == Id;
        }

        public override bool Equals(object obj)
        {
            return obj is IReference<T, Guid> reference && Equals(reference);
        }

        public static bool operator ==(Reference<T> left, Reference<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Reference<T> left, Reference<T> right)
        {
            return !left.Equals(right);
        }

        public static bool operator ==(Reference<T> left, IReference<T, Guid> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Reference<T> left, IReference<T, Guid> right)
        {
            return !left.Equals(right);
        }

        public static bool operator ==(IReference<T, Guid> left, Reference<T> right)
        {
            return right.Equals(left);
        }

        public static bool operator !=(IReference<T, Guid> left, Reference<T> right)
        {
            return !right.Equals(left);
        }

        #endregion

        public override int GetHashCode()
        {
            return typeof(T).GetHashCode() ^ Id.GetHashCode();
        }

        public override string ToString()
        {
            return $"Reference<{typeof(T).FullName}> #{Id}";
        }

        public static implicit operator Reference<T>(T aggregate)
        {
            return new Reference<T>(aggregate);
        }
    }
}
