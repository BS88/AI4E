using System;
using System.Threading.Tasks;
using AI4E.Integration;

namespace AI4E.Modularity.Integration
{
    public interface IRemoteQueryDispatcher : IQueryDispatcher
    {
        new IRemoteQueryDispatcher<TQuery, TResult> GetTypedDispatcher<TQuery, TResult>();

        Task<TResult> RemoteDispatchAsync<TQuery, TResult>(TQuery query);

        void ActivateForwarding<TQuery, TResult>();
        void DeactivateForwarding<TQuery, TResult>();
    }

    public interface IRemoteQueryDispatcher<TQuery, TResult> : IQueryDispatcher<TQuery, TResult>
    {
        Task<TResult> RemoteDispatchAsync(TQuery query);

        void ActivateForwarding();
        void DeactivateForwarding();
    }

    public interface INonGenericRemoteQueryDispatcher : INonGenericQueryDispatcher
    {
        new ITypedNonGenericRemoteQueryDispatcher GetTypedDispatcher(Type queryType, Type resultType);

        Task<object> RemoteDispatchAsync(Type queryType, Type resultType, object query);

        void ActivateForwarding(Type queryType, Type resultType);
        void DeactivateForwarding(Type queryType, Type resultType);
    }

    public interface ITypedNonGenericRemoteQueryDispatcher : ITypedNonGenericQueryDispatcher
    {
        Task<object> RemoteDispatchAsync(object query);

        void ActivateForwarding();
        void DeactivateForwarding();
    }
}
