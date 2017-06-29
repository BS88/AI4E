/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        ICommandDispatcher.cs 
 * Types:           (1) AI4E.Integration.ICommandDispatcher
 *                  (2) AI4E.Integration.ICommandDispatcher'1
 *                  (3) AI4E.Integration.ISecureCommandDispatcher
 *                  (4) AI4E.Integration.ISecureCommandDispatcher'1
 *                  (5) AI4E.Integration.INonGenericCommandDispatcher
 *                  (6) AI4E.Integration.ITypedNonGenericCommandDispatcher
 *                  (7) AI4E.Integration.CommandDispatchException
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   01.06.2017 
 * Status:          Ready
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

using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace AI4E.Integration
{
    /// <summary>
    /// Represents a command dispatcher that dispatches commands to command handlers.
    /// </summary>
    public interface ICommandDispatcher
    {
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
        Task<IHandlerRegistration<ICommandHandler<TCommand>>> RegisterAsync<TCommand>(IHandlerFactory<ICommandHandler<TCommand>> commandHandlerFactory); // TODO: Correct xml-comments

        /// <summary>
        /// Returns a typed command dispatcher for the specified command type.
        /// </summary>
        /// <typeparam name="TCommand">The type of command.</typeparam>
        /// <returns>A typed command dispatcher for command of type <typeparamref name="TCommand"/>.</returns>
        ICommandDispatcher<TCommand> GetTypedDispatcher<TCommand>();

        /// <summary>
        /// Asynchronously dispatches a command.
        /// </summary>
        /// <typeparam name="TCommand">The type of command.</typeparam>
        /// <param name="command">The command to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains a <see cref="CommandResult"/> indicating command handling state.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
        /// <exception cref="CommandDispatchException">Thrown if the command cannot be dispatched.</exception>
        Task<ICommandResult> DispatchAsync<TCommand>(TCommand command);
    }

    /// <summary>
    /// Represents a typed command dispatcher that dispatches commands to command handlers.
    /// </summary>
    /// <typeparam name="TCommand">The type of command.</typeparam>
    public interface ICommandDispatcher<TCommand>
    {
        /// <summary>
        /// Asynchronously registeres a command handler.
        /// </summary>
        /// <param name="commandHandlerFactory">The command handler to register.</param>
        /// <returns>
        /// A <see cref="IHandlerRegistration"/> representing the asynchronous operation.
        /// The <see cref="IHandlerRegistration"/> cancels the handler registration if completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="commandHandlerFactory"/> is null.</exception>
        Task<IHandlerRegistration<ICommandHandler<TCommand>>> RegisterAsync(IHandlerFactory<ICommandHandler<TCommand>> commandHandlerFactory); // TODO: Correct xml-comments

        /// <summary>
        /// Asynchronously dispatches a command.
        /// </summary>
        /// <param name="command">The command to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains a <see cref="CommandResult"/> indicating command handling state.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
        /// <exception cref="CommandDispatchException">Thrown if the command cannot be dispatched.</exception>
        Task<ICommandResult> DispatchAsync(TCommand command);
    }

    /// <summary>
    /// A secured command dispatcher that controls access.
    /// </summary>
    public interface ISecureCommandDispatcher : ICommandDispatcher
    {
        /// <summary>
        /// Returns a boolean value indicating whether registering the specified command handler is authorized.
        /// </summary>
        /// <typeparam name="TCommand">The type of command.</typeparam>
        /// <param name="commandHandlerFactory">The command handler that shall be registered.</param>
        /// <returns>True if registering <paramref name="commandHandlerFactory"/> is authorized, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="commandHandlerFactory"/> is null.</exception>
        bool IsRegistrationAuthorized<TCommand>(IHandlerFactory<ICommandHandler<TCommand>> commandHandlerFactory);

        /// <summary>
        /// Returns a boolean value indicating whether dispatching the specified command handler is authorized.
        /// </summary>
        /// <typeparam name="TCommand">The type of command.</typeparam>
        /// <param name="command">The command that shall be dispatched.</param>
        /// <returns>True if dispatching <paramref name="command"/> is authorized, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
        bool IsDispatchAuthorized<TCommand>(TCommand command);
    }

    /// <summary>
    /// A secured typed command dispatcher that controls access.
    /// </summary>
    public interface ISecureCommandDispatcher<TCommand> : ICommandDispatcher<TCommand>
    {
        /// <summary>
        /// Returns a boolean value indicating whether registering the specified command handler is authorized.
        /// </summary>
        /// <param name="commandHandler">The command handler that shall be registered.</param>
        /// <returns>True if registering <paramref name="commandHandler"/> is authorized, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="commandHandler"/> is null.</exception>
        bool IsRegistrationAuthorized(IHandlerFactory<ICommandHandler<TCommand>> commandHandler);

        /// <summary>
        /// Returns a boolean value indicating whether dispatching the specified command handler is authorized.
        /// </summary>
        /// <param name="command">The command that shall be dispatched.</param>
        /// <returns>True if dispatching <paramref name="command"/> is authorized, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
        bool IsDispatchAuthorized(TCommand command);
    }

    public interface INonGenericCommandDispatcher
    {
        ITypedNonGenericCommandDispatcher GetTypedDispatcher(Type commandType);

        Task<ICommandResult> DispatchAsync(Type commandType, object command);
    }

    public interface ITypedNonGenericCommandDispatcher
    {
        Task<ICommandResult> DispatchAsync(object command);

        Type CommandType { get; }
    }

    /// <summary>
    /// Represents a type of exception that is thrown when a command cannot be dispatched.
    /// </summary>
    [Serializable]
    public class CommandDispatchException : Exception
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CommandDispatchException"/> type with the specified type of command.
        /// </summary>
        /// <param name="commandType">The type of command.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="commandType"/> is null.</exception>
        public CommandDispatchException(Type commandType) : base("The command cannot be dispatched.")
        {
            if (commandType == null)
                throw new ArgumentNullException(nameof(commandType));

            CommandType = commandType;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CommandDispatchException"/> type with the specified type of command and error message.
        /// </summary>
        /// <param name="commandType">The type of command.</param>
        /// <param name="message">A message describing the exception.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="commandType"/> is null.</exception>
        public CommandDispatchException(Type commandType, string message) : base(message)
        {
            if (commandType == null)
                throw new ArgumentNullException(nameof(commandType));

            CommandType = commandType;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CommandDispatchException"/> type with the specified type of command, error message and inner exception.
        /// </summary>
        /// <param name="commandType">The type of command.</param>
        /// <param name="message">A message describing the exception.</param>
        /// <param name="innerException">An exception that caused the command dispatch exception.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="commandType"/> is null.</exception>
        public CommandDispatchException(Type commandType, string message, Exception innerException) : base(message, innerException)
        {
            if (commandType == null)
                throw new ArgumentNullException(nameof(commandType));

            CommandType = commandType;
        }

        protected CommandDispatchException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            CommandType = (Type)info.GetValue(nameof(CommandType), typeof(Type));
        }

        /// <summary>
        /// Gets the type of command.
        /// </summary>
        public Type CommandType { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(CommandType), CommandType);
        }
    }
}
