/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        EventHandler.cs 
 * Types:           (1) AI4E.Integration.EventHandler
 *                  (2) AI4E.Integration.EventHandlerAttribute
 *                  (3) AI4E.Integration.NoEventHandlerAttribute
 *                  (4) AI4E.Integration.EventHandlerActionAttribute
 *                  (5) AI4E.Integration.NoEventHandlerActionAttribute
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
using AI4E.Integration.EventResults;

namespace AI4E.Integration
{
    [EventHandler]
    public abstract class EventHandler
    {
        [EventDispatchContext]
        public EventDispatchContext EventDispatchContext { get; internal set; }

        [NoCommandHandlerAction]
        public virtual FailureEventResult Failure()
        {
            return FailureEventResult.UnknownFailure;
        }

        [NoCommandHandlerAction]
        public virtual FailureEventResult Failure(string message)
        {
            return new FailureEventResult(message);
        }

        [NoCommandHandlerAction]
        public virtual SuccessEventResult Success()
        {
            return SuccessEventResult.Default;
        }

        [NoCommandHandlerAction]
        public virtual SuccessEventResult Success(string message)
        {
            return new SuccessEventResult(message);
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public class EventHandlerAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public sealed class NoEventHandlerAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class EventHandlerActionAttribute : Attribute
    {
        public Type EventType { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class NoEventHandlerActionAttribute : Attribute { }
}
