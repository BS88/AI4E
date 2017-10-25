/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        ICommandDispatcher.cs 
 * Types:           (1) AI4E.ICommandDispatcher
 *                  (2) AI4E.ICommandDispatcher'1
 *                  (3) AI4E.ISecureCommandDispatcher
 *                  (4) AI4E.ISecureCommandDispatcher'1
 *                  (5) AI4E.INonGenericCommandDispatcher
 *                  (6) AI4E.ITypedNonGenericCommandDispatcher
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

using System;
using System.Threading.Tasks;

namespace AI4E
{
    /// <summary>
    /// Represents a command dispatcher that dispatches commands to command handlers.
    /// </summary>
    public interface ICommandDispatcher : INonGenericCommandDispatcher
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
        Task<IHandlerRegistration<ICommandHandler<TCommand>>> RegisterAsync<TCommand>(IContextualProvider<ICommandHandler<TCommand>> commandHandlerFactory); // TODO: Correct xml-comments

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
        /// The <see cref="Task{TResult}.Result"/> contains a <see cref="ICommandResult"/> indicating command handling state.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
        Task<ICommandResult> DispatchAsync<TCommand>(TCommand command);
    }

    /// <summary>
    /// Represents a typed command dispatcher that dispatches commands to command handlers.
    /// </summary>
    /// <typeparam name="TCommand">The type of command.</typeparam>
    public interface ICommandDispatcher<TCommand> : ITypedNonGenericCommandDispatcher
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
        Task<IHandlerRegistration<ICommandHandler<TCommand>>> RegisterAsync(IContextualProvider<ICommandHandler<TCommand>> commandHandlerFactory); // TODO: Correct xml-comments

        /// <summary>
        /// Asynchronously dispatches a command.
        /// </summary>
        /// <param name="command">The command to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains a <see cref="ICommandResult"/> indicating command handling state.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
        Task<ICommandResult> DispatchAsync(TCommand command);
    }

    /// <summary>
    /// Represents a command dispatcher that controls access.
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
        bool IsRegistrationAuthorized<TCommand>(IContextualProvider<ICommandHandler<TCommand>> commandHandlerFactory);

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
    /// Represents a typed command dispatcher that controls access.
    /// </summary>
    public interface ISecureCommandDispatcher<TCommand> : ICommandDispatcher<TCommand>
    {
        /// <summary>
        /// Returns a boolean value indicating whether registering the specified command handler is authorized.
        /// </summary>
        /// <param name="commandHandler">The command handler that shall be registered.</param>
        /// <returns>True if registering <paramref name="commandHandler"/> is authorized, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="commandHandler"/> is null.</exception>
        bool IsRegistrationAuthorized(IContextualProvider<ICommandHandler<TCommand>> commandHandler);

        /// <summary>
        /// Returns a boolean value indicating whether dispatching the specified command handler is authorized.
        /// </summary>
        /// <param name="command">The command that shall be dispatched.</param>
        /// <returns>True if dispatching <paramref name="command"/> is authorized, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
        bool IsDispatchAuthorized(TCommand command);
    }

    /// <summary>
    /// Represents a non-generic command dispatcher that dispatches commands to command handlers.
    /// </summary>
    public interface INonGenericCommandDispatcher
    {
        /// <summary>
        /// Returns a typed non-generic command dispatcher for the specified command type.
        /// </summary>
        /// <param name="commandType">The type of command.</param>
        /// <returns>A typed non-gemeric command dispatcher for commands of type <paramref name="commandType"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="commandType"/> is null.</exception>
        ITypedNonGenericCommandDispatcher GetTypedDispatcher(Type commandType);

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
        Task<ICommandResult> DispatchAsync(Type commandType, object command);
    }

    /// <summary>
    /// Represents a non-generic typed command dispatcher that dispatches commands to command handlers.
    /// </summary>
    public interface ITypedNonGenericCommandDispatcher
    {
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
        Task<ICommandResult> DispatchAsync(object command);

        /// <summary>
        /// Gets the type of commands the dispatcher can handler.
        /// </summary>
        Type CommandType { get; }
    }
}
