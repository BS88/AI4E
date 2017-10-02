/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        IModuleHost.cs
 * Types:           (1) AI4E.Modularity.IModuleHost
 *                  (2) AI4E.Modularity.IModuleConnection
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   01.10.2017 
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
using System.Net;
using System.Threading.Tasks;
using AI4E.Async;

namespace AI4E.Modularity
{
    /// <summary>
    /// Represents a module host.
    /// </summary>
    public interface IModuleHost : IAsyncCompletion
    {
        /// <summary>
        /// Gets a collection of connections to modules.
        /// </summary>
        IEnumerable<IModuleConnection> Connections { get; }

        /// <summary>
        /// Gets the local end point.
        /// </summary>
        IPEndPoint LocalEndPoint { get; }

        /// <summary>
        /// Asynchronously registers a module installation.
        /// </summary>
        /// <param name="installation">The module installation.</param>
        /// <returns>A task representing the asynchonous operation.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if the module descrived by <paramref name="installation"/> is currently connected.</exception>
        Task RegisterInstallationAsync(IModuleInstallation installation);

        /// <summary>
        /// Asynchronously unregisters a module installation.
        /// </summary>
        /// <param name="installation">The module installation.</param>
        /// <returns>A task representing the asynchonous operation.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if the module descrived by <paramref name="installation"/> is currently connected.</exception>
        Task UnregisterInstallationAsync(IModuleInstallation installation);

        /// <summary>
        /// Asynchronously gets the connection to the module specified by its identifier.
        /// Completes only if and when the connection is established.
        /// </summary>
        /// <param name="module">The module identifier.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task<IModuleConnection> GetConnectionAsync(ModuleIdentifier module);

        /// <summary>
        /// Tries to get the connection to the module specified by its identifier.
        /// </summary>
        /// <param name="module">The module identifier.</param>
        /// <param name="connection">Contains the connection if the operation succeeds.</param>
        /// <returns>True if the operation suceeded, false otherwise.</returns>
        bool TryGetConnection(ModuleIdentifier module, out IModuleConnection connection);
    }

    /// <summary>
    /// Represents a connection to a module.
    /// </summary>
    public interface IModuleConnection
    {
        /// <summary>
        /// Gets a boolean value indicating whether the connection is openend.
        /// </summary>
        bool IsOpened { get; }

        /// <summary>
        /// Gets the underlying message end point.
        /// </summary>
        IMessageEndPoint UnderlyingEndPoint { get; }

        /// <summary>
        /// Asynchronously opens the connection.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OpenAsync();

        /// <summary>
        /// Asynchronously closes the connection.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CloseAsync();

        /// <summary>
        /// Gets a boolean value indicating whether the conneciton is a debug session.
        /// </summary>
        bool IsDebugSession { get; }

        /// <summary>
        /// Gets the unique identifier of the connected module.
        /// </summary>
        ModuleIdentifier ConnectedModule { get; }

        /// <summary>
        /// Get the version of the connection module.
        /// </summary>
        ModuleVersion ConnectedModuleVersion { get; }
    }
}