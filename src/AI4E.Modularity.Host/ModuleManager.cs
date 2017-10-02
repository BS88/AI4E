using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

// TODO: Dependency resolution
// TODO: Platform standard
// TODO: Load modules on restart

namespace AI4E.Modularity
{
    public sealed partial class ModuleManager : IModuleManager
    {
        private readonly IModuleSourceManager _moduleSourceStore;
        private readonly IModuleHost _moduleHost;
        private readonly IModuleInstaller _moduleInstaller;

        #region C'tor

        /// <summary>
        /// Creates a new instanc of the <see cref="ModuleManager"/> type.
        /// </summary>
        /// <param name="dataStore">The data store that is used to store the manager settings.</param>
        public ModuleManager(IModuleSourceManager moduleSourceStore, IModuleHost moduleHost, IModuleInstaller moduleInstaller)
        {
            if (moduleSourceStore == null)
                throw new ArgumentNullException(nameof(moduleSourceStore));

            if (moduleHost == null)
                throw new ArgumentNullException(nameof(moduleHost));

            if (moduleInstaller == null)
                throw new ArgumentNullException(nameof(moduleInstaller));

            _moduleSourceStore = moduleSourceStore;
            _moduleHost = moduleHost;
            _moduleInstaller = moduleInstaller;
        }

        #endregion

        public async Task<IEnumerable<IModule>> GetAvailableModulesAsync(bool includePreReleases = false)
        {
            var modules = new Dictionary<ModuleIdentifier, List<ModuleRelease>>();

            void AddRelease(ModuleRelease release)
            {
                if (!modules.TryGetValue(release.Identifier.Module, out var releaseList))
                {
                    releaseList = new List<ModuleRelease>();

                    modules.Add(release.Identifier.Module, releaseList);
                }

                if (!releaseList.Any(p => p.Identifier == release.Identifier))
                {
                    releaseList.Add(release);
                }
            }

            // The installed releases have to be added first, because a single release van be present in multiple sources. 
            // We want to select the source the module was installed from.
            foreach (var installedRelease in await GetInstalledAsync())
            {
                AddRelease((ModuleRelease)installedRelease);
            }

            foreach (var source in await _moduleSourceStore.GetModuleSourcesAsync())
            {
                var releases = await GetModuleReleasesAsync(source, includePreReleases);

                foreach (var release in releases)
                {
                    AddRelease(release);
                }
            }

            return modules.Select(p => new Module(p.Key, p.Value, _moduleInstaller));
        }

        public async Task<IModule> GetModuleAsync(ModuleIdentifier moduleIdentifier)
        {
            var releaseList = new List<ModuleRelease>();

            foreach (var installedRelease in (await GetInstalledAsync()).Where(p => p.Identifier.Module == moduleIdentifier))
            {
                releaseList.Add((ModuleRelease)installedRelease);
            }

            foreach (var source in await _moduleSourceStore.GetModuleSourcesAsync())
            {
                var releases = await GetModuleReleasesAsync(source, includePreReleases: true);

                foreach (var release in releases.Where(p => p.Identifier.Module == moduleIdentifier))
                {
                    releaseList.Add(release);
                }
            }

            if (releaseList.Any())
            {
                return new Module(moduleIdentifier, releaseList, _moduleInstaller);
            }

            return (await GetDebugModulesAsync()).FirstOrDefault(p => p.Identifier == moduleIdentifier);
        }

        public async Task<IEnumerable<IModuleRelease>> GetUpdatesAsync(bool includePreReleases = false)
        {
            var modules = new Dictionary<ModuleIdentifier, ModuleRelease>();

            foreach (var installedRelease in await GetInstalledAsync())
            {
                modules.Add(installedRelease.Identifier.Module, (ModuleRelease)installedRelease);
            }

            foreach (var source in await _moduleSourceStore.GetModuleSourcesAsync())
            {
                var releases = await GetModuleReleasesAsync(source, includePreReleases);

                foreach (var release in releases)
                {
                    if (modules.TryGetValue(release.Identifier.Module, out var latestRelease))
                    {
                        if (latestRelease.Version < release.Version)
                        {
                            modules[release.Identifier.Module] = release;
                        }
                    }
                }
            }

            var result = new List<ModuleRelease>();

            foreach (var installedRelease in await GetInstalledAsync())
            {
                var latestRelease = modules[installedRelease.Identifier.Module];

                if (latestRelease.Version > installedRelease.Version)
                {
                    result.Add(latestRelease);
                }
            }

            return result;
        }

        public async Task<IEnumerable<IModuleRelease>> GetInstalledAsync()
        {
            var result = new List<ModuleRelease>();

            foreach (var installation in _moduleInstaller.InstalledModules)
            {
                var moduleLoader = _moduleSourceStore.GetModuleLoader(installation.Source);
                Debug.Assert(moduleLoader != null);
                var manifest = await moduleLoader.LoadModuleMetadataAsync(new ModuleReleaseIdentifier(installation.Module, installation.Version));

                result.Add(new ModuleRelease(manifest, _moduleInstaller, installation.Source));
            }

            return result;
        }

        public Task<IEnumerable<IModule>> GetDebugModulesAsync()
        {
            return Task.FromResult<IEnumerable<IModule>>(_moduleHost.Connections.Where(p => p.IsDebugSession).Select(p => new Module(p.ConnectedModule, _moduleInstaller)));
        }

        private async Task<IEnumerable<ModuleRelease>> GetModuleReleasesAsync(IModuleSource source, bool includePreReleases)
        {
            var result = new List<ModuleRelease>();
            var moduleLoader = _moduleSourceStore.GetModuleLoader(source);

            Debug.Assert(moduleLoader != null);

            var availableModules = await moduleLoader.ListModulesAsync();

            foreach (var availableModule in availableModules)
            {
                var manifest = await moduleLoader.LoadModuleMetadataAsync(availableModule);

                result.Add(new ModuleRelease(manifest, _moduleInstaller, source));
            }

            return result;
        }
    }
}
