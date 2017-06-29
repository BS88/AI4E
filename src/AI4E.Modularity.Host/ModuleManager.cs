using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using AI4E.Async;
using AI4E.Async.Processing;
using Newtonsoft.Json;
using Nito.AsyncEx;

namespace AI4E.Modularity
{
    public sealed class ModuleManager : IModuleManager
    {
        private readonly List<ModuleSource> _moduleSources = new List<ModuleSource>();
        private readonly Dictionary<VersionedModule, ModuleRunnerBase> _installedModules = new Dictionary<VersionedModule, ModuleRunnerBase>();
        private readonly AsyncLock _lock = new AsyncLock();
        private readonly IServiceProvider _serviceProvider;
        private readonly string _workingDirectory;
        private readonly TcpListener _tcpListener;
        private readonly Task _execution;
        private readonly ConcurrentDictionary<Guid, ModuleConnection> _connections = new ConcurrentDictionary<Guid, ModuleConnection>();

        public ModuleManager(string workingDirectory, IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            if (string.IsNullOrWhiteSpace(workingDirectory))
                throw new ArgumentNullOrWhiteSpaceException(nameof(workingDirectory));

            _moduleSources.Add(new ModuleSource(Path.Combine(workingDirectory, "Available"), "default"));
            _serviceProvider = serviceProvider;
            _workingDirectory = workingDirectory;

            _tcpListener = new TcpListener(new IPEndPoint(IPAddress.Loopback, 4001));
            _tcpListener.Start();

            _execution = DoExecute();
        }

        private async Task DoExecute()
        {
            while (true)
            {
                var client = await _tcpListener.AcceptTcpClientAsync();
                var stream = client.GetStream();

                var connection = new ModuleConnection(stream, _serviceProvider);

                var runner = new DebugModuleRunner(connection);

                _installedModules.Add(new VersionedModule("Debug-Port", new ModuleVersion()), runner);

                //await Task.Delay(5000);

                //runner.Complete();

                //await runner.Completion;

                //_installedModules.Remove(new VersionedModule("Debug-Port", new ModuleVersion()));
            }
        }

        public IEnumerable<VersionedModule> InstalledModules => _installedModules.Keys.ToImmutableList();

        public IEnumerable<ModuleSource> ModuleSources => _moduleSources.AsReadOnly();

        public async Task<IEnumerable<IGrouping<Module, VersionedModule>>> GetAvailableModulesAsync()
        {
            return (await Task.WhenAll(_moduleSources.Select(p => p.GetAvailableModulesAsync()))).SelectMany(_ => _).GroupBy(p => new Module(p.Name));
        }

        public async Task InstallAsync(VersionedModule module)
        {
            ModuleRunner moduleSupervisor;

            using (await _lock.LockAsync())
            {
                if (_installedModules.ContainsKey(module))
                    return;

                var moduleInstalled = false;
                ModuleInstallation installation = null;

                foreach (var source in _moduleSources)
                {
                    if ((installation = await source.TryInstallModuleAsync(module, _workingDirectory)) != null)
                    {
                        moduleInstalled = true;
                        break;
                    }
                }

                if (!moduleInstalled)
                {
                    throw new Exception("No source available that can handle the specified module.");
                }

                Debug.Assert(installation != null);

                moduleSupervisor = new ModuleRunner(installation, _serviceProvider);

                _installedModules.Add(module, moduleSupervisor);
            }

            await moduleSupervisor.Initialization;
        }

        public async Task UninstallAsync(VersionedModule module)
        {
            ModuleRunnerBase moduleSupervisor;

            using (await _lock.LockAsync())
            {
                if (!_installedModules.TryGetValue(module, out moduleSupervisor))
                {
                    return;
                }

                moduleSupervisor.Complete();

                _installedModules.Remove(module);
            }

            await moduleSupervisor.Completion;

            if (moduleSupervisor is ModuleRunner runner)
                Directory.Delete(runner.Installation.InstallationDirectory, recursive: true);
        }
    }

    public sealed class ModuleSource
    {
        private ImmutableDictionary<VersionedModule, (string path, ModuleManifest manifest)> _lookup
            = ImmutableDictionary<VersionedModule, (string path, ModuleManifest manifest)>.Empty;

        public ModuleSource(string source, string name)
        {
            if (string.IsNullOrWhiteSpace(source))
                throw new ArgumentNullOrWhiteSpaceException(nameof(source));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullOrWhiteSpaceException(nameof(name));

            Source = source;
            Name = name;
        }

        public string Source { get; }

        public string Name { get; }

        public async Task<IEnumerable<VersionedModule>> GetAvailableModulesAsync()
        {
            var directory = new DirectoryInfo(Source);

            if (!directory.Exists)
                return Enumerable.Empty<VersionedModule>();

            var fileInfos = directory.GetFiles("*.aep", SearchOption.AllDirectories);

            var modules = new List<VersionedModule>();

            var lookup = _lookup.ToBuilder();

            foreach (var fileInfo in fileInfos)
            {
                using (var fileStream = fileInfo.OpenReadAsync())
                using (var package = new ZipArchive(fileStream, ZipArchiveMode.Read))
                {
                    var manifest = package.GetEntry("module.json");

                    // Invalid package
                    if (manifest == null)
                    {
                        continue;
                    }

                    using (var memStream = new MemoryStream())
                    using (var manifestStream = manifest.Open())
                    using (var manifestReader = new StreamReader(memStream))
                    using (var manifestJsonReader = new JsonTextReader(manifestReader))
                    {
                        await manifestStream.CopyToAsync(memStream, 4096);
                        memStream.Position = 0;

                        var moduleManifest = JsonSerializer.Create().Deserialize<ModuleManifest>(manifestJsonReader);

                        // Invalid package
                        if (moduleManifest == null)
                        {
                            continue;
                        }

                        modules.Add(new VersionedModule(moduleManifest.Name, moduleManifest.Version));

                        lookup[new VersionedModule(moduleManifest.Name, moduleManifest.Version)] = (fileInfo.FullName, moduleManifest);
                    }
                }
            }

            _lookup = lookup.ToImmutable();

            return modules;
        }

        internal async Task<ModuleInstallation> TryInstallModuleAsync(VersionedModule module, string installDirectory)
        {
            var lookup = _lookup;

            (string path, ModuleManifest manifest) packageDetails;

            while (!lookup.TryGetValue(module, out packageDetails))
            {
                if (!(await GetAvailableModulesAsync()).Contains(module))
                {
                    return null;
                }

                lookup = _lookup;
            }

            var fileInfo = new FileInfo(packageDetails.path);

            using (var fileStream = fileInfo.OpenRead())
            using (var package = new ZipArchive(fileStream, ZipArchiveMode.Read))
            {
                var moduleInstallationDirectory = Path.Combine(installDirectory, packageDetails.manifest.Name);
                Directory.Delete(moduleInstallationDirectory, recursive: true);
                package.ExtractToDirectory(moduleInstallationDirectory);

                return new ModuleInstallation { Manifest = packageDetails.manifest, InstallationDirectory = moduleInstallationDirectory };
            }
        }
    }

    internal sealed class ModuleManifest
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public ModuleVersion Version { get; set; }

        [JsonProperty("host-version")]
        public string HostVersion { get; set; }

        [JsonProperty("entry")]
        public string Entry { get; set; }

        [JsonProperty("dependencies")]
        public List<ModuleDependency> Dependencies { get; set; }
    }

    internal sealed class ModuleDependency
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public ModuleVersion Version { get; set; }
    }

    internal sealed class ModuleInstallation
    {
        public ModuleManifest Manifest { get; set; }

        public string InstallationDirectory { get; set; }
    }



    internal abstract class ModuleRunnerBase : IAsyncInitialization, IAsyncCompletion
    {
        private readonly TaskCompletionSource<object> _completionSource = new TaskCompletionSource<object>();
        private Task _completing;

        public abstract Task Initialization { get; }

        public Task Completion => _completionSource.Task;

        public async void Complete()
        {
            if (_completing != null)
            {
                return;
            }

            _completing = DoCompletionAsync();

            try
            {
                await _completing;
            }
            catch (Exception exc)
            {
                _completionSource.TrySetException(exc);
            }

            _completionSource.TrySetResult(null);
        }

        protected abstract Task DoCompletionAsync();
    }

    internal sealed class ModuleRunner : ModuleRunnerBase
    {
        private readonly ModuleInstallation _installation;
        private readonly AsyncProcess _process;
        private readonly IServiceProvider _serviceProvider;

        public ModuleRunner(ModuleInstallation installation, IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));
            _process = new AsyncProcess(ExecuteAsync);

            _installation = installation;
            _serviceProvider = serviceProvider;
            Initialization = DoInitializationAsync();
        }

        private async Task ExecuteAsync(CancellationToken cancellation)
        {
            while (cancellation.ThrowOrContinue())
            {
                var processCompletionSource = new TaskCompletionSource<object>();

                var listener = new TcpListener(IPAddress.Loopback, 0);
                listener.Start();
                var port = ((IPEndPoint)listener.LocalEndpoint).Port;

                var process = Process.Start("dotnet", "\"" + new DirectoryInfo(Path.Combine(Installation.InstallationDirectory, Installation.Manifest.Entry)).FullName + "\" " + port);

                var client = await listener.AcceptTcpClientAsync();
                var connection = new ModuleConnection(client.GetStream(), _serviceProvider);

                process.EnableRaisingEvents = true;
                process.Exited += (s, e) => processCompletionSource.TrySetResult(null);
                cancellation.Register(() => connection.Complete());

                await connection.Initialization;
                await Task.WhenAny(processCompletionSource.Task, connection.Completion);

                listener.Stop();
            }
        }

        protected async Task DoInitializationAsync()
        {
            await _process.StartExecutionAndAwait();
        }

        protected override async Task DoCompletionAsync()
        {
            await _process.TerminateExecutionAndAwait();
        }

        internal ModuleInstallation Installation => _installation;

        public override Task Initialization { get; }
    }

    internal sealed class DebugModuleRunner : ModuleRunnerBase
    {
        private readonly ModuleConnection _connection;

        public DebugModuleRunner(ModuleConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            _connection = connection;
            Initialization = DoInitializationAsync();
        }

        protected Task DoInitializationAsync()
        {
            return _connection.Initialization;
        }

        protected override Task DoCompletionAsync()
        {
            _connection.Complete();

            return _connection.Completion;
        }

        public override Task Initialization { get; }
    }
}
