using AI4E.Storage;
using System;
using System.Threading.Tasks;

namespace AI4E.Domain.Services
{
    public sealed class ReferenceResolver : IReferenceResolver
    {
        private readonly IEntityStore<Guid, AggregateRoot> _entityStore;

        public ReferenceResolver(IEntityStore<Guid, AggregateRoot> entityStore)
        {
            if (entityStore == null)
                throw new ArgumentNullException(nameof(entityStore));

            _entityStore = entityStore;
        }

        public Task<TEntity> ResolveAsync<TEntity>(Guid id)
            where TEntity : AggregateRoot
        {
            if (id.Equals(default))
                return Task.FromResult(default(TEntity));

            return _entityStore.GetByIdAsync<TEntity>(id);
        }
    }
}
