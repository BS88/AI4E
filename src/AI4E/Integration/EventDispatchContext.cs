﻿/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        EventDispatchContext.cs 
 * Types:           (1) AI4E.Integration.EventDispatchContext
 *                  (2) AI4E.Integration.EventDispatchContextAttribute
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

namespace AI4E.Integration
{
    public class EventDispatchContext
    {
        public EventDispatchContext() { }

        public IServiceProvider DispatchServices { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EventDispatchContextAttribute : Attribute { }
}
