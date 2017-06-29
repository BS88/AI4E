using System.Threading.Tasks;

namespace AI4E.Async
{
    public interface IAwaitable<TResult> : ICovariantAwaitable<TResult>
    {
        Task<TResult> AsTask();
        new IAwaiter<TResult> GetAwaiter();
        new IAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext);
    }

    public interface IAwaiter<TResult> : ICovariantAwaiter<TResult> { }
}
