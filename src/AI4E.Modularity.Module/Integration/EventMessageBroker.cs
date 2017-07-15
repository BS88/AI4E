using System;
using System.Threading.Tasks;
using AI4E.Async;

namespace AI4E.Modularity.Integration
{
    public sealed class EventMessageBroker :
        IMessageHandler<DispatchEvent, EventDispatchResult>,
        IMessageHandler<ActivateEventForwarding>,
        IMessageHandler<DeactivateEventForwarding>
    {
        private readonly INonGenericRemoteEventDispatcher _remoteEventDispatcher;

        public EventMessageBroker(INonGenericRemoteEventDispatcher remoteEventDispatcher)
        {
            if (remoteEventDispatcher == null)
                throw new ArgumentNullException(nameof(remoteEventDispatcher));

            _remoteEventDispatcher = remoteEventDispatcher;
        }

        public async ICovariantAwaitable<EventDispatchResult> HandleAsync(DispatchEvent message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Console.WriteLine($"Receiving 'DispatchEvent' for event type '{message.EventType.FullName}' with event '{message.Event}'.");

            return new EventDispatchResult(await _remoteEventDispatcher.LocalDispatchAsync(message.EventType, message.Event));
        }

        public Task HandleAsync(ActivateEventForwarding message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Console.WriteLine($"Receiving 'ActivateEventForwarding' for event type '{message.EventType.FullName}'.");

            _remoteEventDispatcher.NotifyForwardingActive(message.EventType);

            return Task.CompletedTask;
        }

        public Task HandleAsync(DeactivateEventForwarding message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Console.WriteLine($"Receiving 'DeactivateEventForwarding' for event type '{message.EventType.FullName}'.");

            _remoteEventDispatcher.NotifyForwardingInactive(message.EventType);

            return Task.CompletedTask;
        }


    }
}
