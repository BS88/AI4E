using System;
using System.Threading.Tasks;
using AI4E;

namespace AI4E.Modularity.Integration
{
    public interface IHostEventDispatcher
    {
        Task RegisterForwardingAsync(Type eventType);
        Task UnregisterForwardingAsync(Type eventType);

        Task<IAggregateEventResult> DispatchAsync(Type eventType, object evt);

        ITypedHostEventDispatcher GetTypedDispatcher(Type eventType);
    }

    public interface ITypedHostEventDispatcher
    {
        Task RegisterForwardingAsync();
        Task UnregisterForwardingAsync();

        Task<IAggregateEventResult> DispatchAsync(object evt);

        Type EventType { get; }
    }
}
