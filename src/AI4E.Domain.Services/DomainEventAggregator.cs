/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        DomainEventAggregator.cs 
 * Types:           (1) AI4E.Domain.Services.DomainEventAggregator
 *                  (2) AI4E.Domain.Services.DomainEventAggregatorBinder
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

namespace AI4E.Domain.Services
{
    public class DomainEventAggregator : IDomainEventAggregator
    {
        private readonly IEventProcessor<IDomainEvent<Guid>, AggregateRoot> _eventProcessor;

        public DomainEventAggregator(IEventProcessor<IDomainEvent<Guid>, AggregateRoot> eventProcessor)
        {
            if (eventProcessor == null)
                throw new ArgumentNullException(nameof(eventProcessor));

            _eventProcessor = eventProcessor;
        }

        public void Add<TEvent>(AggregateRoot aggregate, TEvent evt) where TEvent : IDomainEvent<Guid>
        {
            if (aggregate == null)
                throw new ArgumentNullException(nameof(aggregate));

            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            _eventProcessor.RegisterEvent(aggregate, evt);
        }
    }

    public sealed class DomainEventAggregatorBinder : IEventProcessorBinder<IDomainEvent<Guid>, DomainEventPublisher, AggregateRoot>
    {
        public void BindProcessor(DomainEventPublisher eventPublisher, IEventProcessor<IDomainEvent<Guid>, AggregateRoot> eventProcessor)
        {
            if (eventPublisher == null)
                throw new ArgumentNullException(nameof(eventPublisher));

            if (eventProcessor == null)
                throw new ArgumentNullException(nameof(eventProcessor));

            eventPublisher.BindAggregator(new DomainEventAggregator(eventProcessor));
        }

        public void UnbindProcessor(DomainEventPublisher eventPublisher)
        {
            if (eventPublisher == null)
                throw new ArgumentNullException(nameof(eventPublisher));

            eventPublisher.ReleaseAggregator();
        }
    }
}
