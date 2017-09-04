/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        EventReplayerFeature.cs 
 * Types:           (1) AI4E.Storage.EventReplayerFeature
 *                  (2) AI4E.Storage.EventReplayerFeatureProvider
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   04.09.2017 
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

namespace AI4E.Storage
{
    public class EventReplayerFeature
    {
        public IList<Type> EventReplayers { get; } = new List<Type>();
    }

    public class EventReplayerFeatureProvider : IApplicationFeatureProvider<EventReplayerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, EventReplayerFeature feature)
        {
            foreach (var part in parts.OfType<IApplicationPartTypeProvider>())
            {
                foreach (var type in part.Types)
                {
                    if (IsEventReplayer(type) && !feature.EventReplayers.Contains(type))
                    {
                        feature.EventReplayers.Add(type);
                    }
                }
            }
        }

        protected virtual bool IsEventReplayer(Type type)
        {
            return (type.IsClass || type.IsValueType && !type.IsEnum) &&
                   !type.IsAbstract &&
                   type.IsPublic &&
                   !type.ContainsGenericParameters &&
                   !type.IsDefined<NoEventReplayerAttribute>() &&
                   (type.Name.EndsWith("EventReplayer", StringComparison.OrdinalIgnoreCase) || type.IsDefined<EventReplayerAttribute>());
        }
    }
}
