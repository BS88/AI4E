﻿using System;
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
        //private readonly IModuleSourceManager _moduleSourcManager;
        private readonly IModuleHost _moduleHost;
        private readonly IModuleInstaller _moduleInstaller;

        #region C'tor

        /// <summary>
        /// Creates a new instanc of the <see cref="ModuleManager"/> type.
        /// </summary>
        /// <param name="dataStore">The data store that is used to store the manager settings.</param>
        public ModuleManager(/*IModuleSourceManager moduleSourceManager,*/ IModuleHost moduleHost, IModuleInstaller moduleInstaller)
        {
            //if (moduleSourceManager == null)
            //    throw new ArgumentNullException(nameof(moduleSourceManager));

            if (moduleHost == null)
                throw new ArgumentNullException(nameof(moduleHost));

            if (moduleInstaller == null)
                throw new ArgumentNullException(nameof(moduleInstaller));

            //_moduleSourcManager = moduleSourceManager;
            _moduleHost = moduleHost;
            _moduleInstaller = moduleInstaller;
        }

        #endregion

        public async Task<IEnumerable<IModule>> GetModulesAsync(bool includePreReleases = false)
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

            foreach (var source in await GetModuleSourcesAsync())
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

            foreach (var source in await GetModuleSourcesAsync())
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

            foreach (var source in await GetModuleSourcesAsync())
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

        public Task<IEnumerable<IModuleRelease>> GetInstalledAsync()
        {
            var result = new List<ModuleRelease>();

            foreach (var installation in _moduleInstaller.InstalledModules)
            {
                result.Add(new ModuleRelease(installation.ModuleMetadata, _moduleInstaller, installation.Source));
            }

            return Task.FromResult<IEnumerable<IModuleRelease>>(result);
        }

        public Task<IEnumerable<IModule>> GetDebugModulesAsync()
        {
            return Task.FromResult<IEnumerable<IModule>>(_moduleHost.Connections.Where(p => p.IsDebugSession).Select(p => new Module(p.ConnectedModule, _moduleInstaller)));
        }

        private async Task<IEnumerable<ModuleRelease>> GetModuleReleasesAsync(IModuleSource source, bool includePreReleases)
        {
            var result = new List<ModuleRelease>();
            var moduleLoader = _moduleInstaller.GetModuleLoader(source);

            Debug.Assert(moduleLoader != null);

            var availableModules = await moduleLoader.ListModulesAsync();

            foreach (var availableModule in availableModules)
            {
                var manifest = await moduleLoader.LoadModuleMetadataAsync(availableModule);

                result.Add(new ModuleRelease(manifest, _moduleInstaller, source));
            }

            return result;
        }

        public Task<IModuleRelease> GetModuleReleaseAsync(ModuleReleaseIdentifier moduleReleaseIdentifier)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IModuleSource>> GetModuleSourcesAsync()
        {
            return Task.FromResult<IEnumerable<IModuleSource>>(_moduleInstaller.ModuleSources);
        }

        public Task AddModuleSourceAsync(string name, string source)
        {
            return _moduleInstaller.AddModuleSourceAsync(name, source);
        }

        public Task RemoveModuleSourceAsync(string name)
        {
            return _moduleInstaller.RemoveModuleSourceAsync(name);
        }

        public Task UpdateModuleSourceAsync(string name, string source)
        {
            return _moduleInstaller.UpdateModuleSourceAsync(name, source);
        }
    }
}
