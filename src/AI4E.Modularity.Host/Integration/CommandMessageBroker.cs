using System;
using System.Threading.Tasks;
using AI4E.Async;

namespace AI4E.Modularity.Integration
{
    public sealed class CommandMessageBroker :
        IMessageHandler<RegisterCommandForwarding>,
        IMessageHandler<UnregisterCommandForwarding>,
        IMessageHandler<DispatchCommand, CommandDispatchResult>
    {
        private readonly IHostCommandDispatcher _commandDispatcher;

        public CommandMessageBroker(IHostCommandDispatcher commandDispatcher)
        {
            if (commandDispatcher == null)
                throw new ArgumentNullException(nameof(commandDispatcher));

            _commandDispatcher = commandDispatcher;
        }

        public Task HandleAsync(RegisterCommandForwarding message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Console.WriteLine($"Received 'RegisterCommandForwarding' for command type '{message.CommandType.FullName}'.");

            return _commandDispatcher.RegisterForwardingAsync(message.CommandType);
        }

        public Task HandleAsync(UnregisterCommandForwarding message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Console.WriteLine($"Received 'UnregisterCommandForwarding' for command type '{message.CommandType.FullName}'.");

            return _commandDispatcher.UnregisterForwardingAsync(message.CommandType);
        }

        public async ICovariantAwaitable<CommandDispatchResult> HandleAsync(DispatchCommand message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Console.WriteLine($"Received 'DispatchCommand' for command type '{message.CommandType.FullName}' with command '{message.Command}'.");

            var answer = new CommandDispatchResult(await _commandDispatcher.DispatchAsync(message.CommandType, message.Command));

            return answer;
        }
    }
}
