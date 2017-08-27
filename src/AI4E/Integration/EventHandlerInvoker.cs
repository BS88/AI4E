/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        EventHandlerInvoker.cs 
 * Types:           AI4E.Integration.EventHandlerInvoker'1
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
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using AI4E.Integration.EventResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace AI4E.Integration
{
    public sealed class EventHandlerInvoker<TEvent> : IEventHandler<TEvent>
    {
        private readonly object _handler;
        private readonly EventHandlerActionDescriptor _actionDescriptor;
        private readonly IServiceProvider _serviceProvider;

        public EventHandlerInvoker(object handler, EventHandlerActionDescriptor actionDescriptor, IServiceProvider serviceProvider)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            _handler = handler;
            _actionDescriptor = actionDescriptor;
            _serviceProvider = serviceProvider;
        }

        public async Task<IEventResult> HandleAsync(TEvent evt)
        {
            var member = _actionDescriptor.Member;

            Debug.Assert(member != null);

            var parameters = member.GetParameters();

            var callingArgs = new object[parameters.Length];

            callingArgs[0] = evt;

            for (var i = 1; i < callingArgs.Length; i++)
            {
                var parameterType = parameters[i].ParameterType;

                object arg;

                if (parameterType.IsDefined<FromServicesAttribute>())
                {
                    arg = _serviceProvider.GetRequiredService(parameterType);
                }
                else
                {
                    arg = _serviceProvider.GetService(parameterType);

                    if (arg == null && parameterType.IsValueType)
                    {
                        arg = FormatterServices.GetUninitializedObject(parameterType);
                    }
                }

                callingArgs[i] = arg;
            }

            object result;

            try
            {
                result = member.Invoke(_handler, callingArgs);
            }
            catch (Exception exc)
            {
                return new FailureEventResult(exc.Message);
            }

            if (member.ReturnType == typeof(void))
            {
                return SuccessEventResult.Default;
            }

            if (typeof(Task).IsAssignableFrom(member.ReturnType))
            {
                try
                {
                    await (Task)result;
                }
                catch (Exception exc)
                {
                    return new FailureEventResult(exc.Message);
                }

                if (member.ReturnType == typeof(Task))
                {
                    return SuccessEventResult.Default;
                }

                // This only happens if the BCL changed.
                if (!member.ReturnType.IsGenericType)
                {
                    return SuccessEventResult.Default;
                }

                result = (object)((dynamic)result).Result;
            }

            if (result is IEventResult eventResult)
                return eventResult;

            //if (result == null)
            return SuccessEventResult.Default;

            // TODO: Currently event-handlers are not allowed to return values.
            //return (ICommandResult)Activator.CreateInstance(typeof(SuccessCommandResult<>).MakeGenericType(result.GetType()), result); 
        }
    }
}
