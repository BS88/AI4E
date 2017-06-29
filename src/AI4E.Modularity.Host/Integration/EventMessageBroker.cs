using System;
using System.Threading.Tasks;

namespace AI4E.Modularity.Integration
{
    public sealed class EventMessageBroker :
        IMessageHandler<RegisterEventForwarding>,
        IMessageHandler<UnregisterEventForwarding>,
        IMessageHandler<DispatchEvent>
    {
        private readonly IHostEventDispatcher _eventDispatcher;

        public EventMessageBroker(IHostEventDispatcher eventDispatcher)
        {
            if (eventDispatcher == null)
                throw new ArgumentNullException(nameof(eventDispatcher));

            _eventDispatcher = eventDispatcher;
        }

        public Task HandleAsync(RegisterEventForwarding message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Console.WriteLine($"Receiving 'RegisterEventForwarding' for event type '{message.EventType.FullName}'.");

            return _eventDispatcher.RegisterForwardingAsync(message.EventType);
        }

        public Task HandleAsync(UnregisterEventForwarding message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Console.WriteLine($"Receiving 'UnregisterEventForwarding' for event type '{message.EventType.FullName}'.");

            return _eventDispatcher.UnregisterForwardingAsync(message.EventType);
        }

        public Task HandleAsync(DispatchEvent message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Console.WriteLine($"Receiving 'DispatchEvent' for event type '{message.EventType.FullName}' with event '{message.Event}'.");

            return _eventDispatcher.DispatchAsync(message.EventType, message.Event);
        }
    }
}
