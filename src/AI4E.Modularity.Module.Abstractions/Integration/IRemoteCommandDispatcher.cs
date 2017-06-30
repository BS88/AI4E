using System;
using System.Threading.Tasks;
using AI4E.Integration;

namespace AI4E.Modularity.Integration
{
    public interface IRemoteCommandDispatcher : ICommandDispatcher
    {
        new IRemoteCommandDispatcher<TCommand> GetTypedDispatcher<TCommand>();

        Task<ICommandResult> LocalDispatchAsync<TCommand>(TCommand command);

        void ActivateForwarding<TCommand>();
        void DeactiveForwarding<TCommand>();
    }

    public interface IRemoteCommandDispatcher<TCommand> : ICommandDispatcher<TCommand>
    {
        Task<ICommandResult> LocalDispatchAsync(TCommand command);

        void ActivateForwarding();
        void DeactiveForwarding();
    }

    public interface INonGenericRemoteCommandDispatcher : INonGenericCommandDispatcher
    {
        Task<ICommandResult> LocalDispatchAsync(Type commandType, object command);

        void ActivateForwarding(Type commandType);
        void DeactiveForwarding(Type commandType);

        new ITypedNonGenericRemoteCommandDispatcher GetTypedDispatcher(Type commandType);
    }

    public interface ITypedNonGenericRemoteCommandDispatcher : ITypedNonGenericCommandDispatcher
    {
        Task<ICommandResult> LocalDispatchAsync(object command);

        void ActivateForwarding();
        void DeactiveForwarding();
    }
}
