﻿/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        ValidationFailureCommandResult.cs 
 * Types:           AI4E.Integration.CommandResults.ValidationFailureCommandResult
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

using System.Collections.Generic;
using System.Collections.Immutable;

namespace AI4E.Integration.CommandResults
{
    public class ValidationFailureCommandResult : FailureCommandResult
    {
        public ValidationFailureCommandResult() : base("Unkown validation failure") { }

        public ValidationFailureCommandResult(IEnumerable<ValidationResult> validationResults) : this()
        {
            ValidationResults = validationResults.ToImmutableArray();
        }

        public ImmutableArray<ValidationResult> ValidationResults { get; }
    }
}
