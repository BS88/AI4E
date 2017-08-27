/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        CommandHandlerInspector.cs 
 * Types:           AI4E.Integration.CommandHandlerInspector
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AI4E.Integration
{
    public sealed class CommandHandlerInspector
    {
        private readonly Type _type;

        public CommandHandlerInspector(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            _type = type;
        }

        public IEnumerable<CommandHandlerActionDescriptor> GetCommandHandlerDescriptors()
        {
            var members = _type.GetMethods();
            var descriptors = new List<CommandHandlerActionDescriptor>();

            foreach (var member in members)
            {
                if (TryGetHandlingMember(member, out var descriptor))
                {
                    descriptors.Add(descriptor);
                }
            }

            return descriptors;
        }

        private bool TryGetHandlingMember(MethodInfo member, out CommandHandlerActionDescriptor result)
        {
            var parameters = member.GetParameters();

            if (parameters.Length == 0)
            {
                result = default;
                return false;
            }

            if (parameters.Any(p => p.ParameterType.IsByRef))
            {
                result = default;
                return false;
            }

            if (member.IsGenericMethod || member.IsGenericMethodDefinition)
            {
                result = default;
                return false;
            }

            if (member.IsDefined<NoCommandHandlerActionAttribute>())
            {
                result = default;
                return false;
            }

            var commandType = parameters[0].ParameterType;

            var actionAttribute = member.GetCustomAttribute<CommandHandlerActionAttribute>();

            if (actionAttribute != null && actionAttribute.CommandType != null)
            {
                if (!commandType.IsAssignableFrom(actionAttribute.CommandType))
                {
                    throw new InvalidOperationException();
                }

                commandType = actionAttribute.CommandType;
            }

            // Synchronous handler
            if ((member.Name == "Handle" || actionAttribute != null) &&
                (member.ReturnType == typeof(void) || !typeof(Task).IsAssignableFrom(member.ReturnType)))
            {
                result = new CommandHandlerActionDescriptor(commandType, member);
                return true;
            }

            // Asynchronous handler
            if ((member.Name == "HandleAsync" || actionAttribute != null) &&
                (typeof(Task).IsAssignableFrom(member.ReturnType)))
            {
                result = new CommandHandlerActionDescriptor(commandType, member);
                return true;
            }

            result = default;
            return false;
        }
    }
}
