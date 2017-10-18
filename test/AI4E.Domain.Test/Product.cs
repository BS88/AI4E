using System;

namespace AI4E.Domain.Test
{
    public sealed class Product : AggregateRoot
    {
        private string _name;

        public Product(Guid id, DomainEventPublisher domainEventPublisher, string name)
            : base(id, domainEventPublisher)
        {
            _name = name;

            Publish(new ProductCreatedEvent(id, name));
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name == value)
                    return;

                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException();

                _name = value;
                Publish(new ProductRenamedEvent(Id, value));
            }
        }

        protected override void DoDispose()
        {
            Publish(new ProductDisposedEvent(Id));
        }

        public sealed class ProductCreatedEvent : DomainEvent
        {
            public ProductCreatedEvent(Guid stream, string name) : base(stream)
            {
                Name = name;
            }

            public string Name { get; }
        }

        public sealed class ProductRenamedEvent : DomainEvent
        {
            public ProductRenamedEvent(Guid stream, string name) : base(stream)
            {
                Name = name;
            }

            public string Name { get; }
        }

        public sealed class ProductDisposedEvent : DomainEvent
        {
            public ProductDisposedEvent(Guid stream) : base(stream) { }
        }
    }
}
