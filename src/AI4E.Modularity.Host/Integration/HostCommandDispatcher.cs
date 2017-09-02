using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using AI4E.Integration;
using Nito.AsyncEx;

namespace AI4E.Modularity.Integration
{
    public sealed class HostCommandDispatcher : IHostCommandDispatcher
    {
        private static readonly Type _typedDispatcherType = typeof(HostCommandDispatcher<>);

        private readonly ConcurrentDictionary<Type, ITypedHostCommandDispatcher> _typedDispatcher = new ConcurrentDictionary<Type, ITypedHostCommandDispatcher>();
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IMessageEndPoint _messageEndPoint;

        public HostCommandDispatcher(ICommandDispatcher commandDispatcher, IMessageEndPoint messageEndPoint)
        {
            if (commandDispatcher == null)
                throw new ArgumentNullException(nameof(commandDispatcher));

            if (messageEndPoint == null)
                throw new ArgumentNullException(nameof(messageEndPoint));

            _commandDispatcher = commandDispatcher;
            _messageEndPoint = messageEndPoint;
        }

        public Task RegisterForwardingAsync(Type commandType)
        {
            if (commandType == null)
                throw new ArgumentNullException(nameof(commandType));

            return GetTypedDispatcher(commandType).RegisterForwardingAsync();
        }

        public Task UnregisterForwardingAsync(Type commandType)
        {
            if (commandType == null)
                throw new ArgumentNullException(nameof(commandType));

            return GetTypedDispatcher(commandType).UnregisterForwardingAsync();
        }

        public Task<ICommandResult> DispatchAsync(Type commandType, object command)
        {
            if (commandType == null)
                throw new ArgumentNullException(nameof(commandType));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            return GetTypedDispatcher(commandType).DispatchAsync(command);
        }

        public ITypedHostCommandDispatcher GetTypedDispatcher(Type commandType)
        {
            if (commandType == null)
                throw new ArgumentNullException(nameof(commandType));

            return _typedDispatcher.GetOrAdd(commandType, type =>
            {
                var dispatcher = Activator.CreateInstance(_typedDispatcherType.MakeGenericType(commandType), _commandDispatcher, _messageEndPoint);

                Debug.Assert(dispatcher != null);

                return dispatcher as ITypedHostCommandDispatcher;
            });
        }
    }

    public class HostCommandDispatcher<TCommand> : ITypedHostCommandDispatcher
    {
        private readonly ICommandDispatcher<TCommand> _commandDispatcher;
        private readonly IMessageEndPoint _messageEndPoint;
        private readonly AsyncLock _lock = new AsyncLock();
        private IHandlerRegistration<ICommandHandler<TCommand>> _proxyRegistration = null;

        public HostCommandDispatcher(ICommandDispatcher<TCommand> commandDispatcher, IMessageEndPoint messageEndPoint)
        {
            if (commandDispatcher == null)
                throw new ArgumentNullException(nameof(commandDispatcher));

            if (messageEndPoint == null)
                throw new ArgumentNullException(nameof(messageEndPoint));

            _commandDispatcher = commandDispatcher;
            _messageEndPoint = messageEndPoint;
        }

        public HostCommandDispatcher(ICommandDispatcher commandDispatcher, IMessageEndPoint messageEndPoint)
        {
            if (commandDispatcher == null)
                throw new ArgumentNullException(nameof(commandDispatcher));

            if (messageEndPoint == null)
                throw new ArgumentNullException(nameof(messageEndPoint));

            _commandDispatcher = commandDispatcher.GetTypedDispatcher<TCommand>();
            _messageEndPoint = messageEndPoint;
        }

        public async Task RegisterForwardingAsync()
        {
            using (await _lock.LockAsync())
            {
                IContextualProvider<ICommandHandler<TCommand>> proxy;

                if (_proxyRegistration != null)
                {
                    proxy = _proxyRegistration.Handler;
                }
                else
                {
                    proxy = new CommandHandlerProxy<TCommand>(_messageEndPoint);
                }

                _proxyRegistration = await _commandDispatcher.RegisterAsync(proxy);
            }
        }

        public async Task UnregisterForwardingAsync()
        {
            using (await _lock.LockAsync())
            {
                if (_proxyRegistration != null)
                {
                    _proxyRegistration.Complete();
                    await _proxyRegistration.Completion;
                    _proxyRegistration = null;
                }
            }
        }

        public Task<ICommandResult> DispatchAsync(object command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (!(command is TCommand typedCommand))
            {
                throw new ArgumentException("The argument is not of the specified command type or a derived type.", nameof(command));
            }

            return _commandDispatcher.DispatchAsync(typedCommand);
        }

        Type ITypedHostCommandDispatcher.CommandType => typeof(TCommand);
    }
}
