/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        ICommandHandler.cs
 * Types:           AI4E.Integration.ICommandHandler'1
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   09.05.2017 
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

using System.Threading.Tasks;

namespace AI4E.Integration
{
    /// <summary>
    /// Represents a command handler.
    /// </summary>
    /// <typeparam name="TCommand">The type of command.</typeparam>
    public interface ICommandHandler<in TCommand>
    {
        /// <summary>
        /// Asynchronously handles a command.
        /// </summary>
        /// <param name="command">The command that shall be handled.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains a <see cref="ICommandResult"/> indicating command handling state.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown  if <paramref name="command"/> is null.</exception>
        Task<ICommandResult> HandleAsync(TCommand command);
    }
}
