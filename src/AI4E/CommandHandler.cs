/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        CommandHandler.cs 
 * Types:           (1) AI4E.CommandHandler
 *                  (2) AI4E.CommandHandlerAttribute
 *                  (3) AI4E.NoCommandHandlerAttribute
 *                  (4) AI4E.CommandHandlerActionAttribute
 *                  (5) AI4E.NoCommandHandlerActionAttribute
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
using AI4E.CommandResults;

namespace AI4E
{
    [CommandHandler]
    public abstract class CommandHandler
    {
        [CommandDispatchContext]
        public CommandDispatchContext CommandDispatchContext { get; internal set; }

        [NoCommandHandlerAction]
        public virtual ConcurrencyIssueCommandResult ConcurrencyIssue()
        {
            return new ConcurrencyIssueCommandResult();
        }

        [NoCommandHandlerAction]
        public virtual EntityNotFoundCommandResult EntityNotFound(Type entityType, Guid id)
        {
            return new EntityNotFoundCommandResult(entityType, id);
        }

        [NoCommandHandlerAction]
        public virtual EntityNotFoundCommandResult EntityNotFound<TEntity>(Guid id)
        {
            return new EntityNotFoundCommandResult(typeof(TEntity), id);
        }

        [NoCommandHandlerAction]
        public virtual FailureCommandResult Failure()
        {
            return FailureCommandResult.UnknownFailure;
        }

        [NoCommandHandlerAction]
        public virtual FailureCommandResult Failure(string message)
        {
            return new FailureCommandResult(message);
        }

        [NoCommandHandlerAction]
        public virtual SuccessCommandResult Success()
        {
            return SuccessCommandResult.Default;
        }

        [NoCommandHandlerAction]
        public virtual SuccessCommandResult Success(string message)
        {
            return new SuccessCommandResult(message);
        }

        [NoCommandHandlerAction]
        public virtual SuccessCommandResult<TResult> Success<TResult>(TResult result)
        {
            return new SuccessCommandResult<TResult>(result);
        }

        [NoCommandHandlerAction]
        public virtual SuccessCommandResult<TResult> Success<TResult>(TResult result, string message)
        {
            return new SuccessCommandResult<TResult>(result, message);
        }

        [NoCommandHandlerAction]
        public virtual UnauthenticatedCommandResult Unauthenticated()
        {
            return new UnauthenticatedCommandResult();
        }

        [NoCommandHandlerAction]
        public virtual UnauthorizedCommandResult Unauthorized()
        {
            return new UnauthorizedCommandResult();
        }

        [NoCommandHandlerAction]
        public virtual ValidationFailureCommandResult ValidationFailure()
        {
            return new ValidationFailureCommandResult();
        }

        [NoCommandHandlerAction]
        public virtual ValidationFailureCommandResult ValidationFailure(IEnumerable<ValidationResult> validationResults)
        {
            return new ValidationFailureCommandResult(validationResults);
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public sealed class CommandHandlerAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public sealed class NoCommandHandlerAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class CommandHandlerActionAttribute : Attribute
    {
        public Type CommandType { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class NoCommandHandlerActionAttribute : Attribute { }
}
