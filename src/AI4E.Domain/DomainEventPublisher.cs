/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        DomainEventPublisher.cs 
 * Types:           (1) AI4E.Domain.DomainEventPublisher
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
    public sealed class DomainEventPublisher
    {
        private IDomainEventAggregator _domainEventAggregator;

        public DomainEventPublisher()
        {
            _domainEventAggregator = null;
        }

        public DomainEventPublisher(IDomainEventAggregator domainEventAggregator)
        {
            if (domainEventAggregator == null)
                throw new ArgumentNullException(nameof(domainEventAggregator));

            _domainEventAggregator = domainEventAggregator;
        }

        public bool IsBound => _domainEventAggregator != null;

        public void BindAggregator(IDomainEventAggregator domainEventAggregator)
        {
            if (domainEventAggregator == null)
                throw new ArgumentNullException(nameof(domainEventAggregator));

            if (IsBound)
                throw new InvalidOperationException("The event publisher is already bound to an event aggregator.");

            _domainEventAggregator = domainEventAggregator;
        }

        public void ReleaseAggregator()
        {
            if (!IsBound)
                return;

            _domainEventAggregator = null;
        }

        public void Publish<TEvent>(AggregateRoot aggregate, TEvent evt)
            where TEvent:IDomainEvent<Guid>
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            var domainEventAggregator = _domainEventAggregator;

            if (domainEventAggregator == null)
                throw new InvalidOperationException("The event publisher is not bound to an event aggregator.");

            domainEventAggregator.Add(aggregate, evt);
        }
    }
}
