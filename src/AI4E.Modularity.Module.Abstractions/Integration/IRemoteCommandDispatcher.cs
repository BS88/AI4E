using System;
using System.Threading.Tasks;
using AI4E;

namespace AI4E.Modularity.Integration
{
    public interface IRemoteCommandDispatcher : ICommandDispatcher
    {
        new IRemoteCommandDispatcher<TCommand> GetTypedDispatcher<TCommand>();

        Task<ICommandResult> LocalDispatchAsync<TCommand>(TCommand command);

        void NotifyForwardingActive<TCommand>();
        void NotifyForwardingInactive<TCommand>();
    }

    public interface IRemoteCommandDispatcher<TCommand> : ICommandDispatcher<TCommand>
    {
        Task<ICommandResult> LocalDispatchAsync(TCommand command);

        void NotifyForwardingActive();
        void NotifyForwardingInactive();

        bool IsForwardingActive { get; }
    }

    public interface INonGenericRemoteCommandDispatcher : INonGenericCommandDispatcher
    {
        new ITypedNonGenericRemoteCommandDispatcher GetTypedDispatcher(Type commandType);

        Task<ICommandResult> LocalDispatchAsync(Type commandType, object command);

        void NotifyForwardingActive(Type commandType);
        void NotifyForwardingInactive(Type commandType);
    }

    public interface ITypedNonGenericRemoteCommandDispatcher : ITypedNonGenericCommandDispatcher
    {
        Task<ICommandResult> LocalDispatchAsync(object command);

        void NotifyForwardingActive();
        void NotifyForwardingInactive();

        bool IsForwardingActive { get; }
    }
}
