﻿/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        FailureCommandResult.cs 
 * Types:           AI4E.Integration.CommandResults.FailureCommandResult
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
    /// Represents a failed command execution.
    /// </summary>
    public class FailureCommandResult : ICommandResult
    {
        /// <summary>
        /// Gets a <see cref="FailureCommandResult"/> that represents unkown failures.
        /// </summary>
        public static FailureCommandResult UnknownFailure = new FailureCommandResult("Unknown failure.");

        /// <summary>
        /// Creates a new instance of the <see cref="FailureCommandResult"/> type with the specified message.
        /// </summary>
        /// <param name="message">The failure message.</param>
        public FailureCommandResult(string message)
        {
            Message = message;
        }

        bool IDispatchResult.IsSuccess => false;

        /// <summary>
        /// Gets a failure message of the command result.
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
            Debug.Assert(obj is FailureCommandResult);

            return Message == ((FailureCommandResult)obj).Message;
        }

        bool IEquatable<IDispatchResult>.Equals(IDispatchResult other)
        {
            return Equals(other);
        }

        bool IEquatable<ICommandResult>.Equals(ICommandResult other)
        {
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode() ^ Message?.GetHashCode()??0;
        }

        public static bool operator ==(FailureCommandResult left, FailureCommandResult right)
        {
            if (ReferenceEquals(left, null))
                return ReferenceEquals(right, null);

            if (ReferenceEquals(right, null))
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(FailureCommandResult left, FailureCommandResult right)
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
}
