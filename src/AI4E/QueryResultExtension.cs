/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        QueryResultExtension.cs 
 * Types:           AI4E.QueryResultExtension
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
using AI4E.QueryResults;

namespace AI4E
{
    public static class QueryResultExtension
    {
        public static bool IsTimeout(this IQueryResult queryResult)
        {
            return queryResult is TimeoutQueryResult;
        }

        public static bool IsTimeout(this IQueryResult queryResult, out DateTime dueTime)
        {
            if (queryResult is TimeoutQueryResult timoutQueryResult)
            {
                dueTime = timoutQueryResult.DueTime;
                return true;
            }

            dueTime = default;
            return false;
        }

        public static bool IsNotFound(this IQueryResult queryResult)
        {
            return queryResult is NotFoundQueryResult;
        }
    }
}
