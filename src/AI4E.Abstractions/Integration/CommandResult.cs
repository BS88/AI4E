/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        CommandResult.cs 
 * Types:           (1) AI4E.Integration.CommandResult
 *                  (2) AI4E.Integration.CommandState
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   29.04.2017 
 * Status:          Ready
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
using System.Collections.Immutable;

namespace AI4E.Integration
{
    public interface ICommandResult
    {
        bool IsSuccess { get; }

        string Message { get; }
    }

    public class SuccessCommandResult : ICommandResult
    {
        public SuccessCommandResult(string message)
        {
            Message = message;
        }

        public SuccessCommandResult() : this("Success") { }

        public bool IsSuccess => true;

        public string Message { get; }
    }

    public class FailureCommandResult : ICommandResult
    {
        public FailureCommandResult(string message)
        {
            Message = message;
        }

        public FailureCommandResult() : this("Unknown failure") { }

        public bool IsSuccess => false;

        public string Message { get; }
    }

    public class ValidationFailureCommandResult : FailureCommandResult
    {
        public ValidationFailureCommandResult() : base("Unkown validation failure") { }

        public ValidationFailureCommandResult(IEnumerable<ValidationResult> validationResults) : this()
        {
            ValidationResults = validationResults.ToImmutableArray();
        }

        public ImmutableArray<ValidationResult> ValidationResults { get; }
    }

    public class UnauthorizedCommandResult : FailureCommandResult
    {
        public UnauthorizedCommandResult() : base("Action forbidden") { }
    }

    public class UnauthenticatedCommandResult : FailureCommandResult
    {
        public UnauthenticatedCommandResult() : base("Authentication required") { }
    }

    public class ConcurrencyIssueCommandResult : FailureCommandResult
    {
        public ConcurrencyIssueCommandResult() : base("A concurrency issue occured.") { }
    }

    public class EntityNotFoundCommandResult : FailureCommandResult
    {
        public EntityNotFoundCommandResult(Type entityType, Guid id) 
            : base($"The entity '{(entityType ?? throw new ArgumentNullException(nameof(entityType))).FullName}' with the id '{id}' was not found.")
        {
            EntityType = entityType;
            Id = id;
        }

        public Type EntityType { get; }

        public Guid Id { get; }
    }

    public class CommandDispatchFailureCommandResult : FailureCommandResult
    {
        public CommandDispatchFailureCommandResult(Type commandType)
            : base($"The command '{(commandType ?? throw new ArgumentNullException(nameof(commandType))).FullName }'cannot be dispatched.")
        {
            CommandType = commandType;
        }

        public Type CommandType { get; }
    }
}
