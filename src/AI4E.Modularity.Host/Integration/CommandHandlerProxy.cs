using System;
using System.Threading.Tasks;
using AI4E.Integration;

namespace AI4E.Modularity.Integration
{
    public sealed class CommandHandlerProxy<TCommand> :
        ICommandHandler<TCommand>,
        IContextualProvider<ICommandHandler<TCommand>>,
        IActivationNotifyable,
        IDeactivationNotifyable
    {
        private readonly IMessageEndPoint _messageEndPoint;

        public CommandHandlerProxy(IMessageEndPoint messageEndPoint)
        {
            if (messageEndPoint == null)
                throw new ArgumentNullException(nameof(messageEndPoint));

            _messageEndPoint = messageEndPoint;
        }

        public async Task<ICommandResult> HandleAsync(TCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var message = new DispatchCommand(typeof(TCommand), command);

            Console.WriteLine($"Sending 'DispatchCommand' for command type '{message.CommandType.FullName}' with command '{message.Command}'.");

            var answer = await _messageEndPoint.SendAsync<DispatchCommand, CommandDispatchResult>(message);

            return answer.CommandResult;
        }

        ICommandHandler<TCommand> IContextualProvider<ICommandHandler<TCommand>>.GetInstance(IServiceProvider serviceProvider) { return this; }

        public Task NotifyActivationAsync()
        {
            var message = new ActivateCommandForwarding(typeof(TCommand));

            Console.WriteLine($"Sending 'ActivateCommandForwarding' for command type '{message.CommandType.FullName}'.");

            return _messageEndPoint.SendAsync(message);
        }

        public Task NotifyDeactivationAsync()
        {
            var message = new DeactivateCommandForwarding(typeof(TCommand));

            Console.WriteLine($"Sending 'DeactivateCommandForwarding' for command type '{message.CommandType.FullName}'.");

            return _messageEndPoint.SendAsync(message);
        }
    }
}
