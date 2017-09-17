/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        CommandDispatcherExtension.cs 
 * Types:           AI4E.Integration.CommandDispatcherExtension
 *                  AI4E.Integration.CommandDispatcherExtension.AnonymousCommandHandler'1
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   01.07.2017 
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
using System.Diagnostics;
using System.Threading.Tasks;

namespace AI4E.Integration
{
    /// <summary>
    /// Defines extensions for the <see cref="ICommandDispatcher"/> interface.
    /// </summary>
    public static class CommandDispatcherExtension
    {
        /// <summary>
        /// Asynchronously registers an anonymous command handler for the specified type of command.
        /// </summary>
        /// <typeparam name="TCommand">The type of command.</typeparam>
        /// <param name="commandDispatcher">The command dispatcher.</param>
        /// <param name="handler">The command handler that shall be registered.</param>
        /// <returns>
        /// A <see cref="IHandlerRegistration"/> representing the asynchronous operation.
        /// The <see cref="IHandlerRegistration"/> cancels the handler registration if completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if either <paramref name="commandDispatcher"/> or <paramref name="handler"/> is null.</exception>
        public static Task<IHandlerRegistration<ICommandHandler<TCommand>>> OnCommand<TCommand>(this ICommandDispatcher commandDispatcher, Func<TCommand, Task<ICommandResult>> handler) // TODO: Correct xml-comments
        {
            if (commandDispatcher == null)
                throw new ArgumentNullException(nameof(commandDispatcher));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            return commandDispatcher.RegisterAsync(new AnonymousCommandHandler<TCommand>(handler));
        }

        /// <summary>
        /// Asynchronously dispatches a command of the specified command type.
        /// </summary>
        /// <param name="commandDispatcher">The command dispatcher.</param>
        /// <param name="command">The command to dispatch.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains a <see cref="ICommandResult"/> indicating command handling state.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
        public static Task<ICommandResult> DispatchAsync(this INonGenericCommandDispatcher commandDispatcher, object command)
        {
            if (commandDispatcher == null)
                throw new ArgumentNullException(nameof(commandDispatcher));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            return commandDispatcher.DispatchAsync(command.GetType(), command);
        }

        private class AnonymousCommandHandler<TCommand> : ICommandHandler<TCommand>, IContextualProvider<ICommandHandler<TCommand>>
        {
            private readonly Func<TCommand, Task<ICommandResult>> _handler;

            public AnonymousCommandHandler(Func<TCommand, Task<ICommandResult>> handler)
            {
                Debug.Assert(handler != null);
                _handler = handler;
            }

            public ICommandHandler<TCommand> ProvideInstance(IServiceProvider serviceProvider) { return this; }

            public Task<ICommandResult> HandleAsync(TCommand command)
            {
                if (command == null)
                    throw new ArgumentNullException(nameof(command));

                return _handler(command);
            }
        }
    }
}
