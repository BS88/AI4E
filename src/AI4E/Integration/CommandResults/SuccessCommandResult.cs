/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        SuccessCommandResult.cs 
 * Types:           (1) AI4E.Integration.CommandResults.SuccessCommandResult
 *                  (2) AI4E.Integration.CommandResults.SuccessCommandResult'1
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   15.07.2017 
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
using System.Diagnostics;

namespace AI4E.Integration.CommandResults
{
    /// <summary>
    /// Represents a successful command execution.
    /// </summary>
    public class SuccessCommandResult : ICommandResult
    {
        /// <summary>
        /// Gets the default <see cref="SuccessCommandResult"/>.
        /// </summary>
        public static SuccessCommandResult Default { get; } = new SuccessCommandResult("Success");

        /// <summary>
        /// Creates a new instance of the <see cref="SuccessCommandResult"/> type with the specified message.
        /// </summary>
        /// <param name="message">The command result message.</param>
        public SuccessCommandResult(string message)
        {
            Message = message;
        }

        bool IDispatchResult.IsSuccess => true;

        /// <summary>
        /// Gets a description of the command result.
        /// </summary>
        public string Message { get; }

        #region Equality

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;

            if (ReferenceEquals(obj, this))
                return true;

            if (obj.GetType() != GetType())
                return false;

            return IsEqualByValue(obj);
        }

        protected virtual bool IsEqualByValue(object obj)
        {
            Debug.Assert(obj is SuccessCommandResult);

            return Message == ((SuccessCommandResult)obj).Message;
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode() ^ Message.GetHashCode();
        }

        public static bool operator ==(SuccessCommandResult left, SuccessCommandResult right)
        {
            if (ReferenceEquals(left, null))
                return ReferenceEquals(right, null);

            if (ReferenceEquals(right, null))
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(SuccessCommandResult left, SuccessCommandResult right)
        {
            if (ReferenceEquals(left, null))
                return !ReferenceEquals(right, null);

            if (ReferenceEquals(right, null))
                return true;

            return !left.Equals(right);
        }

        #endregion

        public override string ToString()
        {
            return Message;
        }
    }

    /// <summary>
    /// Represents a successful command execution with result.
    /// </summary>
    public class SuccessCommandResult<TResult> : SuccessCommandResult, ICommandResult<TResult>
    {
        public SuccessCommandResult(TResult result, string message) : base(message)
        {
            Result = result;
        }

        public SuccessCommandResult(TResult result) : this(result, "Success") { }

        public TResult Result { get; }

        protected override bool IsEqualByValue(object obj)
        {
            var other = (SuccessCommandResult<TResult>)obj;

            return Message == other.Message && Result.Equals(other.Result);
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode() ^ Message?.GetHashCode() ?? 0 ^ Result?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return $"{Message} [Result: {Result}]";
        }
    }
}
