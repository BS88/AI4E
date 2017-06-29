using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AI4E.Modularity
{
    public interface IModuleManager
    {
        IEnumerable<VersionedModule> InstalledModules { get; }
        //IEnumerable<ModuleSource> ModuleSources { get; }

        Task<IEnumerable<IGrouping<Module, VersionedModule>>> GetAvailableModulesAsync();
        Task InstallAsync(VersionedModule module);
        Task UninstallAsync(VersionedModule module);
    }
}