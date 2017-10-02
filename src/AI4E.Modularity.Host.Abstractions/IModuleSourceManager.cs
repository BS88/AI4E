using System.Collections.Generic;
using System.Threading.Tasks;

namespace AI4E.Modularity
{
    public interface IModuleSourceManager
    {
        Task<IEnumerable<IModuleSource>> GetModuleSourcesAsync();

        Task<bool> TryAddModuleSourceAsync(IModuleSource moduleSource);

        Task<bool> TryRemoveModuleSourceAsync(IModuleSource moduleSource);

        IModuleLoader GetModuleLoader(IModuleSource moduleSource);
    }
}
