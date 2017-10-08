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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using AI4E.Integration.EventResults;
using AI4E.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AI4E.Integration
{
    public sealed class EventHandlerInvoker<TEvent> : IEventHandler<TEvent>
    {
        private readonly object _handler;
        private readonly EventHandlerActionDescriptor _actionDescriptor;
        private readonly IServiceProvider _serviceProvider;

        private readonly IEnumerable<IContextualProvider<IEventProcessor>> _eventProcessors;

        public EventHandlerInvoker(object handler, EventHandlerActionDescriptor actionDescriptor, IServiceProvider serviceProvider, IOptions<MessagingOptions> optionsProvider)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            if (optionsProvider == null)
                throw new ArgumentNullException(nameof(optionsProvider));

            _handler = handler;
            _actionDescriptor = actionDescriptor;
            _serviceProvider = serviceProvider;

            var options = optionsProvider.Value;

            _eventProcessors = new List<IContextualProvider<IEventProcessor>>(options.EventProcessors);
        }

        //private bool IsProcessManager()
        //{
        //    var type = _handler.GetType();

        //    return (type.IsClass || type.IsValueType && !type.IsEnum) &&
        //           !type.IsAbstract &&
        //           type.IsPublic &&
        //           !type.ContainsGenericParameters &&
        //           !type.IsDefined<NoProcessManagerAttribute>() &&
        //           (type.Name.EndsWith("ProcessManager", StringComparison.OrdinalIgnoreCase) || type.IsDefined<ProcessManagerAttribute>());
        //}

        private async Task<IEventResult> InternalHandleAsync(TEvent evt)
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

        public async Task<IEventResult> HandleAsync(TEvent evt)
        {
            //var isProcessManager = IsProcessManager();

            //if (!isProcessManager)
            //{
            //    return await InternalHandleAsync(evt);
            //}

            //var type = _handler.GetType();
            //var dataStore = _serviceProvider.GetRequiredService<IDataStore>();
            //object state = null;
            //var created = false;

            //var processManagerStateProperty = type.GetProperties().SingleOrDefault(p => p.CanWrite && p.IsDefined<ProcessManagerStateAttribute>());

            //if (processManagerStateProperty != null)
            //{
            //    var stateType = processManagerStateProperty.PropertyType;
            //    {
            //        var customType = processManagerStateProperty.GetCustomAttribute<ProcessManagerStateAttribute>().StateType;

            //        if (customType != null)
            //        {
            //            if (!stateType.IsAssignableFrom(customType))
            //            {
            //                throw new InvalidOperationException(); // TODO
            //            }
            //            stateType = customType;
            //        }
            //    }

            //    var attachments = Activator.CreateInstance(typeof(ProcessManagerAttachment<>).MakeGenericType(stateType), dataStore);

            //    Debug.Assert(attachments != null);

            //    ((dynamic)_handler).AttachProcessManager(attachments);

            //    state = (object)(await ((dynamic)attachments).GetStateAsync(evt));

            //    // TODO: Not all events are allowed to start a process.
            //    if (state == null)
            //    {
            //        if (!(bool)(((dynamic)_handler).CanInitiateProzess(evt)))
            //        {
            //            return new FailureEventResult(""); // TODO: Maybe the events are out of order? What to do about that?
            //        }

            //        // Create state
            //        state = (object)(((dynamic)_handler).CreateInitialState(evt, stateType));
            //        created = true;
            //    }

            //    processManagerStateProperty.SetValue(_handler, state);
            //}

            var eventProcessorStack = new Stack<IEventProcessor>();

            foreach (var eventProcessorProvider in _eventProcessors)
            {
                var eventProcessor = eventProcessorProvider.ProvideInstance(_serviceProvider);

                Debug.Assert(eventProcessor != null);

                var props = eventProcessor.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var prop = props.FirstOrDefault(p => p.IsDefined<EventProcessorContextAttribute>() &&
                                                     p.PropertyType == typeof(object) || p.PropertyType == typeof(IEventProcessorContext) &&
                                                     p.GetIndexParameters().Length == 0 && 
                                                     p.CanWrite);

                if (prop != null)
                {
                    IEventProcessorContext eventProcessorContext = new EventProcessorContext(_handler, typeof(TEvent));

                    prop.SetValue(eventProcessor, eventProcessorContext);
                }

                await eventProcessor.PreProcessAsync(evt);

                eventProcessorStack.Push(eventProcessor);
            }

            var result = await InternalHandleAsync(evt);

            foreach (var eventProcessor in eventProcessorStack)
            {
                await eventProcessor.PostProcessAsync(result);
            }

            //var terminated = (bool)((dynamic)_handler).IsTerminated;

            //if (created && !terminated)
            //{
            //    dataStore.Add((dynamic)state);
            //}
            //else if (!created && terminated)
            //{
            //    dataStore.Remove((dynamic)state);
            //}
            //else if (!created && !terminated)
            //{
            //    dataStore.Update((dynamic)state);
            //}

            //await dataStore.SaveChangesAsync();
            return result;
        }

        private sealed class EventProcessorContext : IEventProcessorContext
        {
            public EventProcessorContext(object eventHandler, Type eventType)
            {
                Debug.Assert(eventHandler != null);
                Debug.Assert(eventType != null);

                EventHandler = eventHandler;
                EventType = eventType;
            }

            public object EventHandler { get; }

            public Type EventType { get; }
        }
    }
}
