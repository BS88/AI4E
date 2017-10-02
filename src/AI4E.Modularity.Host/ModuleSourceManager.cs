using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AI4E.Modularity
{
    public sealed partial class ModuleSourceManager : IModuleSourceManager
    {
        private readonly IModuleSourceStore _moduleSourceStore;

        public ModuleSourceManager(IModuleSourceStore moduleSourceStore)
        {
            if (moduleSourceStore == null)
                throw new ArgumentNullException(nameof(moduleSourceStore));

            _moduleSourceStore = moduleSourceStore;
        }

        public async Task<IEnumerable<IModuleSource>> GetModuleSourcesAsync()
        {
            return (await _moduleSourceStore.GetAllAsync(default)).Select(p => new ModuleSource(p.name, p.source));
        }

        public async Task AddModuleSourceAsync(IModuleSource moduleSource)
        {
            if (moduleSource == null)
                throw new ArgumentNullException(nameof(moduleSource));

            await _moduleSourceStore.AddAsync(moduleSource.Name, moduleSource.Source, default);
            await _moduleSourceStore.SaveChangesAsync(default);
        }

        public async Task RemoveModuleSourceAsync(IModuleSource moduleSource)
        {
            if (moduleSource == null)
                throw new ArgumentNullException(nameof(moduleSource));

            await _moduleSourceStore.RemoveAsync(moduleSource.Name, default);
            await _moduleSourceStore.SaveChangesAsync(default);
        }

        public async Task UpdateModuleSourceAsync(IModuleSource moduleSource)
        {
            if (moduleSource == null)
                throw new ArgumentNullException(nameof(moduleSource));

            await _moduleSourceStore.UpdateAsync(moduleSource.Name, moduleSource.Source, default);
            await _moduleSourceStore.SaveChangesAsync(default);
        }

        public IModuleLoader GetModuleLoader(IModuleSource moduleSource)
        {
            return new ModuleLoader(moduleSource);
        }
    }
}
