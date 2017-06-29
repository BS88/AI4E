using System;
using System.Threading.Tasks;

namespace AI4E.Modularity.Integration
{
    public interface IHostEventDispatcher
    {
        Task RegisterForwardingAsync(Type eventType);
        Task UnregisterForwardingAsync(Type eventType);

        Task DispatchAsync(Type eventType, object evt);

        ITypedHostEventDispatcher GetTypedDispatcher(Type eventType);
    }

    public interface ITypedHostEventDispatcher
    {
        Task RegisterForwardingAsync();
        Task UnregisterForwardingAsync();

        Task DispatchAsync(object evt);

        Type EventType { get; }
    }
}
