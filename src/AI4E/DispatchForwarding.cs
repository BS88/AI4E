/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        AsyncSingleHandlerRegistry.cs 
 * Types:           (1) AI4E.AsyncSingleHandlerRegistry'1
 *                  (2) AI4E.AsyncSingleHandlerRegistry'1.NoDispatchForwarding
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   16.06.2017 
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

using System.Threading.Tasks;

namespace AI4E
{
    public sealed class DispatchForwarding : IDispatchForwarding
    {
        public static IDispatchForwarding None { get; } = new DispatchForwarding();

        private DispatchForwarding() { }

        public Task RegisterForwardingAsync()
        {
            //if (IsForwardingActive)
            //    return;

            return Task.CompletedTask;
        }

        public Task UnregisterForwardingAsync()
        {
            return Task.CompletedTask;
        }
    }
}
