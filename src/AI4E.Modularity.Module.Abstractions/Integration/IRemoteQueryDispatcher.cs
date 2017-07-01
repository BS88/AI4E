using System;
using System.Threading.Tasks;
using AI4E.Integration;

namespace AI4E.Modularity.Integration
{
    public interface IRemoteQueryDispatcher : IQueryDispatcher
    {
        new IRemoteQueryDispatcher<TQuery, TResult> GetTypedDispatcher<TQuery, TResult>();

        Task<TResult> LocalDispatchAsync<TQuery, TResult>(TQuery query);

        void NotifyForwardingActive<TQuery, TResult>();
        void NotifyForwardingInactive<TQuery, TResult>();
    }

    public interface IRemoteQueryDispatcher<TQuery, TResult> : IQueryDispatcher<TQuery, TResult>
    {
        Task<TResult> LocalDispatchAsync(TQuery query);

        void NotifyForwardingActive();
        void NotifyForwardingInactive();

        bool IsForwardingActive { get; }
    }

    public interface INonGenericRemoteQueryDispatcher : INonGenericQueryDispatcher
    {
        new ITypedNonGenericRemoteQueryDispatcher GetTypedDispatcher(Type queryType, Type resultType);

        Task<object> LocalDispatchAsync(Type queryType, Type resultType, object query);

        void NotifyForwardingActive(Type queryType, Type resultType);
        void NotifyForwardingInactive(Type queryType, Type resultType);
    }

    public interface ITypedNonGenericRemoteQueryDispatcher : ITypedNonGenericQueryDispatcher
    {
        Task<object> LocalDispatchAsync(object query);

        void NotifyForwardingActive();
        void NotifyForwardingInactive();

        bool IsForwardingActive { get; }
    }
}
