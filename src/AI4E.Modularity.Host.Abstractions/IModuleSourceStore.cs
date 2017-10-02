using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AI4E.Modularity
{
    public interface IModuleSourceStore
    {
        Task<IEnumerable<(string name, string source)>> GetAllAsync(CancellationToken cancellation);

        Task<(string name, string source)> GetByNameAsync(string name, CancellationToken cancellation);

        Task AddAsync(string name, string source, CancellationToken cancellation);

        Task RemoveAsync(string name, CancellationToken cancellation);

        Task UpdateAsync(string name, string source, CancellationToken cancellation);

        Task SaveChangesAsync(CancellationToken cancellation);

        Task DiscardChangesAsync(CancellationToken cancellation);
    }
}
