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
