/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        IGenerator.cs
 * Types:           AI4E.IGenerator'1
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   16.05.2017 
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
    public interface IGenerator<T>
    {
        /// <summary>
        /// Generates a new instance of the specified type of data.
        /// </summary>
        /// <returns>A new instance with data-type <typeparamref name="T"/>.</returns>
        T Generate();
    }

    public interface ISandboxedGenerator<T>
    {
        T Generate(IServiceProvider serviceProvider);
    }
}
