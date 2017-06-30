﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using AI4E.Integration;
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

        public Task<IHandlerRegistration<ICommandHandler<TCommand>>> RegisterAsync<TCommand>(IHandlerFactory<ICommandHandler<TCommand>> commandHandlerFactory)
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

        public void ActivateForwarding<TCommand>()
        {
            GetTypedDispatcher<TCommand>().ActivateForwarding();
        }

        public void DeactiveForwarding<TCommand>()
        {
            GetTypedDispatcher<TCommand>().DeactiveForwarding();
        }

        void INonGenericRemoteCommandDispatcher.ActivateForwarding(Type commandType)
        {
            if (commandType == null)
                throw new ArgumentNullException(nameof(commandType));

            GetTypedDispatcher(commandType).ActivateForwarding();
        }

        void INonGenericRemoteCommandDispatcher.DeactiveForwarding(Type commandType)
        {
            if (commandType == null)
                throw new ArgumentNullException(nameof(commandType));

            GetTypedDispatcher(commandType).DeactiveForwarding();
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

        public Task<IHandlerRegistration<ICommandHandler<TCommand>>> RegisterAsync(IHandlerFactory<ICommandHandler<TCommand>> commandHandlerFactory)
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
                    return handler.GetHandler(scope.ServiceProvider).HandleAsync(command);
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
                        return handler.GetHandler(scope.ServiceProvider).HandleAsync(command);
                    }
                    catch (ConsistencyException)
                    {
                        return Task.FromResult<ICommandResult>(CommandResult.ConcurrencyIssue());
                    }
                    catch (Exception exc)
                    {
                        return Task.FromResult<ICommandResult>(CommandResult.Failure(exc.ToString()));
                    }
                }
            }

            return Task.FromResult<ICommandResult>(CommandResult.DispatchFailure<TCommand>());
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

        public void ActivateForwarding()
        {
            _isForwardingActive = true;
        }

        public void DeactiveForwarding()
        {
            _isForwardingActive = false;
        }

        Type ITypedNonGenericCommandDispatcher.CommandType => typeof(TCommand);

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
