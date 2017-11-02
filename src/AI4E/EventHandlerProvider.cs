﻿/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        EventHandlerProvider.cs 
 * Types:           AI4E.EventHandlerProvider'1
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
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace AI4E
{
    public sealed class EventHandlerProvider<TEvent> : IContextualProvider<IEventHandler<TEvent>>
    {
        private readonly Type _type;
        private readonly EventHandlerActionDescriptor _actionDescriptor;
        private readonly ImmutableArray<IContextualProvider<IEventProcessor>> _eventProcessors;

        public EventHandlerProvider(Type type, EventHandlerActionDescriptor actionDescriptor, ImmutableArray<IContextualProvider<IEventProcessor>> eventProcessors)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (eventProcessors == null)
                throw new ArgumentNullException(nameof(eventProcessors));

            _type = type;
            _actionDescriptor = actionDescriptor;
            _eventProcessors = eventProcessors;
        }

        public IEventHandler<TEvent> ProvideInstance(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            // Create a new instance of the handler type.
            var handler = ActivatorUtilities.CreateInstance(serviceProvider, _type);

            Debug.Assert(handler != null);

            var contextProperty = _type.GetProperties().FirstOrDefault(p => p.PropertyType == typeof(EventDispatchContext) &&
                                                                            p.CanWrite &&
                                                                            p.IsDefined<EventDispatchContextAttribute>());

            if (contextProperty != null)
            {
                var context = new EventDispatchContext { DispatchServices = serviceProvider };

                contextProperty.SetValue(handler, context);
            }

            return new EventHandlerInvoker<TEvent>(handler, _actionDescriptor, _eventProcessors, serviceProvider);
        }
    }
}