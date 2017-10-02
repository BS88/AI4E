/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        ModuleInstaller.cs
 * Types:           AI4E.Modularity.ModuleInstaller
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
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Nito.AsyncEx;

// TODO: Updates
// TODO: Dependency resolution

namespace AI4E.Modularity
{
    /// <summary>
    /// Represents a module installer that is able to install modules.
    /// </summary>
    public sealed partial class ModuleInstaller : IModuleInstaller
    {
        #region Fields

        private readonly IModuleSupervision _moduleSupervision;
        private readonly IModuleSourceManager _sourceManager;
        private readonly ConcurrentDictionary<ModuleIdentifier, ModuleInstallation> _installations = new ConcurrentDictionary<ModuleIdentifier, ModuleInstallation>();
        private readonly AsyncLock _lock = new AsyncLock();

        #endregion

        #region C'tor

        /// <summary>
        /// Creates a new instance of the <see cref="ModuleInstaller"/> type.
        /// </summary>
        /// <param name="moduleSupervision">The module supervisition that controls module runtime.</param>
        /// <param name="sourceManager">The module source manager.</param>
        /// <exception cref="ArgumentNullException">Thrown if either <paramref name="moduleSupervision"/> or <paramref name="sourceManager"/> is null.</exception>
        public ModuleInstaller(IModuleSupervision moduleSupervision, IModuleSourceManager sourceManager)
        {
            if (moduleSupervision == null)
                throw new ArgumentNullException(nameof(moduleSupervision));

            if (sourceManager == null)
                throw new ArgumentNullException(nameof(sourceManager));

            _moduleSupervision = moduleSupervision;
            _sourceManager = sourceManager;
        }

        #endregion

        /// <summary>
        /// Get a collection of installed modules.
        /// </summary>
        public IReadOnlyCollection<IModuleInstallation> InstalledModules => new List<IModuleInstallation>(_installations.Values);

        /// <summary>
        /// Asynchronously installs the module specified by its identifier.
        /// </summary>
        /// <param name="module">The identifier of the module release.</param>
        /// <param name="source">The module source, the module shall be loaded from.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="module"/> equals <see cref="ModuleReleaseIdentifier.UnknownModuleRelease"/>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null.</exception>
        /// <exception cref="ModuleInstallationException">Thrown if the specified module could not be installed.</exception>
        public async Task InstallAsync(ModuleReleaseIdentifier module, IModuleSource source)
        {
            if (module == ModuleReleaseIdentifier.UnknownModuleRelease)
                throw new ArgumentException("The module is not specified.", nameof(module));

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var moduleSupervisor = default(IModuleSupervisor);

            using (await _lock.LockAsync())
            {
                if (_installations.TryGetValue(module.Module, out var installation))
                {
                    if (installation.Version == module.Version)
                        return;

                    // The module is already registered as installed. (This is an update)

                    throw new NotImplementedException();
                }

                var moduleLoader = _sourceManager.GetModuleLoader(source);

                var (stream, metadata) = await moduleLoader.LoadModuleAsync(module);

                var installationDirectory = Path.Combine(".", "modules", module.Module.Name); // TODO: This shall be configurable

                using (var packageStream = new MemoryStream())
                {
                    await stream.CopyToAsync(packageStream, 4096);

                    using (var package = new ZipArchive(packageStream, ZipArchiveMode.Read))
                    {
                        package.ExtractToDirectory(installationDirectory);
                    }
                }

                installation = new ModuleInstallation(module.Module, metadata, installationDirectory, source);

                await _moduleSupervision.RegisterModuleInstallationAsync(installation);
                moduleSupervisor = _moduleSupervision.GetSupervisor(installation);
            }

            await moduleSupervisor.StartModuleAsync();
        }

        /// <summary>
        /// Asynchronously uninstalls the module specified by its identifier.
        /// </summary>
        /// <param name="module">The module that shall be uninstalled.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="module"/> equals <see cref="ModuleIdentifier.UnknownModule"/>.</exception>
        /// <exception cref="ModuleUninstallationException">Thrown if the module is currently installed but cannot be uninstalled.</exception>
        public async Task UninstallAsync(ModuleIdentifier module)
        {
            if (module == ModuleIdentifier.UnknownModule)
                throw new ArgumentException("The module is not specified.", nameof(module));

            using (await _lock.LockAsync())
            {
                if (!_installations.TryGetValue(module, out var installation))
                {
                    return;
                }

                var moduleSupervisor = _moduleSupervision.GetSupervisor(installation);

                await _moduleSupervision.UnregisterModuleInstallationAsync(installation);

                await moduleSupervisor.StopModuleAsync();

                if (Directory.Exists(installation.InstallationDirectory))
                {
                    Directory.Delete(installation.InstallationDirectory);
                }

                _installations.TryRemove(module, out _);
            }
        }
    }
}
