using System;
using System.Threading.Tasks;
using AI4E.Integration;

namespace AI4E.Modularity.Integration
{
    public sealed class EventHandlerProxy<TEvent> :
        IEventHandler<TEvent>,
        IHandlerProvider<IEventHandler<TEvent>>,
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

        IEventHandler<TEvent> IHandlerProvider<IEventHandler<TEvent>>.GetHandler(IServiceProvider serviceProvider)
        {
            return this;
        }

        public async Task<IEventResult> HandleAsync(TEvent evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            var message = new DispatchEvent(typeof(TEvent), evt);

            Console.WriteLine($"Sending 'DispatchEvent' for event type '{message.EventType.FullName}' with event '{message.Event}'.");

            var answer = await _messageEndPoint.SendAsync<DispatchEvent, EventDispatchResult>(message);

            return answer.EventResult;
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
