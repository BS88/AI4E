/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        ModuleHost.cs
 * Types:           (1) AI4E.Modularity.ModuleHost
 *                  (2) AI4E.Modularity.ModuleHost.Entry
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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using AI4E.Async;
using AI4E.Async.Processing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nito.AsyncEx;

namespace AI4E.Modularity
{
    /// <summary>
    /// Represents a module host that is used to listen for and accept connections from modules.
    /// </summary>
    public sealed partial class ModuleHost : IModuleHost, IAsyncCompletion
    {
        #region Fields

        private readonly TcpListener _tcpHost;
        private readonly bool _allowDebugSessions;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ModuleHost> _logger;
        private readonly AsyncProcess _receiveProcess;
        private readonly ConcurrentDictionary<ModuleIdentifier, Entry> _entries = new ConcurrentDictionary<ModuleIdentifier, Entry>();
        private readonly TaskCompletionSource<object> _completionSource = new TaskCompletionSource<object>();
        private Task _completion;

        #endregion

        #region C'tor

        /// <summary>
        /// Creates a new instance of the <see cref="ModuleHost"/> type.
        /// </summary>
        /// <param name="options">The modularity options.</param>
        /// <param name="serviceProvider">The service provider used to resolve services.</param>
        /// <exception cref="ArgumentNullException">Thrown if either <paramref name="options"/> or <paramref name="serviceProvider"/> is null.</exception>
        public ModuleHost(IOptions<ModularityOptions> options, IServiceProvider serviceProvider)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            var opt = options.Value;

            if (opt.EnableDebugging)
            {
                LocalEndPoint = new IPEndPoint(IPAddress.Any, opt.DebugPort);
                _allowDebugSessions = true;
            }
            else
            {
                LocalEndPoint = new IPEndPoint(IPAddress.Loopback, 0);
            }

            _serviceProvider = serviceProvider;

            _receiveProcess = new AsyncProcess(ConnectionProcedure);
            _tcpHost = new TcpListener(LocalEndPoint);
            _tcpHost.Start();
            _receiveProcess.StartExecution();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ModuleHost"/> type.
        /// </summary>
        /// <param name="options">The modularity options.</param>
        /// <param name="serviceProvider">The service provider used to resolve services.</param>
        /// <param name="logger">A <see cref="ILogger"/> used for logging.</param>
        /// <exception cref="ArgumentNullException">Thrown if either <paramref name="options"/> or <paramref name="serviceProvider"/> is null.</exception>
        public ModuleHost(IOptions<ModularityOptions> options, IServiceProvider serviceProvider, ILogger<ModuleHost> logger)
            : this(options, serviceProvider)
        {
            _logger = logger;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the local end point.
        /// </summary>
        public IPEndPoint LocalEndPoint { get; }

        /// <summary>
        /// Gets a collection of connections.
        /// </summary>
        public IEnumerable<IModuleConnection> Connections => new List<Entry>(_entries.Values).Select(p => p.Connection);

        /// <summary>
        /// Gets a task that represents the asynchronous completion of the instance.
        /// </summary>
        public Task Completion => _completionSource.Task;

        private bool IsDisposed => _completion != null;

        #endregion

        /// <summary>
        /// Asynchronously registers a module installation.
        /// </summary>
        /// <param name="installation">The module installation.</param>
        /// <returns>A task representing the asynchonous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the module descrived by <paramref name="installation"/> is currently connected.</exception>
        public async Task RegisterInstallationAsync(IModuleInstallation installation)
        {
            if (installation == null)
                throw new ArgumentNullException(nameof(installation));

            var entry = _entries.GetOrAdd(installation.Module, id => new Entry { });

            using (await entry.Lock.LockAsync())
            {
                if (entry.Connection != null)
                {
                    throw new InvalidOperationException("A installation cannot be registered for a module connected currently.");
                }

                entry.Installation = installation;
            }
        }

        /// <summary>
        /// Asynchronously unregisters a module installation.
        /// </summary>
        /// <param name="installation">The module installation.</param>
        /// <returns>A task representing the asynchonous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the module descrived by <paramref name="installation"/> is currently connected.</exception>
        public async Task UnregisterInstallationAsync(IModuleInstallation installation)
        {
            if (installation == null)
                throw new ArgumentNullException(nameof(installation));

            var entry = _entries.GetOrAdd(installation.Module, id => new Entry { });

            using (await entry.Lock.LockAsync())
            {
                if (entry.Connection != null)
                {
                    throw new InvalidOperationException("A installation cannot be unregistered for a module connected currently.");
                }

                entry.Installation = null;
            }
        }

        /// <summary>
        /// Asynchronously gets the connection to the module specified by its identifier.
        /// Completes only if and when the connection is established.
        /// </summary>
        /// <param name="module">The module identifier.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task<IModuleConnection> GetConnectionAsync(ModuleIdentifier module)
        {
            var entry = _entries.GetOrAdd(module, id => new Entry { });

            var tcs = default(TaskCompletionSource<IModuleConnection>);

            using (await entry.Lock.LockAsync())
            {
                if (entry.Tcs == null)
                    entry.Tcs = new TaskCompletionSource<IModuleConnection>();

                tcs = entry.Tcs;
            }

            return await tcs.Task;
        }

        /// <summary>
        /// Tries to get the connection to the module specified by its identifier.
        /// </summary>
        /// <param name="module">The module identifier.</param>
        /// <param name="connection">Contains the connection if the operation succeeds.</param>
        /// <returns>True if the operation suceeded, false otherwise.</returns>
        public bool TryGetConnection(ModuleIdentifier module, out IModuleConnection connection)
        {
            if (_entries.TryGetValue(module, out var entry))
            {
                connection = entry.Connection;
                return connection != null;
            }

            connection = default;
            return false;
        }

        /// <summary>
        /// Starts completing the instance asynchronously.
        /// </summary>
        /// <remarks>
        /// This is conceptually similar to <see cref="System.IDisposable.Dispose"/>.
        /// After calling this method, invoking any member except <see cref="Completion"/> is forbidden.
        /// </remarks>
        public void Complete()
        {
            if (_completion != null)
                return;

            _completion = CompleteAsync();
        }

        private async Task ConnectionProcedure(CancellationToken cancellation)
        {
            _logger?.LogInformation($"Started module connection procedure.");

            while (cancellation.ThrowOrContinue())
            {
                try
                {
                    var client = await _tcpHost.AcceptTcpClientAsync();

                    _logger?.LogInformation($"Module connected. Proceeding request.");

                    // Any failure in module connection must not disturb the host from handling other modules.
                    OnConnectedAsync(client).HandleExceptions();
                }
                catch (ObjectDisposedException) when (IsDisposed)
                {
                    break;
                }
            }
        }

        private async Task OnConnectedAsync(TcpClient client)
        {
            await Task.Yield();

            var stream = client.GetStream();

            var connection = new ModuleConnection(client, _serviceProvider);

            await connection.OpenAsync();

            if (connection.ConnectedModule == ModuleIdentifier.UnknownModule)
            {
                // TODO: Log failed connection.
                await connection.CloseAsync();
                return;
            }

            var entry = _entries.GetOrAdd(connection.ConnectedModule, id => new Entry { Connection = connection });

            var tcs = default(TaskCompletionSource<IModuleConnection>);

            using (await entry.Lock.LockAsync())
            {
                if (entry.Connection == null)
                {
                    entry.Connection = connection;
                }
                else if (entry.Connection != connection)
                {
                    // TODO: There is already a connection registered. What to do now?
                    // Currently we just abort the connection. 

                    await connection.CloseAsync();
                    return;
                }

                if (entry.Installation == null)
                {
                    if (!_allowDebugSessions)
                    {
                        // Debug-sessions are unallowed.

                        await connection.CloseAsync();
                        return;
                    }

                    entry.Connection.IsDebugSession = true;  // TODO: This is not thread-safe
                }

                tcs = entry.Tcs;

                entry.Tcs = null;
            }

            tcs?.TrySetResult(connection);

            try
            {
                var t = await Task.WhenAny(Completion, connection.UnderlyingEndPoint.Completion);
            }
            finally
            {
                await connection.CloseAsync();

                if (_entries.TryGetValue(connection.ConnectedModule, out var e))
                {
                    using (await e.Lock.LockAsync())
                    {
                        e.Connection = null;
                    }
                }
            }
        }

        private async Task CompleteAsync()
        {
            _tcpHost.Stop();
            await _receiveProcess.TerminateExecutionAndAwait();

            try
            {
                await Task.WhenAll(_entries.Values.Select(p => CompleteAsync()));
            }
            catch (Exception exc) when (!(exc is OperationCanceledException))
            {
                _completionSource.TrySetException(exc);
            }

            _completionSource.TrySetResult(null);
        }

        // TODO: The access to this type is mostly procedural currently. This should be changed in favor of a more oo model.
        private class Entry
        {
            public ModuleConnection Connection { get; set; }
            public AsyncLock Lock { get; } = new AsyncLock();
            public IModuleInstallation Installation { get; set; }
            public TaskCompletionSource<IModuleConnection> Tcs { get; set; }

            public async Task CompleteAsync()
            {
                var connection = default(IModuleConnection);

                using (await Lock.LockAsync())
                {
                    connection = Connection;
                }

                if (connection != null)
                {
                    await connection.CloseAsync();
                }
            }
        }
    }
}
