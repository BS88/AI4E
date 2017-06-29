using System;
using System.Threading.Tasks;
using AI4E.Integration;

namespace AI4E.Modularity.Integration
{
    public interface IHostCommandDispatcher
    {
        Task RegisterForwardingAsync(Type commandType);
        Task UnregisterForwardingAsync(Type commandType);

        Task<ICommandResult> DispatchAsync(Type commandType, object command);

        ITypedHostCommandDispatcher GetTypedDispatcher(Type commandType);
    }

    public interface ITypedHostCommandDispatcher
    {
        Task RegisterForwardingAsync();
        Task UnregisterForwardingAsync();

        Task<ICommandResult> DispatchAsync(object command);

        Type CommandType { get; }
    }
}
