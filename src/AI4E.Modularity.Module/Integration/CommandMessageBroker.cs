using System;
using System.Threading.Tasks;
using AI4E.Async;
using AI4E.Integration;

namespace AI4E.Modularity.Integration
{
    public sealed class CommandMessageBroker :
        IMessageHandler<DispatchCommand, CommandDispatchResult>,
        IMessageHandler<ActivateCommandForwarding>,
        IMessageHandler<DeactivateCommandForwarding>
    {
        private readonly INonGenericRemoteCommandDispatcher _remoteCommandDispatcher;

        public CommandMessageBroker(INonGenericRemoteCommandDispatcher remoteCommandDispatcher)
        {
            if (remoteCommandDispatcher == null)
                throw new ArgumentNullException(nameof(remoteCommandDispatcher));

            _remoteCommandDispatcher = remoteCommandDispatcher;
        }

        public async ICovariantAwaitable<CommandDispatchResult> HandleAsync(DispatchCommand message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Console.WriteLine($"Received 'DispatchCommand' for command type '{message.CommandType.FullName}' with command '{message.Command}'.");

            var answer = new CommandDispatchResult(await _remoteCommandDispatcher.RemoteDispatchAsync(message.CommandType, message.Command));

            return answer;
        }

        public Task HandleAsync(ActivateCommandForwarding message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Console.WriteLine($"Received 'ActivateCommandForwarding' for command type '{message.CommandType.FullName}'.");

            _remoteCommandDispatcher.ActivateForwarding(message.CommandType);

            return Task.CompletedTask;
        }

        public Task HandleAsync(DeactivateCommandForwarding message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Console.WriteLine($"Received 'DeactivateCommandForwarding' for command type '{message.CommandType.FullName}'.");

            _remoteCommandDispatcher.DeactiveForwarding(message.CommandType);

            return Task.CompletedTask;
        }
    }
}
