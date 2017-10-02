using System.Collections.Generic;
using System.Threading.Tasks;

namespace AI4E.Modularity
{
    public interface IModuleSourceManager
    {
        Task<IEnumerable<IModuleSource>> GetModuleSourcesAsync();

        Task AddModuleSourceAsync(IModuleSource moduleSource);

        Task RemoveModuleSourceAsync(IModuleSource moduleSource);

        Task UpdateModuleSourceAsync(IModuleSource moduleSource);

        IModuleLoader GetModuleLoader(IModuleSource moduleSource);
    }
}
