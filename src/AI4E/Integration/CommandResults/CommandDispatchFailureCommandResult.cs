/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        CommandDispatchFailureCommandResult.cs 
 * Types:           AI4E.Integration.CommandResults.CommandDispatchFailureCommandResult
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
    public class CommandDispatchFailureCommandResult : FailureCommandResult
    {
        public CommandDispatchFailureCommandResult(Type commandType)
            : base($"The command '{(commandType ?? throw new ArgumentNullException(nameof(commandType))).FullName }'cannot be dispatched.")
        {
            CommandType = commandType;
        }

        public Type CommandType { get; }

        protected override bool IsEqualByValue(object obj)
        {
            Debug.Assert(obj is CommandDispatchFailureCommandResult);

            return CommandType == ((CommandDispatchFailureCommandResult)obj).CommandType;
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode() ^ CommandType?.GetHashCode() ?? 0;
        }
    }
}
