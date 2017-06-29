using System;
using System.Threading.Tasks;

namespace AI4E.Modularity.Integration
{
    public interface IHostQueryDispatcher
    {
        Task RegisterForwardingAsync(Type queryType, Type resultType);
        Task UnregisterForwardingAsync(Type queryType, Type resultType);

        Task<object> DispatchAsync(Type queryType, Type resultType, object query);

        ITypedHostQueryDispatcher GetTypedDispatcher(Type queryType, Type resultType);
    }

    public interface ITypedHostQueryDispatcher
    {
        Task RegisterForwardingAsync();
        Task UnregisterForwardingAsync();

        Task<object> DispatchAsync(object query);

        Type QueryType { get; }
        Type ResultType { get; }
    }
}
