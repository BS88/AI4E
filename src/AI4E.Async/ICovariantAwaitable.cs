using System.Runtime.CompilerServices;

namespace AI4E.Async
{
    [AsyncMethodBuilder(typeof(AsyncCovariantAwaitableMethodBuilder<>))]
    public interface ICovariantAwaitable<out TResult>
    {
        bool IsCompleted { get; }
        bool IsCompletedSuccessfully { get; }
        bool IsFaulted { get; }
        bool IsCanceled { get; }
        TResult Result { get; }
        ICovariantAwaiter<TResult> GetAwaiter();
        ICovariantAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext);
    }

    public interface ICovariantAwaiter<out TResult> : ICriticalNotifyCompletion
    {
        bool IsCompleted { get; }
        bool IsCompletedSuccessfully { get; }
        bool IsFaulted { get; }
        bool IsCanceled { get; }
        TResult GetResult();
    }
}
