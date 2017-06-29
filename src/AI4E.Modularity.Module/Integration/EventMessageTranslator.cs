using System;
using System.Threading.Tasks;

namespace AI4E.Modularity.Integration
{
    public sealed class EventMessageTranslator : IEventMessageTranslator
    {
        private readonly IMessageEndPoint _messageEndPoint;

        public EventMessageTranslator(IMessageEndPoint messageEndPoint)
        {
            if (messageEndPoint == null)
                throw new ArgumentNullException(nameof(messageEndPoint));

            _messageEndPoint = messageEndPoint;
        }

        public Task RegisterForwardingAsync<TEvent>()
        {
            var message = new RegisterEventForwarding(typeof(TEvent));

            Console.WriteLine($"Sending 'RegisterEventForwarding' for event type '{message.EventType.FullName}'.");

            return _messageEndPoint.SendAsync(message);
        }

        public Task UnregisterForwardingAsync<TEvent>()
        {
            var message = new UnregisterEventForwarding(typeof(TEvent));

            Console.WriteLine($"Sending 'UnregisterEventForwarding' for event type '{message.EventType.FullName}'.");

            return _messageEndPoint.SendAsync(message);
        }

        public Task DispatchAsync<TEvent>(TEvent evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            var message = new DispatchEvent(typeof(TEvent), evt);

            Console.WriteLine($"Sending 'DispatchEvent' for event type '{message.EventType.FullName}' with event '{message.Event}'.");

            return _messageEndPoint.SendAsync(message);
        }
    }
}
