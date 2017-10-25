using System;
using System.Threading.Tasks;
using AI4E;

namespace AI4E.Modularity.Integration
{
    public sealed class CommandMessageTranslator : ICommandMessageTranslator
    {
        private readonly IMessageEndPoint _messageEndPoint;

        public CommandMessageTranslator(IMessageEndPoint messageEndPoint)
        {
            if (messageEndPoint == null)
                throw new ArgumentNullException(nameof(messageEndPoint));

            _messageEndPoint = messageEndPoint;
        }

        public Task RegisterForwardingAsync<TCommand>()
        {
            var message = new RegisterCommandForwarding(typeof(TCommand));

            Console.WriteLine($"Sending 'RegisterCommandForwarding' for command type '{message.CommandType.FullName}'.");

            return _messageEndPoint.SendAsync(message);
        }

        public Task UnregisterForwardingAsync<TCommand>()
        {
            var message = new UnregisterCommandForwarding(typeof(TCommand));

            Console.WriteLine($"Sending 'UnregisterCommandForwarding' for command type '{message.CommandType.FullName}'.");

            return _messageEndPoint.SendAsync(message);
        }

        public async Task<ICommandResult> DispatchAsync<TCommand>(TCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var message = new DispatchCommand(typeof(TCommand), command);

            Console.WriteLine($"Sending 'DispatchCommand' for command type '{message.CommandType.FullName}' with command '{message.Command}'.");

            var answer = await _messageEndPoint.SendAsync<DispatchCommand, CommandDispatchResult>(message);

            return answer.CommandResult;
        }
    }
}
