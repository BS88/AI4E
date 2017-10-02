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

        public async Task<bool> TryAddModuleSourceAsync(IModuleSource moduleSource) // TODO: Forget about the try.
        {
            if (moduleSource == null)
                throw new ArgumentNullException(nameof(moduleSource));

            //if (await _moduleSourceStore.GetByAsync<ModuleSource>(p => p.Name == moduleSource.Name).Any())
            //{
            //    return false;
            //}

            await _moduleSourceStore.AddAsync(moduleSource.Name, moduleSource.Source, default);
            await _moduleSourceStore.SaveChangesAsync(default);
            return true;
        }

        public async Task<bool> TryRemoveModuleSourceAsync(IModuleSource moduleSource) // TODO: Forget about the try.
        {
            if (moduleSource == null)
                throw new ArgumentNullException(nameof(moduleSource));

            //if (!(await _moduleSourceStore.GetByAsync<ModuleSource>(p => p.Name == moduleSource.Name).Any()))
            //{
            //    return false;
            //}

            await _moduleSourceStore.RemoveAsync(moduleSource.Name, default);
            await _moduleSourceStore.SaveChangesAsync(default);
            return true;
        }

        // TODO: Update source

        public IModuleLoader GetModuleLoader(IModuleSource moduleSource)
        {
            return new ModuleLoader(moduleSource);
        }
    }
}
