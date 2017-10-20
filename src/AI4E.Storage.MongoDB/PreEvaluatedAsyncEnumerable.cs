using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AI4E.Storage.MongoDB
{
    public sealed class PreEvaluatedAsyncEnumerable<T> : IAsyncEnumerable<T>
    {
        private readonly Func<Task<IEnumerable<T>>> _preEvaluation;

        public PreEvaluatedAsyncEnumerable(Func<Task<IEnumerable<T>>> preEvaluation)
        {
            if (preEvaluation == null)
                throw new ArgumentNullException(nameof(preEvaluation));

            _preEvaluation = preEvaluation;
        }

        public IAsyncEnumerator<T> GetEnumerator()
        {
            return new PreEvaluatedAsyncEnumerator(_preEvaluation);
        }

        private sealed class PreEvaluatedAsyncEnumerator : IAsyncEnumerator<T>
        {
            private readonly Func<Task<IEnumerable<T>>> _preEvaluation;

            private IEnumerator<T> _enumerator;

            public PreEvaluatedAsyncEnumerator(Func<Task<IEnumerable<T>>> preEvaluation)
            {
                _preEvaluation = preEvaluation;
            }

            public async Task<bool> MoveNext(CancellationToken cancellationToken)
            {
                if (_enumerator == null)
                {
                    _enumerator = (await _preEvaluation()).GetEnumerator();
                }

                return _enumerator.MoveNext();
            }

            public T Current => _enumerator != null ? _enumerator.Current : default;

            public void Dispose()
            {
                _enumerator?.Dispose();
            }
        }
    }
}
