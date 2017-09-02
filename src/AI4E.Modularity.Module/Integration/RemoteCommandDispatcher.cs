using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using AI4E.Integration;
using AI4E.Integration.CommandResults;
using Microsoft.Extensions.DependencyInjection;

namespace AI4E.Modularity.Integration
{
    public sealed class RemoteCommandDispatcher : IRemoteCommandDispatcher, INonGenericRemoteCommandDispatcher
    {
        private static readonly Type _typedDispatcherType = typeof(RemoteCommandDispatcher<>);

        private readonly ICommandMessageTranslator _commandMessageTranslator;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<Type, ITypedNonGenericRemoteCommandDispatcher> _typedDispatchers
            = new ConcurrentDictionary<Type, ITypedNonGenericRemoteCommandDispatcher>();

        public RemoteCommandDispatcher(ICommandMessageTranslator commandMessageTranslator, IServiceProvider serviceProvider)
        {
            if (commandMessageTranslator == null)
                throw new ArgumentNullException(nameof(commandMessageTranslator));

            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            _commandMessageTranslator = commandMessageTranslator;
            _serviceProvider = serviceProvider;
        }

        public Task<IHandlerRegistration<ICommandHandler<TCommand>>> RegisterAsync<TCommand>(IContextualProvider<ICommandHandler<TCommand>> commandHandlerFactory)
        {
            if (commandHandlerFactory == null)
                throw new ArgumentNullException(nameof(commandHandlerFactory));

            return GetTypedDispatcher<TCommand>().RegisterAsync(commandHandlerFactory);
        }

        public IRemoteCommandDispatcher<TCommand> GetTypedDispatcher<TCommand>()
        {
            return _typedDispatchers.GetOrAdd(typeof(TCommand), t => new RemoteCommandDispatcher<TCommand>(_commandMessageTranslator, _serviceProvider)) as IRemoteCommandDispatcher<TCommand>;
        }

        ICommandDispatcher<TCommand> ICommandDispatcher.GetTypedDispatcher<TCommand>()
        {
            return GetTypedDispatcher<TCommand>();
        }

        public ITypedNonGenericRemoteCommandDispatcher GetTypedDispatcher(Type commandType)
        {
            if (commandType == null)
                throw new ArgumentNullException(nameof(commandType));

            return _typedDispatchers.GetOrAdd(commandType, type =>
            {
                var result = Activator.CreateInstance(_typedDispatcherType.MakeGenericType(commandType), _commandMessageTranslator, _serviceProvider);
                Debug.Assert(result != null);
                return result as ITypedNonGenericRemoteCommandDispatcher;
            });
        }

        ITypedNonGenericCommandDispatcher INonGenericCommandDispatcher.GetTypedDispatcher(Type commandType)
        {
            return GetTypedDispatcher(commandType);
        }

        public Task<ICommandResult> DispatchAsync<TCommand>(TCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            return GetTypedDispatcher<TCommand>().DispatchAsync(command);
        }

        public Task<ICommandResult> DispatchAsync(Type commandType, object command)
        {
            if (commandType == null)
                throw new ArgumentNullException(nameof(commandType));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            return GetTypedDispatcher(commandType).DispatchAsync(command);
        }

        public Task<ICommandResult> LocalDispatchAsync<TCommand>(TCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            return GetTypedDispatcher<TCommand>().LocalDispatchAsync(command);
        }

        public Task<ICommandResult> LocalDispatchAsync(Type commandType, object command)
        {
            if (commandType == null)
                throw new ArgumentNullException(nameof(commandType));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            return GetTypedDispatcher(commandType).LocalDispatchAsync(command);
        }

        public void NotifyForwardingActive<TCommand>()
        {
            GetTypedDispatcher<TCommand>().NotifyForwardingActive();
        }

        public void NotifyForwardingInactive<TCommand>()
        {
            GetTypedDispatcher<TCommand>().NotifyForwardingInactive();
        }

        void INonGenericRemoteCommandDispatcher.NotifyForwardingActive(Type commandType)
        {
            if (commandType == null)
                throw new ArgumentNullException(nameof(commandType));

            GetTypedDispatcher(commandType).NotifyForwardingActive();
        }

        void INonGenericRemoteCommandDispatcher.NotifyForwardingInactive(Type commandType)
        {
            if (commandType == null)
                throw new ArgumentNullException(nameof(commandType));

            GetTypedDispatcher(commandType).NotifyForwardingInactive();
        }
    }

    public sealed class RemoteCommandDispatcher<TCommand> : IRemoteCommandDispatcher<TCommand>, ITypedNonGenericRemoteCommandDispatcher
    {
        private readonly ICommandMessageTranslator _commandMessageTranslator;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAsyncSingleHandlerRegistry<ICommandHandler<TCommand>> _handlerRegistry;

        private bool _isForwardingActive;

        public RemoteCommandDispatcher(ICommandMessageTranslator commandMessageTranslator, IServiceProvider serviceProvider)
        {
            if (commandMessageTranslator == null)
                throw new ArgumentNullException(nameof(commandMessageTranslator));

            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            _commandMessageTranslator = commandMessageTranslator;
            _serviceProvider = serviceProvider;
            _handlerRegistry = new AsyncSingleHandlerRegistry<ICommandHandler<TCommand>>(new DispatchForwarding(this));
        }

        Type ITypedNonGenericCommandDispatcher.CommandType => typeof(TCommand);

        public bool IsForwardingActive => _isForwardingActive;

        public Task<IHandlerRegistration<ICommandHandler<TCommand>>> RegisterAsync(IContextualProvider<ICommandHandler<TCommand>> commandHandlerFactory)
        {
            if (commandHandlerFactory == null)
                throw new ArgumentNullException(nameof(commandHandlerFactory));

            return HandlerRegistration.CreateRegistrationAsync(_handlerRegistry, commandHandlerFactory);
        }

        public Task<ICommandResult> DispatchAsync(TCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (_isForwardingActive && _handlerRegistry.TryGetHandler(out var handler))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    return handler.GetInstance(scope.ServiceProvider).HandleAsync(command);
                }
            }

            return _commandMessageTranslator.DispatchAsync(command);
        }

        public Task<ICommandResult> DispatchAsync(object command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (!(command is TCommand typedCommand))
            {
                throw new ArgumentException("The argument is not of the specified command type or a derived type.", nameof(command));
            }

            return DispatchAsync(typedCommand);
        }

        public Task<ICommandResult> LocalDispatchAsync(TCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (_handlerRegistry.TryGetHandler(out var handler))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    try
                    {
                        return handler.GetInstance(scope.ServiceProvider).HandleAsync(command);
                    }
                    catch (ConsistencyException)
                    {
                        return Task.FromResult<ICommandResult>(new ConcurrencyIssueCommandResult());
                    }
                    catch (Exception exc)
                    {
                        return Task.FromResult<ICommandResult>(new FailureCommandResult(exc.ToString()));
                    }
                }
            }

            return Task.FromResult<ICommandResult>(new CommandDispatchFailureCommandResult(typeof(TCommand)));
        }

        public Task<ICommandResult> LocalDispatchAsync(object command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (!(command is TCommand typedCommand))
            {
                throw new ArgumentException("The argument is not of the specified command type or a derived type.", nameof(command));
            }

            return LocalDispatchAsync(typedCommand);
        }

        public void NotifyForwardingActive()
        {
            _isForwardingActive = true;
        }

        public void NotifyForwardingInactive()
        {
            _isForwardingActive = false;
        }

        private sealed class DispatchForwarding : IDispatchForwarding
        {
            private readonly RemoteCommandDispatcher<TCommand> _commandDispatcher;

            public DispatchForwarding(RemoteCommandDispatcher<TCommand> commandDispatcher)
            {
                Debug.Assert(commandDispatcher != null);

                _commandDispatcher = commandDispatcher;
            }

            public Task RegisterForwardingAsync()
            {
                if (_commandDispatcher._isForwardingActive)
                {
                    return Task.CompletedTask;
                }

                return _commandDispatcher._commandMessageTranslator.RegisterForwardingAsync<TCommand>();
            }

            public Task UnregisterForwardingAsync()
            {
                return _commandDispatcher._commandMessageTranslator.UnregisterForwardingAsync<TCommand>();
            }
        }
    }
}
