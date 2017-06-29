using System;
using System.Threading.Tasks;
using AI4E.Integration;

namespace AI4E.Modularity.Integration
{
    public sealed class EventHandlerProxy<TEvent> :
        IEventHandler<TEvent>,
        IHandlerFactory<IEventHandler<TEvent>>,
        IActivationNotifyable,
        IDeactivationNotifyable
    {
        private readonly IMessageEndPoint _messageEndPoint;

        public EventHandlerProxy(IMessageEndPoint messageEndPoint)
        {
            if (messageEndPoint == null)
                throw new ArgumentNullException(nameof(messageEndPoint));

            _messageEndPoint = messageEndPoint;
        }

        IEventHandler<TEvent> IHandlerFactory<IEventHandler<TEvent>>.GetHandler(IServiceProvider serviceProvider)
        {
            return this;
        }

        public Task HandleAsync(TEvent evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            var message = new DispatchEvent(typeof(TEvent), evt);

            Console.WriteLine($"Sending 'DispatchEvent' for event type '{message.EventType.FullName}' with event '{message.Event}'.");

            return _messageEndPoint.SendAsync(message);
        }

        public Task NotifyActivationAsync()
        {
            var message = new ActivateEventForwarding(typeof(TEvent));

            Console.WriteLine($"Sending 'ActivateEventForwarding' for event type '{message.EventType.FullName}'.");

            return _messageEndPoint.SendAsync(message);
        }

        public Task NotifyDeactivationAsync()
        {
            var message = new DeactivateEventForwarding(typeof(TEvent));

            Console.WriteLine($"Sending 'DeactivateEventForwarding' for event type '{message.EventType.FullName}'.");

            return _messageEndPoint.SendAsync(message);
        }
    }
}
