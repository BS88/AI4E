/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        QueryHandler.cs 
 * Types:           (1) AI4E.Integration.QueryHandler
 *                  (2) AI4E.Integration.QueryHandlerAttribute
 *                  (3) AI4E.Integration.NoQueryHandlerAttribute
 *                  (4) AI4E.Integration.QueryHandlerActionAttribute
 *                  (5) AI4E.Integration.NoQueryHandlerActionAttribute
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
using AI4E.Integration.QueryResults;

namespace AI4E.Integration
{
    [QueryHandler]
    public abstract class QueryHandler
    {
        [QueryDispatchContext]
        public QueryDispatchContext QueryDispatchContext { get; }

        [NoQueryHandlerAction]
        public virtual FailureQueryResult Failure()
        {
            return FailureQueryResult.UnknownFailure;
        }

        [NoQueryHandlerAction]
        public virtual FailureQueryResult Failure(string message)
        {
            return new FailureQueryResult(message);
        }

        [NoQueryHandlerAction]
        public virtual SuccessQueryResult Success()
        {
            return SuccessQueryResult.Default;
        }

        [NoQueryHandlerAction]
        public virtual SuccessQueryResult Success(string message)
        {
            return new SuccessQueryResult(message);
        }

        [NoQueryHandlerAction]
        public virtual SuccessQueryResult<TResult> Success<TResult>(TResult result)
        {
            return new SuccessQueryResult<TResult>(result);
        }

        [NoQueryHandlerAction]
        public virtual SuccessQueryResult<TResult> Success<TResult>(TResult result, string message)
        {
            return new SuccessQueryResult<TResult>(result, message);
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public sealed class QueryHandlerAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public sealed class NoQueryHandlerAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class QueryHandlerActionAttribute : Attribute
    {
        public Type QueryType { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class NoQueryHandlerActionAttribute : Attribute { }
}
