using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace AI4E.Modularity
{
    public sealed partial class ModuleSupervision : IModuleSupervision
    {
        private readonly ConcurrentDictionary<IModuleInstallation, ModuleSupervisor> _supervisors = new ConcurrentDictionary<IModuleInstallation, ModuleSupervisor>();
        private readonly IModuleHost _moduleHost;

        public ModuleSupervision(IModuleHost moduleHost)
        {
            if (moduleHost == null)
                throw new ArgumentNullException(nameof(moduleHost));

            _moduleHost = moduleHost;
        }

        public IModuleSupervisor GetSupervisor(IModuleInstallation installation)
        {
            if (_supervisors.TryGetValue(installation, out var result))
            {
                return result;
            }

            return default;
        }

        public async Task RegisterModuleInstallationAsync(IModuleInstallation installation)
        {
            if (_supervisors.TryAdd(installation, new ModuleSupervisor(installation, _moduleHost)))
            {
                await _moduleHost.RegisterInstallationAsync(installation);
            }
        }

        public async Task UnregisterModuleInstallationAsync(IModuleInstallation installation)
        {
            if (_supervisors.TryRemove(installation, out _))
            {
                await _moduleHost.UnregisterInstallationAsync(installation);
            }
        }
    }
}
