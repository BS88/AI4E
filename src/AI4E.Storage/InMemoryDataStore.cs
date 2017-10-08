using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace AI4E.Storage
{
    public class InMemoryDataStore : IDataStore
    {
        private static readonly List<object> _storage = new List<object>();
        private static readonly AsyncLock _lock = new AsyncLock();


        private readonly ISet<object> _created = new HashSet<object>();
        private readonly ISet<object> _updated = new HashSet<object>();
        private readonly ISet<object> _removed = new HashSet<object>();

        public bool ChangesPending => _created.Count > 0 || _updated.Count > 0 || _removed.Count > 0;

        public void Add<TData>(TData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            _created.Add(data);
        }

        public void Update<TData>(TData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            _updated.Add(data);
        }

        public void Remove<TData>(TData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            _removed.Add(data);
        }

        public void DiscardChanges()
        {
            _created.Clear();
            _updated.Clear();
            _removed.Clear();
        }

        public async Task SaveChangesAsync(CancellationToken cancellation = default)
        {
            using (await _lock.LockAsync())
            {
                foreach (var obj in _created)
                {
                    _storage.Add(obj);
                }

                foreach (var obj in _removed)
                {
                    _storage.Remove(obj);
                }
            }

            DiscardChanges();
        }

        public IAsyncEnumerable<TResult> QueryAsync<TData, TResult>(Func<IQueryable<TData>, IQueryable<TResult>> queryShaper, CancellationToken cancellation = default)
        {
            if (queryShaper == null)
                throw new ArgumentNullException(nameof(queryShaper));

            return new ResultEnumerable<TData, TResult>(queryShaper);
        }

        private sealed class ResultEnumerable<TData, TResult> : IAsyncEnumerable<TResult>
        {
            private readonly Func<IQueryable<TData>, IQueryable<TResult>> _queryShaper;

            public ResultEnumerable(Func<IQueryable<TData>, IQueryable<TResult>> queryShaper)
            {
                Debug.Assert(queryShaper != null);

                _queryShaper = queryShaper;
            }

            public IAsyncEnumerator<TResult> GetEnumerator()
            {
                return new ResultEnumerator(_queryShaper);
            }

            private sealed class ResultEnumerator : IAsyncEnumerator<TResult>
            {
                private readonly Func<IQueryable<TData>, IQueryable<TResult>> _queryShaper;

                private IEnumerator<TResult> _resultEnumerator;

                public ResultEnumerator(Func<IQueryable<TData>, IQueryable<TResult>> queryShaper)
                {
                    Debug.Assert(queryShaper != null);

                    _queryShaper = queryShaper;
                }

                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    if (_resultEnumerator == null)
                    {
                        lock (await _lock.LockAsync())
                        {
                            _resultEnumerator = _queryShaper(_storage.OfType<TData>().AsQueryable()).GetEnumerator();
                        }
                    }

                    return _resultEnumerator.MoveNext();
                }

                public TResult Current
                {
                    get
                    {
                        if (_resultEnumerator == null)
                            return default;

                        return _resultEnumerator.Current;
                    }
                }

                public void Dispose()
                {
                    _resultEnumerator.Dispose();
                }
            }
        }

        public void Dispose() { }
    }
}
