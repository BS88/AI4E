/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        CommandDispatcher.cs 
 * Types:           (1) AI4E.Integration.CommandDispatcher
 *                  (2) AI4E.Integration.CommandDispatcher'1
 *                  (3) AI4E.Integration.CommandAuthorizationVerifyer
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   01.07.2017 
 * --------------------------------------------------------------------------------------------------------------------
 */

/* License
 * --------------------------------------------------------------------------------------------------------------------
 * This file is part of the AI4E distribution.
 *   (https://gitlab.com/EnterpriseApplicationEquipment/AI4E)
 * Copyright (c) 2017 Andreas Trütschel.
 * 
 * AI4E is free software: you can redistribute it and/or modify  
 * it under the terms of the GNU Lesser General Public License as   
 * published by the Free Software Foundation, version 3.
 *
 * AI4E is distributed in the hope that it will be useful, but 
 * WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU 
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 * --------------------------------------------------------------------------------------------------------------------
 */

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AI4E.Integration
{
    /// <summary>
    /// Represents a command dispatcher that dispatches commands to command handlers.
    /// </summary>
    public sealed class CommandDispatcher : ICommandDispatcher, ISecureCommandDispatcher, INonGenericCommandDispatcher
    {
        private static readonly Type _typedDispatcherType = typeof(CommandDispatcher<>);

        private readonly IServiceProvider _serviceProvider;
        private readonly ICommandAuthorizationVerifyer _authorizationVerifyer;
        private readonly ConcurrentDictionary<Type, ITypedNonGenericCommandDispatcher> _typedDispatchers
            = new ConcurrentDictionary<Type, ITypedNonGenericCommandDispatcher>();

        /// <summary>
        /// Creates a new instance of the <see cref="CommandDispatcher"/> type.
        /// </summary>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> used to obtain services.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="serviceProvider"/> is null.</exception>
        public CommandDispatcher(IServiceProvider serviceProvider) : this(CommandAuthorizationVerifyer.Default, serviceProvider) { }

        /// <summary>
        /// Creates a new instance of the <see cref="CommandDispatcher"/> type.
        /// </summary>
        /// <param name="authorizationVerifyer">An <see cref="ICommandAuthorizationVerifyer"/> that controls authorization or <see cref="CommandAuthorizationVerifyer.Default"/>.</param>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> used to obtain services.</param>
        /// <exception cref="ArgumentNullException">Thrown if either <paramref name="serviceProvider"/> or <paramref name="authorizationVerifyer"/> is null.</exception>
        public CommandDispatcher(ICommandAuthorizationVerifyer authorizationVerifyer, IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            if (authorizationVerifyer == null)
                throw new ArgumentNullException(nameof(authorizationVerifyer));

            _serviceProvider = serviceProvider;
            _authorizationVerifyer = authorizationVerifyer;
        }

        /// <summary>
        /// Asynchronously registers a command handler.
        /// </summary>
        /// <typeparam name="TCommand">The type of command.</typeparam>
        /// <param name="commandHandlerFactory">The command handler to register.</param>
        /// <returns>
        /// A <see cref="IHandlerRegistration"/> representing the asynchronous operation.
        /// The <see cref="IHandlerRegistration"/> cancels the handler registration if completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="commandHandlerFactory"/> is null.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if the access is unauthorized.</exception>
        public Task<IHandlerRegistration<ICommandHandler<TCommand>>> RegisterAsync<TCommand>(IHandlerFactory<ICommandHandler<TCommand>> commandHandlerFactory) // TODO: Correct xml-comments
        {
            if (commandHandlerFactory == null)
                throw new ArgumentNullException(nameof(commandHandlerFactory));

            if (!_authorizationVerifyer.AuthorizeHandlerRegistry())
                throw new UnauthorizedAccessException();

            return GetTypedDispatcher<TCommand>().RegisterAsync(commandHandlerFactory);
        }

        /// <summary>
        /// Returns a typed command dispatcher for the specified command type.
        /// </summary>
        /// <typeparam name="TCommand">The type of command.</typeparam>
        /// <returns>A typed command dispatcher for command of type <typeparamref name="TCommand"/>.</returns>
        public ICommandDispatcher<TCommand> GetTypedDispatcher<TCommand>()
        {
            return _typedDispatchers.GetOrAdd(typeof(TCommand), t => new CommandDispatcher<TCommand>(_authorizationVerifyer, _serviceProvider)) as ICommandDispatcher<TCommand>;
        }

        /// <summary>
        /// Returns a typed non-generic command dispatcher for the specified command type.
        /// </summary>
        /// <param name="commandType">The type of command.</param>
        /// <returns>A typed non-gemeric command dispatcher for commands of type <paramref name="commandType"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="commandType"/> is null.</exception>
        public ITypedNonGenericCommandDispatcher GetTypedDispatcher(Type commandType)
        {
            if (commandType == null)
                throw new ArgumentNullException(nameof(commandType));

            return _typedDispatchers.GetOrAdd(commandType, type =>
            {
                var result = Activator.CreateInstance(_typedDispatcherType.MakeGenericType(commandType), _authorizationVerifyer, _serviceProvider);
                Debug.Assert(result != null);
                return result as ITypedNonGenericCommandDispatcher;
            });
        }

        /// <summary>
        /// Asynchronously dispatches a command.
        /// </summary>
        /// <typeparam name="TCommand">The type of command.</typeparam>
        /// <param name="command">The command to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains a <see cref="ICommandResult"/> indicating command handling state.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
        public Task<ICommandResult> DispatchAsync<TCommand>(TCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            return GetTypedDispatcher<TCommand>().DispatchAsync(command);
        }

        /// <summary>
        /// Asynchronously dispatches a command of the specified command type.
        /// </summary>
        /// <param name="commandType">The type of command.</param>
        /// <param name="command">The command to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains a <see cref="ICommandResult"/> indicating command handling state.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if either <paramref name="commandType"/> or <paramref name="command"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="command"/> is not of type <paramref name="commandType"/> or a derived type.</exception>
        public Task<ICommandResult> DispatchAsync(Type commandType, object command)
        {
            if (commandType == null)
                throw new ArgumentNullException(nameof(commandType));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            return GetTypedDispatcher(commandType).DispatchAsync(command);
        }

        /// <summary>
        /// Returns a boolean value indicating whether registering the specified command handler is authorized.
        /// </summary>
        /// <typeparam name="TCommand">The type of command.</typeparam>
        /// <param name="commandHandlerFactory">The command handler that shall be registered.</param>
        /// <returns>True if registering <paramref name="commandHandlerFactory"/> is authorized, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="commandHandlerFactory"/> is null.</exception>
        public bool IsRegistrationAuthorized<TCommand>(IHandlerFactory<ICommandHandler<TCommand>> commandHandlerFactory)
        {
            if (commandHandlerFactory == null)
                throw new ArgumentNullException(nameof(commandHandlerFactory));

            return _authorizationVerifyer.AuthorizeHandlerRegistry();
        }

        /// <summary>
        /// Returns a boolean value indicating whether dispatching the specified command handler is authorized.
        /// </summary>
        /// <typeparam name="TCommand">The type of command.</typeparam>
        /// <param name="command">The command that shall be dispatched.</param>
        /// <returns>True if dispatching <paramref name="command"/> is authorized, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
        public bool IsDispatchAuthorized<TCommand>(TCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            return _authorizationVerifyer.AuthorizeCommandDispatch(command);
        }
    }

    /// <summary>
    /// Represents a typed command dispatcher that dispatches commands to command handlers.
    /// </summary>
    /// <typeparam name="TCommand">The type of command.</typeparam>
    public sealed class CommandDispatcher<TCommand> : ICommandDispatcher<TCommand>, ISecureCommandDispatcher<TCommand>, ITypedNonGenericCommandDispatcher
    {
        private readonly AsyncSingleHandlerRegistry<ICommandHandler<TCommand>> _handlerRegistry
            = new AsyncSingleHandlerRegistry<ICommandHandler<TCommand>>();
        private readonly IServiceProvider _serviceProvider;
        private readonly ICommandAuthorizationVerifyer _authorizationVerifyer;

        /// <summary>
        /// Creates a new instance of the <see cref="CommandDispatcher{TCommand}"/> type.
        /// </summary>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> used to obtain services.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="serviceProvider"/>is null.</exception>
        public CommandDispatcher(IServiceProvider serviceProvider) : this(CommandAuthorizationVerifyer.Default, serviceProvider) { }

        /// <summary>
        /// Creates a new instance of the <see cref="CommandDispatcher{TCommand}"/> type.
        /// </summary>
        /// <param name="authorizationVerifyer">An <see cref="ICommandAuthorizationVerifyer"/> that controls authorization or <see cref="CommandAuthorizationVerifyer.Default"/>.</param>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> used to obtain services.</param>
        /// <exception cref="ArgumentNullException">Thrown if either <paramref name="serviceProvider"/> or <paramref name="authorizationVerifyer"/> is null.</exception>
        public CommandDispatcher(ICommandAuthorizationVerifyer authorizationVerifyer, IServiceProvider serviceProvider)
        {
            // Remark: If you modify/delete this constructors arguments, remember to also change the non-generic GetTypedDispatcher() method above

            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            if (authorizationVerifyer == null)
                throw new ArgumentNullException(nameof(authorizationVerifyer));

            _serviceProvider = serviceProvider;
            _authorizationVerifyer = authorizationVerifyer;
        }

        Type ITypedNonGenericCommandDispatcher.CommandType => typeof(TCommand);

        /// <summary>
        /// Asynchronously registeres a command handler.
        /// </summary>
        /// <param name="commandHandlerFactory">The command handler to register.</param>
        /// <returns>
        /// A <see cref="IHandlerRegistration"/> representing the asynchronous operation.
        /// The <see cref="IHandlerRegistration"/> cancels the handler registration if completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="commandHandlerFactory"/> is null.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if the access is unauthorized.</exception>
        public Task<IHandlerRegistration<ICommandHandler<TCommand>>> RegisterAsync(IHandlerFactory<ICommandHandler<TCommand>> commandHandlerFactory) // TODO: Correct xml-comments
        {
            if (commandHandlerFactory == null)
                throw new ArgumentNullException(nameof(commandHandlerFactory));

            if (!_authorizationVerifyer.AuthorizeHandlerRegistry())
                throw new UnauthorizedAccessException();

            return HandlerRegistration.CreateRegistrationAsync(_handlerRegistry, commandHandlerFactory);
        }

        /// <summary>
        /// Asynchronously dispatches a command.
        /// </summary>
        /// <param name="command">The command to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains a <see cref="ICommandResult"/> indicating command handling state.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
        public Task<ICommandResult> DispatchAsync(TCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (!_authorizationVerifyer.AuthorizeCommandDispatch(command))
                throw new UnauthorizedAccessException();

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

        /// <summary>
        /// Asynchronously dispatches a command.
        /// </summary>
        /// <param name="command">The command to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains a <see cref="ICommandResult"/> indicating command handling state.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="command"/> is not of type <see cref="CommandType"/> or a derived type.</exception>
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

        /// <summary>
        /// Returns a boolean value indicating whether registering the specified command handler is authorized.
        /// </summary>
        /// <param name="commandHandlerFactory">The command handler that shall be registered.</param>
        /// <returns>True if registering <paramref name="commandHandlerFactory"/> is authorized, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="commandHandlerFactory"/> is null.</exception>
        public bool IsRegistrationAuthorized(IHandlerFactory<ICommandHandler<TCommand>> commandHandlerFactory)
        {
            if (commandHandlerFactory == null)
                throw new ArgumentNullException(nameof(commandHandlerFactory));

            return _authorizationVerifyer.AuthorizeHandlerRegistry();
        }

        /// <summary>
        /// Returns a boolean value indicating whether dispatching the specified command handler is authorized.
        /// </summary>
        /// <param name="command">The command that shall be dispatched.</param>
        /// <returns>True if dispatching <paramref name="command"/> is authorized, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
        public bool IsDispatchAuthorized(TCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            return _authorizationVerifyer.AuthorizeCommandDispatch(command);
        }
    }

    public sealed class CommandAuthorizationVerifyer : ICommandAuthorizationVerifyer
    {
        public static CommandAuthorizationVerifyer Default { get; } = new CommandAuthorizationVerifyer();

        private CommandAuthorizationVerifyer() { }

        public bool AuthorizeHandlerRegistry()
        {
            return true;
        }

        public bool AuthorizeCommandDispatch<TCommand>(TCommand command)
        {
            return true;
        }
    }
}
