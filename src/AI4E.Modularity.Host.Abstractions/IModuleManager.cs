using System.Collections.Generic;
using System.Threading.Tasks;

namespace AI4E.Modularity
{
    public interface IModuleManager
    {
        Task<IEnumerable<IModule>> GetAvailableModulesAsync(bool includePreReleases = false);
        Task<IModule> GetModuleAsync(ModuleIdentifier moduleIdentifier);
        Task<IEnumerable<IModuleRelease>> GetUpdatesAsync(bool includePreReleases = false);
        Task<IEnumerable<IModuleRelease>> GetInstalledAsync();
        Task<IEnumerable<IModule>> GetDebugModulesAsync();
    }
}
