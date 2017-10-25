/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        CommandHandlerProvider.cs 
 * Types:           AI4E.CommandHandlerProvider'1
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   26.08.2017 
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
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace AI4E
{
    public sealed class CommandHandlerProvider<TCommand> : IContextualProvider<ICommandHandler<TCommand>>
    {
        private readonly Type _type;
        private readonly CommandHandlerActionDescriptor _memberDescriptor;

        public CommandHandlerProvider(Type type, CommandHandlerActionDescriptor memberDescriptor)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            _type = type;
            _memberDescriptor = memberDescriptor;
        }

        public ICommandHandler<TCommand> ProvideInstance(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            // Create a new instance of the handler type.
            var handler = ActivatorUtilities.CreateInstance(serviceProvider, _type);

            Debug.Assert(handler != null);

            var contextProperty = _type.GetProperties().FirstOrDefault(p => p.PropertyType == typeof(CommandDispatchContext) &&
                                                                            p.CanWrite &&
                                                                            p.IsDefined<CommandDispatchContextAttribute>());

            if (contextProperty != null)
            {
                var context = new CommandDispatchContext { DispatchServices = serviceProvider };

                contextProperty.SetValue(handler, context);
            }

            return new CommandHandlerInvoker<TCommand>(handler, _memberDescriptor, serviceProvider);
        }
    }
}
