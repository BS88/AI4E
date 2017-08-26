/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        CommandHandlerFeature.cs 
 * Types:           (1) AI4E.Integration.CommandHandlerFeature
 *                  (2) AI4E.Integration.CommandHandlerFeatureProvider
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
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace AI4E.Integration
{
    public class CommandHandlerFeature
    {
        public IList<Type> CommandHandlers { get; } = new List<Type>();
    }

    public class CommandHandlerFeatureProvider : IApplicationFeatureProvider<CommandHandlerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, CommandHandlerFeature feature)
        {
            foreach (var part in parts.OfType<IApplicationPartTypeProvider>())
            {
                foreach (var type in part.Types)
                {
                    if (IsCommandHandler(type) && !feature.CommandHandlers.Contains(type))
                    {
                        feature.CommandHandlers.Add(type);
                    }
                }
            }
        }

        protected virtual bool IsCommandHandler(Type type)
        {
            return (type.IsClass || type.IsValueType && !type.IsEnum) &&
                   !type.IsAbstract &&
                   type.IsPublic &&
                   !type.ContainsGenericParameters &&
                   !type.IsDefined<NoCommandHandlerAttribute>() &&
                   (type.Name.EndsWith("CommandHandler", StringComparison.OrdinalIgnoreCase) ||
                    type.IsDefined<CommandHandlerAttribute>());
        }
    }
}
