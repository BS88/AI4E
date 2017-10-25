/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        CommandResultExtension.cs 
 * Types:           AI4E.CommandResultExtension
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   26.08.2017 
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
using System.Linq;
using AI4E.CommandResults;

namespace AI4E
{
    /// <summary>
    /// Defines extensions for the <see cref="CommandResult"/> type.
    /// </summary>
    public static class CommandResultExtension
    {
        public static bool IsUnauthorized(this ICommandResult commandResult)
        {
            return commandResult is UnauthenticatedCommandResult;
        }

        public static bool IsUnauthenticated(this ICommandResult commandResult)
        {
            return commandResult is UnauthenticatedCommandResult;
        }

        public static bool IsValidationFailed(this ICommandResult commandResult)
        {
            return commandResult is ValidationFailureCommandResult;
        }

        public static bool IsValidationFailed(this ICommandResult commandResult, out IEnumerable<ValidationResult> validationResults)
        {
            if (commandResult is ValidationFailureCommandResult validationFailureCommandResult)
            {
                validationResults = validationFailureCommandResult.ValidationResults;
                return true;
            }

            validationResults = Enumerable.Empty<ValidationResult>();
            return false;
        }

        public static bool IsConcurrencyIssue(this ICommandResult commandResult)
        {
            return commandResult is ConcurrencyIssueCommandResult;
        }

        public static bool IsEntityNotFound(this ICommandResult commandResult)
        {
            return commandResult is EntityNotFoundCommandResult;
        }

        public static bool IsEntityNotFound(this ICommandResult commandResult, out Type entityType, out Guid id)
        {
            if (commandResult is EntityNotFoundCommandResult entityNotFoundCommandResult)
            {
                entityType = entityNotFoundCommandResult.EntityType;
                id = entityNotFoundCommandResult.Id;
                return true;
            }

            entityType = default;
            id = default;

            return false;
        }

        public static bool IsTimeout(this ICommandResult commandResult)
        {
            return commandResult is TimeoutCommandResult;
        }

        public static bool IsTimeout(this ICommandResult commandResult, out DateTime dueTime)
        {
            if (commandResult is TimeoutCommandResult timeoutCommandResult)
            {
                dueTime = timeoutCommandResult.DueTime;
                return true;
            }

            dueTime = default;
            return false;
        }
    }
}
