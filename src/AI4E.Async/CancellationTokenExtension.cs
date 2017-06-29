using System.Threading;
using System.Threading.Tasks;

namespace AI4E.Async
{
    public static class CancellationTokenExtension
    {
        public static bool ThrowOrContinue(this CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return true;
        }

        public static Task AsTask(this CancellationToken cancellationToken)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            cancellationToken.Register(() => tcs.SetCanceled());

            return tcs.Task;
        }
    }
}
