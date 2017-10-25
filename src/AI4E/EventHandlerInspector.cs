/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        EventHandlerInspector.cs 
 * Types:           AI4E.Integration.EventHandlerInspector
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   27.08.2017 
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
    public sealed class EventHandlerInspector
    {
        private readonly Type _type;

        public EventHandlerInspector(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            _type = type;
        }

        public IEnumerable<EventHandlerActionDescriptor> GetEventHandlerDescriptors()
        {
            var members = _type.GetMethods();
            var descriptors = new List<EventHandlerActionDescriptor>();

            foreach (var member in members)
            {
                if (TryGetHandlingMember(member, out var descriptor))
                {
                    descriptors.Add(descriptor);
                }
            }

            return descriptors;
        }

        private bool TryGetHandlingMember(MethodInfo member, out EventHandlerActionDescriptor result)
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

            if (member.IsDefined<NoEventHandlerActionAttribute>())
            {
                result = default;
                return false;
            }

            var eventType = parameters[0].ParameterType;

            var actionAttribute = member.GetCustomAttribute<EventHandlerActionAttribute>();

            if (actionAttribute != null && actionAttribute.EventType != null)
            {
                if (!eventType.IsAssignableFrom(actionAttribute.EventType))
                {
                    throw new InvalidOperationException();
                }

                eventType = actionAttribute.EventType;
            }

            // Synchronous handler
            if ((member.Name == "Handle" || actionAttribute != null) &&
                (member.ReturnType == typeof(void) || !typeof(Task).IsAssignableFrom(member.ReturnType)))
            {
                result = new EventHandlerActionDescriptor(eventType, member);
                return true;
            }

            // Asynchronous handler
            if ((member.Name == "HandleAsync" || actionAttribute != null) &&
                (typeof(Task).IsAssignableFrom(member.ReturnType)))
            {
                result = new EventHandlerActionDescriptor(eventType, member);
                return true;
            }

            result = default;
            return false;
        }
    }
}