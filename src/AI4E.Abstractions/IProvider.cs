/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        IProvider.cs
 * Types:           (1) AI4E.IProvider'1
 *                  (2) AI4E.IContextualProvider'1
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   02.09.2017 
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

namespace AI4E
{
    /// <summary>
    /// Represents a generator for the specified type of data.
    /// </summary>
    /// <typeparam name="T">The type of data that can be generated.</typeparam>
    public interface IProvider<T>
    {
        /// <summary>
        /// Generates a new instance of the specified type of data.
        /// </summary>
        /// <returns>A new instance with data-type <typeparamref name="T"/>.</returns>
        T Generate();
    }

    public interface IContextualProvider<T> // TODO: This is conceptionally similar to IHandlerProvider (thats intent is more special T is a handler of something)
    {
        T Generate(IServiceProvider serviceProvider);
    }
}
