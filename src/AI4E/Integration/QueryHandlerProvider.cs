﻿/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        QueryHandlerProvider.cs 
 * Types:           AI4E.Integration.QueryHandlerProvider'1
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
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace AI4E.Integration
{
    public sealed class QueryHandlerProvider<TQuery> : IContextualProvider<IQueryHandler<TQuery>>
    {
        private readonly Type _type;
        private readonly QueryHandlerActionDescriptor _actionDescriptor;

        public QueryHandlerProvider(Type type, QueryHandlerActionDescriptor actionDescriptor)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            _type = type;
            _actionDescriptor = actionDescriptor;
        }

        public IQueryHandler<TQuery> ProvideInstance(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            // Create a new instance of the handler type.
            var handler = ActivatorUtilities.CreateInstance(serviceProvider, _type);

            Debug.Assert(handler != null);

            var contextProperty = _type.GetProperties().FirstOrDefault(p => p.PropertyType == typeof(QueryDispatchContext) &&
                                                                            p.CanWrite &&
                                                                            p.IsDefined<QueryDispatchContextAttribute>());

            if (contextProperty != null)
            {
                var context = new QueryDispatchContext { DispatchServices = serviceProvider };

                contextProperty.SetValue(handler, context);
            }

            return new QueryHandlerInvoker<TQuery>(handler, _actionDescriptor, serviceProvider);
        }
    }
}
