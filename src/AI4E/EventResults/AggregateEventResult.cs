/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        EventAggregateResult.cs 
 * Types:           AI4E.Integration.EventResults.EventAggregateResult
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
using System.Linq;

namespace AI4E.Integration.EventResults
{
    public sealed class AggregateEventResult : IAggregateEventResult
    {
        public AggregateEventResult(IEnumerable<IEventResult> eventResults)
        {
            EventResults = eventResults.ToImmutableArray();
        }

        public ImmutableArray<IEventResult> EventResults { get; }

        public bool IsSuccess => EventResults.Length == 0 || EventResults.All(p => p.IsSuccess);

        string IDispatchResult.Message => IsSuccess ? "Success" : "Failure";

        string IEventResult.Message => ((IDispatchResult)this).Message;

        public override string ToString()
        {
            return ((IDispatchResult)this).Message;
        }
    }
}
