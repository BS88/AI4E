using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using AI4E.Modularity.Integration;
using Microsoft.Extensions.DependencyInjection;

namespace AI4E.Modularity
{
    public sealed partial class ModuleSupervision
    {
        private sealed class ModuleSupervisor : IModuleSupervisor
        {
            private readonly IModuleHost _moduleHost;
            private TaskCompletionSource<object> _processTermination;

            public ModuleSupervisor(IModuleInstallation installation, IModuleHost moduleHost)
            {
                Debug.Assert(installation != null);
                Debug.Assert(moduleHost != null);

                ModuleInstallation = installation;
                _moduleHost = moduleHost;
            }

            public IModuleInstallation ModuleInstallation { get; }

            public IModuleConnection ModuleConnection { get; private set; }

            public async Task StartModuleAsync()
            {
                // Start the module process
                StartProcess();

                // Await module connection
                ModuleConnection = await _moduleHost.GetConnectionAsync(ModuleInstallation.Module);

                // TODO: The following should be configurable or auto detected from the assembly like command, query, event, ... handlers are.
                var messageEndPoint = ModuleConnection.UnderlyingEndPoint;

                var commandBrokerProvider = new ContextualProvider<CommandMessageBroker>(p => p.GetRequiredService<CommandMessageBroker>());
                var queryBrokerProvider = new ContextualProvider<QueryMessageBroker>(p => p.GetRequiredService<QueryMessageBroker>());
                var eventBrokerProvider = new ContextualProvider<EventMessageBroker>(p => p.GetRequiredService<EventMessageBroker>());

                await messageEndPoint.RegisterAsync(commandBrokerProvider);
                await messageEndPoint.RegisterAsync<RegisterCommandForwarding>(commandBrokerProvider);
                await messageEndPoint.RegisterAsync<UnregisterCommandForwarding>(commandBrokerProvider);

                await messageEndPoint.RegisterAsync(queryBrokerProvider);
                await messageEndPoint.RegisterAsync<RegisterQueryForwarding>(queryBrokerProvider);
                await messageEndPoint.RegisterAsync<UnregisterQueryForwarding>(queryBrokerProvider);

                await messageEndPoint.RegisterAsync<RegisterEventForwarding>(eventBrokerProvider);
                await messageEndPoint.RegisterAsync<UnregisterEventForwarding>(eventBrokerProvider);
                await messageEndPoint.RegisterAsync<DispatchEvent>(eventBrokerProvider);
            }

            public async Task StopModuleAsync()
            {
                // Signal the module to terminate
                await ModuleConnection.CloseAsync();

                // Await module process termination
                await _processTermination.Task;
            }

            private void StartProcess()
            {
                _processTermination = new TaskCompletionSource<object>();

                var entryPath = Path.Combine(ModuleInstallation.InstallationDirectory, ModuleInstallation.ModuleMetadata.EntryAssemblyPath);
                var process = Process.Start("dotnet", "\"" + new DirectoryInfo(entryPath).FullName + "\" " + _moduleHost.LocalEndPoint.Port);

                process.EnableRaisingEvents = true;
                process.Exited += (s, e) => _processTermination.TrySetResult(null);
            }
        }
    }
}
