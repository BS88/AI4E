/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        ModuleHost.ModuleConnection.cs
 * Types:           AI4E.Modularity.ModuleHost.ModuleConnection
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
using System.Net.Sockets;
using System.Threading.Tasks;
using AI4E.Integration;
using AI4E.Modularity.Integration;
using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx;

namespace AI4E.Modularity
{
    public sealed partial class ModuleHost
    {
        private sealed class ModuleConnection : IModuleConnection
        {
            private readonly TcpClient _client;
            private readonly IServiceProvider _serviceProvider;
            private readonly AsyncLock _lock = new AsyncLock();

            public ModuleConnection(TcpClient client, IServiceProvider serviceProvider)
            {
                if (client == null)
                    throw new ArgumentNullException(nameof(client));

                if (serviceProvider == null)
                    throw new ArgumentNullException(nameof(serviceProvider));

                _client = client;
                _serviceProvider = serviceProvider;
            }

            public bool IsOpened { get; private set; }

            public IMessageEndPoint UnderlyingEndPoint { get; private set; }

            public async Task OpenAsync()
            {
                using (await _lock.LockAsync())
                {
                    if (IsOpened)
                    {
                        return;
                    }

                    var serviceProvider = BuildSandboxedServices();

                    UnderlyingEndPoint = serviceProvider.GetRequiredService<IMessageEndPoint>();

                    await UnderlyingEndPoint.Initialization;

                    var initAnswer = await UnderlyingEndPoint.SendAsync<InitializeModule, ModuleInitialized>(new InitializeModule());

                    if (initAnswer == null)
                    {
                        UnderlyingEndPoint.Complete();
                        await UnderlyingEndPoint.Completion;

                        return;
                    }

                    ConnectedModule = initAnswer.Module;
                    ConnectedModuleVersion = initAnswer.Version;

                    IsOpened = true;
                }
            }

            // TODO: This should be configurable by configuration
            private IServiceProvider BuildSandboxedServices()
            {
                var sandboxedServices = new ServiceCollection();

                sandboxedServices.AddSingleton(_serviceProvider.GetRequiredService<ICommandDispatcher>());
                sandboxedServices.AddSingleton(_serviceProvider.GetRequiredService<IQueryDispatcher>());
                sandboxedServices.AddSingleton(_serviceProvider.GetRequiredService<IEventDispatcher>());

                sandboxedServices.AddSingleton<IHostCommandDispatcher, HostCommandDispatcher>();
                sandboxedServices.AddSingleton<IHostQueryDispatcher, HostQueryDispatcher>();
                sandboxedServices.AddSingleton<IHostEventDispatcher, HostEventDispatcher>();

                sandboxedServices.AddSingleton<IMessageSerializer, MessageSerializer>();

                sandboxedServices.AddSingleton<IMessageEndPoint>(provider => new MessageEndPoint(_client.GetStream(), provider.GetRequiredService<IMessageSerializer>(), provider));

                sandboxedServices.AddScoped<CommandMessageBroker>();
                sandboxedServices.AddScoped<QueryMessageBroker>();
                sandboxedServices.AddScoped<EventMessageBroker>();

                return sandboxedServices.BuildServiceProvider();
            }

            public async Task CloseAsync()
            {
                using (await _lock.LockAsync())
                {
                    if (!IsOpened)
                    {
                        return;
                    }

                    var terminateAnswer = await UnderlyingEndPoint.SendAsync<TerminateModule, ModuleTerminated>(new TerminateModule());

                    if (terminateAnswer == null)
                    {
                        // TODO: Log
                    }

                    IsOpened = false;

                    UnderlyingEndPoint.Complete();
                    await UnderlyingEndPoint.Completion;
                }
            }

            public ModuleIdentifier ConnectedModule { get; private set; }

            public bool IsDebugSession { get; set; }

            public ModuleVersion ConnectedModuleVersion { get; private set; }
        }
    }
}
