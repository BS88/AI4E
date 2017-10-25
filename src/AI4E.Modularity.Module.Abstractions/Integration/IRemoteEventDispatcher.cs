using System;
using System.Threading.Tasks;
using AI4E;

namespace AI4E.Modularity.Integration
{
    public interface IRemoteEventDispatcher : IEventDispatcher
    {
        new IRemoteEventDispatcher<TEvent> GetTypedDispatcher<TEvent>();

        Task<IAggregateEventResult> LocalDispatchAsync<TEvent>(TEvent evt);

        void NotifyForwardingActive<TEvent>();
        void NotifyForwardingInactive<TEvent>();
    }

    public interface IRemoteEventDispatcher<TEvent> : IEventDispatcher<TEvent>
    {
        Task<IAggregateEventResult> LocalDispatchAsync(TEvent evt);

        void NotifyForwardingActive();
        void NotifyForwardingInactive();

        bool IsForwardingActive { get; }
    }

    public interface INonGenericRemoteEventDispatcher : INonGenericEventDispatcher
    {
        new ITypedNonGenericRemoteEventDispatcher GetTypedDispatcher(Type eventType);

        Task<IAggregateEventResult> LocalDispatchAsync(Type eventType, object evt);

        void NotifyForwardingActive(Type eventType);
        void NotifyForwardingInactive(Type eventType);
    }

    public interface ITypedNonGenericRemoteEventDispatcher : ITypedNonGenericEventDispatcher
    {
        Task<IAggregateEventResult> LocalDispatchAsync(object evt);

        void NotifyForwardingActive();
        void NotifyForwardingInactive();

        bool IsForwardingActive { get; }
    }
}
