using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace AI4E.Storage.MongoDB
{
    public sealed class MongoDbDataStore : IDataStore
    {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly Dictionary<Type, object> _entries = new Dictionary<Type, object>();

        private readonly Dictionary<Guid, object> _identityMap = new Dictionary<Guid, object>();

        public MongoDbDataStore(IMongoDatabase mongoDatabase)
        {
            if (mongoDatabase == null)
                throw new ArgumentNullException(nameof(mongoDatabase));

            _mongoDatabase = mongoDatabase;
        }

        public bool ChangesPending => throw new NotImplementedException();

        public IUnitOfWork<TData> Entry<TData>()
        {
            if (!_entries.TryGetValue(typeof(TData), out var entry))
            {
                entry = new MongoDBUnitOfWork<TData>(GetCollection<TData>(), LookupId);
                _entries.Add(typeof(TData), entry);
            }

            Debug.Assert(entry is MongoDBUnitOfWork<TData>);

            return (MongoDBUnitOfWork<TData>)entry;
        }

        // TODO: This lookup is really slow O(n)
        private Guid LookupId<TData>(TData obj)
        {
            if (obj == null)
                return Guid.Empty;

            foreach (var entry in _identityMap)
            {
                if (entry.Value.Equals(obj))
                    return entry.Key;
            }

            return Guid.Empty;
        }

        private IMongoCollection<SavedObject<T>> GetCollection<T>()
        {
            // TODO
            try
            {
                BsonClassMap.RegisterClassMap<SavedObject<T>>();
            }
            catch { }

            return _mongoDatabase.GetCollection<SavedObject<T>>(typeof(T).Name);
        }

        private SavedObject<T> Register<T>(SavedObject<T> savedObject)
        {
            return savedObject;
        }

        public void Add<TData>(TData data)
        {
            Entry<TData>().RegisterNew(data);
        }

        public void Update<TData>(TData data)
        {
            Entry<TData>().RegisterUpdated(data);
        }

        public void Remove<TData>(TData data)
        {
            Entry<TData>().RegisterDeleted(data);
        }

        public void DiscardChanges()
        {
            foreach (var entry in _entries.Values)
            {
                ((dynamic)entry).Rollback(); // TODO
            }
        }

        public async Task SaveChangesAsync(CancellationToken cancellation = default)
        {
            foreach (var entry in _entries.Values)
            {
                await ((dynamic)entry).CommitAsync(cancellation); // TODO
            }
        }

        public IAsyncEnumerable<TResult> QueryAsync<TData, TResult>(Func<IQueryable<TData>, IQueryable<TResult>> queryShaper, CancellationToken cancellation = default)
        {
            var queryable = GetCollection<TData>().AsQueryable();

            var result = (IMongoQueryable<TResult>)queryShaper(queryable.Select(p => p.Data));

            return new PreEvaluatedAsyncEnumerable<TResult>(async () => (await result.ToListAsync())); // TODO: The entities have to be stored in the identity map
        }

        public void Dispose()
        {
            foreach (var entry in _entries.Values)
            {
                ((dynamic)entry).Dispose(); // TODO
            }
        }
    }
}
