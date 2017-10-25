/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        QueryHandlerFeature.cs 
 * Types:           (1) AI4E.Integration.QueryHandlerFeature
 *                  (2) AI4E.Integration.QueryHandlerFeatureProvider
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
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace AI4E.Integration
{
    public class QueryHandlerFeature
    {
        public IList<Type> QueryHandlers { get; } = new List<Type>();
    }

    public class QueryHandlerFeatureProvider : IApplicationFeatureProvider<QueryHandlerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, QueryHandlerFeature feature)
        {
            foreach (var part in parts.OfType<IApplicationPartTypeProvider>())
            {
                foreach (var type in part.Types)
                {
                    if (IsQueryHandler(type) && !feature.QueryHandlers.Contains(type))
                    {
                        feature.QueryHandlers.Add(type);
                    }
                }
            }
        }

        protected virtual bool IsQueryHandler(Type type)
        {
            return (type.IsClass || type.IsValueType && !type.IsEnum) &&
                   !type.IsAbstract &&
                   type.IsPublic &&
                   !type.ContainsGenericParameters &&
                   !type.IsDefined<NoQueryHandlerAttribute>() &&
                   (type.Name.EndsWith("QueryHandler", StringComparison.OrdinalIgnoreCase) || type.IsDefined<QueryHandlerAttribute>());
        }
    }
}
