using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace AI4E.Async
{
    [StructLayout(LayoutKind.Auto)]
    public struct AsyncCovariantAwaitableMethodBuilder<TResult>
    {
        private AsyncTaskMethodBuilder<TResult> _methodBuilder;

        public static AsyncCovariantAwaitableMethodBuilder<TResult> Create()
        {
            return new AsyncCovariantAwaitableMethodBuilder<TResult>()
            {
                _methodBuilder = AsyncTaskMethodBuilder<TResult>.Create()
            };
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            _methodBuilder.Start(ref stateMachine);
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            _methodBuilder.SetStateMachine(stateMachine);
        }

        public void SetResult(TResult result)
        {
            _methodBuilder.SetResult(result);
        }

        public void SetException(Exception exception)
        {
            _methodBuilder.SetException(exception);
        }

        public ICovariantAwaitable<TResult> Task => new CovariantAwaitable<TResult>(_methodBuilder.Task);


        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            _methodBuilder.AwaitOnCompleted(ref awaiter, ref stateMachine);
        }


        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            _methodBuilder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
        }
    }
}
