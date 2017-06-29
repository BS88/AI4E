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
    ///// <summary>
    ///// Represents the result of a command dispatching operation.
    ///// </summary>
    //public struct CommandResult : IEquatable<CommandResult>
    //{
    //    private CommandResult(CommandState state, string message, ImmutableArray<ValidationResult> validationResults)
    //    {
    //        State = state;
    //        Message = message;
    //        ValidationResults = validationResults;
    //    }

    //    /// <summary>
    //    /// Gets the command state.
    //    /// </summary>
    //    public CommandState State { get; }

    //    /// <summary>
    //    /// Gets a message describing the command result.
    //    /// </summary>
    //    public string Message { get; }

    //    /// <summary>
    //    /// Gets a collection of <see cref="ValidationResult"/>s describing validation failures or 
    //    /// an empty collection if no validation failures are present.
    //    /// </summary>
    //    public ImmutableArray<ValidationResult> ValidationResults { get; }

    //    /// <summary>
    //    /// Defines a command result that represents success.
    //    /// </summary>
    //    public static CommandResult Success { get; }
    //        = new CommandResult(CommandState.Success, nameof(Success), ImmutableArray<ValidationResult>.Empty);

    //    /// <summary>
    //    /// Defines a command result that represents a concurrency issue.
    //    /// </summary>
    //    public static CommandResult ConcurrencyIssue { get; }
    //        = new CommandResult(CommandState.ConcurrencyIssue, nameof(ConcurrencyIssue), ImmutableArray<ValidationResult>.Empty);

    //    /// <summary>
    //    /// Defines an unknown command result.
    //    /// </summary>
    //    public static CommandResult Unknown { get; } = new CommandResult();

    //    /// <summary>
    //    /// Returns a command result that represents a failure with the specified failure message.
    //    /// </summary>
    //    /// <param name="message">A string describing the failure.</param>
    //    /// <returns>A command result that represents the failure.</returns>
    //    public static CommandResult Failure(string message)
    //    {
    //        return new CommandResult(CommandState.Failure, message, ImmutableArray<ValidationResult>.Empty);
    //    }

    //    /// <summary>
    //    /// Returns a command result that represents a validation error with the specified validation results.
    //    /// </summary>
    //    /// <param name="validationResults">A collection of <see cref="ValidationResult"/> describing validation failures.</param>
    //    /// <returns>A command result that represents the validation failures. </returns>
    //    public static CommandResult ValidationError(IEnumerable<ValidationResult> validationResults)
    //    {
    //        return new CommandResult(CommandState.ValidationFailure, "Validation failed", ImmutableArray<ValidationResult>.Empty.AddRange(validationResults));
    //    }

    //    /// <summary>
    //    /// Returns a boolen value indicating whether the specified object equals the current command result.
    //    /// </summary>
    //    /// <param name="obj">The object that shall be compared with the specified command result.</param>
    //    /// <returns>True if <paramref name="obj"/> is a <see cref="CommandResult"/> and equals the current commmand result, false otherwise.</returns>
    //    public override bool Equals(object obj)
    //    {
    //        return obj is CommandResult commandResult && commandResult.Equals(this);
    //    }

    //    /// <summary>
    //    /// Returns a boolean value indicating whether the specified command result equals the current one.
    //    /// </summary>
    //    /// <param name="other">A <see cref="CommandResult"/> that shall be compared with the current one.</param>
    //    /// <returns>True if <paramref name="other"/> equals the current command result, false otherwise.</returns>
    //    public bool Equals(CommandResult other)
    //    {
    //        return State == other.State &&
    //               Message == other.Message &&
    //               ValidationResults.Equals(other.ValidationResults);
    //    }

    //    /// <summary>
    //    /// Returns a hash code for the current command result.
    //    /// </summary>
    //    /// <returns>A hash code for the current command result.</returns>
    //    public override int GetHashCode()
    //    {
    //        return State.GetHashCode() ^ Message.GetHashCode() ^ ValidationResults.GetHashCode();
    //    }

    //    /// <summary>
    //    /// Returns a string representing the current command result.
    //    /// </summary>
    //    /// <returns>A string representing the current command result.</returns>
    //    public override string ToString()
    //    {
    //        return Message;
    //    }

    //    /// <summary>
    //    /// Returns a boolean value indicating whether two <see cref="CommandResult"/>s are equal.
    //    /// </summary>
    //    /// <param name="left">The first command result.</param>
    //    /// <param name="right">The second command result.</param>
    //    /// <returns>True if <paramref name="left"/> equals <paramref name="right"/>, false otherwise.</returns>
    //    public static bool operator ==(CommandResult left, CommandResult right)
    //    {
    //        return left.Equals(right);
    //    }

    //    /// <summary>
    //    /// Returns a boolean value indicating whether two <see cref="CommandResult"/>s are inequal.
    //    /// </summary>
    //    /// <param name="left">The first command result.</param>
    //    /// <param name="right">The second command result.</param>
    //    /// <returns>True if <paramref name="left"/> does not equal <paramref name="right"/>, false otherwise.</returns>
    //    public static bool operator !=(CommandResult left, CommandResult right)
    //    {
    //        return !left.Equals(right);
    //    }
    //}

    ///// <summary>
    ///// Defines command state values.
    ///// </summary>
    //public enum CommandState
    //{
    //    /// <summary>
    //    /// The command state is unknown.
    //    /// </summary>
    //    Unknown = 0,

    //    /// <summary>
    //    /// The command failed.
    //    /// </summary>
    //    Failure,

    //    /// <summary>
    //    /// A validation failure occured when handling the command.
    //    /// </summary>
    //    ValidationFailure,

    //    /// <summary>
    //    /// A concurrency issue occured when handling the command.
    //    /// </summary>
    //    ConcurrencyIssue,

    //    /// <summary>
    //    /// A command was handled successfully.
    //    /// </summary>
    //    Success
    //}

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
}
