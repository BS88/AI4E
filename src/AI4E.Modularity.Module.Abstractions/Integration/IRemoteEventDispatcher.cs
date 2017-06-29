using System;
using System.Threading.Tasks;
using AI4E.Integration;

namespace AI4E.Modularity.Integration
{
    public interface IRemoteEventDispatcher : IEventDispatcher
    {
        new IRemoteEventDispatcher<TEvent> GetTypedDispatcher<TEvent>();

        Task RemoteDispatchAsync<TEvent>(TEvent evt);

        void ActivateForwarding<TEvent>();

        void DeactivateForwarding<TEvent>();
    }

    public interface IRemoteEventDispatcher<TEvent> : IEventDispatcher<TEvent>
    {
        Task RemoteDispatchAsync(TEvent evt);

        void ActivateForwarding();

        void DeactivateForwarding();

        bool IsForwardingActive { get; }
    }

    public interface INonGenericRemoteEventDispatcher : INonGenericEventDispatcher
    {
        new ITypedNonGenericRemoteEventDispatcher GetTypedDispatcher(Type eventType);

        Task RemoteDispatchAsync(Type eventType, object evt);

        void ActivateForwarding(Type eventType);

        void DeactivateForwarding(Type eventType);
    }

    public interface ITypedNonGenericRemoteEventDispatcher : ITypedNonGenericEventDispatcher
    {
        Task RemoteDispatchAsync(object evt);

        void ActivateForwarding();

        void DeactivateForwarding();

        bool IsForwardingActive { get; }
    }
}
