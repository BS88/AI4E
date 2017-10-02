using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AI4E.Async
{
    public static class AsyncEnumerableExtension
    {
        public static TaskAwaiter<T[]> GetAwaiter<T>(this IAsyncEnumerable<T> asyncEnumerable)
        {
            if (asyncEnumerable == null)
                throw new ArgumentNullException(nameof(asyncEnumerable));

            return asyncEnumerable.ToArray().GetAwaiter();
        }
    }
}
