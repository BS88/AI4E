using System;
using System.Threading.Tasks;
using AI4E.Integration;

namespace AI4E.Modularity.Integration
{
    public interface IHostQueryDispatcher
    {
        Task RegisterForwardingAsync(Type queryType);
        Task UnregisterForwardingAsync(Type queryType);

        Task<IQueryResult> DispatchAsync(Type queryType, object query);

        ITypedHostQueryDispatcher GetTypedDispatcher(Type queryType);
    }

    public interface ITypedHostQueryDispatcher
    {
        Task RegisterForwardingAsync();
        Task UnregisterForwardingAsync();

        Task<IQueryResult> DispatchAsync(object query);

        Type QueryType { get; }
    }
}
