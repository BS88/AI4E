using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace AI4E.Storage.MongoDB
{
    internal sealed class MongoDBUnitOfWork<T> : IUnitOfWork<T>
    {
        private readonly HashSet<T> _inserted = new HashSet<T>();
        private readonly HashSet<T> _updated = new HashSet<T>();
        private readonly HashSet<T> _deleted = new HashSet<T>();

        private bool _isDisposed = false;
        private readonly IMongoCollection<SavedObject<T>> _mongoCollection;
        private readonly Func<object, Guid> _idResolver;

        public MongoDBUnitOfWork(IMongoCollection<SavedObject<T>> mongoCollection, Func<object, Guid> idResolver)
        {
            if (mongoCollection == null)
                throw new ArgumentNullException(nameof(mongoCollection));

            if (idResolver == null)
                throw new ArgumentNullException(nameof(idResolver));

            _mongoCollection = mongoCollection;
            _idResolver = idResolver;
        }

        /// <summary>
        /// Gets a boolean value inidacting whether any changes are pending.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        public bool ChangesPending => ThrowIfDisposed(_inserted.Any() || _updated.Any() || _deleted.Any());

        /// <summary>
        /// Gets the collection of items registered as being new.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        public IReadOnlyCollection<T> Inserted => ThrowIfDisposed(_inserted);

        /// <summary>
        /// Gets the collection of items registered as being updated.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        public IReadOnlyCollection<T> Updated => ThrowIfDisposed(_updated);

        /// <summary>
        /// Gets the collection of items registered as being deleted.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        public IReadOnlyCollection<T> Deleted => ThrowIfDisposed(_deleted);

        /// <summary>
        /// Registeres an object as beeing new.
        /// </summary>
        /// <param name="obj">The object to register.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="obj"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        public void RegisterNew(T obj)
        {
            ThrowIfDisposed();
            if (_deleted.Remove(obj))
            {
                _updated.Add(obj);
            }
            else if (!_updated.Contains(obj))
            {
                _inserted.Add(obj);
            }
        }

        /// <summary>
        /// Registeres an object as beeing updated.
        /// </summary>
        /// <param name="obj">The object to register.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="obj"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        public void RegisterUpdated(T obj)
        {
            ThrowIfDisposed();
            if (_deleted.Remove(obj))
            {
                _inserted.Add(obj);
            }
            else if (!_inserted.Contains(obj))
            {
                _updated.Add(obj);
            }
        }

        /// <summary>
        /// Registeres an object as beeing deleted.
        /// </summary>
        /// <param name="obj">The object to register.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="obj"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        public void RegisterDeleted(T obj)
        {
            ThrowIfDisposed();
            if (_inserted.Remove(obj))
            {
                return;
            }

            _updated.Remove(obj);
            _deleted.Add(obj);
        }

        /// <summary>
        /// Deregisters an object.
        /// </summary>
        /// <param name="obj">The object to deregister.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="obj"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        public void Deregister(T obj)
        {
            ThrowIfDisposed();
            _inserted.Remove(obj);
            _updated.Remove(obj);
            _deleted.Remove(obj);
        }

        /// <summary>
        /// Rolls back the unit of work.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        public void Rollback()
        {
            ThrowIfDisposed();
            ClearChanges();
        }

        private void ClearChanges()
        {
            _inserted.Clear();
            _updated.Clear();
            _deleted.Clear();
        }

        /// <summary>
        /// Asynchronously commits the unit of work.
        /// </summary>
        /// <param name="cancellation">A <see cref="CancellationToken"/> used to cancel the asynchronous operation or <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
        public async Task CommitAsync(CancellationToken cancellation)
        {
            ThrowIfDisposed();

            if (_inserted.Any())
            {
                var inserted = _inserted.Select(p => new SavedObject<T> { Data = p, Id = Guid.NewGuid() }); // TODO: How do we ensure that the id is not in use yet?

                await _mongoCollection.InsertManyAsync(inserted, cancellationToken: cancellation);
            }

            foreach (var updated in _updated)
            {
                var id = _idResolver(updated);

                await _mongoCollection.ReplaceOneAsync(p => p.Id == id, new SavedObject<T> { Id = id, Data = updated }, cancellationToken: cancellation);
            }

            foreach (var deleted in _deleted)
            {
                var id = _idResolver(deleted);

                await _mongoCollection.DeleteOneAsync(p => p.Id == id, cancellationToken: cancellation);
            }

            ClearChanges();
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private Q ThrowIfDisposed<Q>(Q t)
        {
            ThrowIfDisposed();
            return t;
        }

        public void Dispose()
        {
            _isDisposed = true;
        }
    }
}
