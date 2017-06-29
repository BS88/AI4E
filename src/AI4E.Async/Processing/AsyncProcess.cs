using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace AI4E.Async.Processing
{
    public sealed class AsyncProcess : IAsyncProcess
    {
        private readonly Func<CancellationToken, Task> _operation;
        private readonly object _lock = new object();

        private Task _execution = Task.CompletedTask;
        private CancellationTokenSource _cancellationSource;

        private TaskCompletionSource<object> _startNotificationSource;
        private TaskCompletionSource<object> _terminationNotificationSource;

        public AsyncProcess(Func<CancellationToken, Task> operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            _operation = operation;
        }

        public Task Execution => _execution;

        public Task Initialization => _startNotificationSource.Task;

        public Task Termination => _terminationNotificationSource.Task;

        public Func<CancellationToken, Task> Operation => _operation;

        public AsyncProcessState State
        {
            get
            {
                if (_execution.IsRunning())
                    return AsyncProcessState.Running;

                if (_execution.IsCompleted)
                    return AsyncProcessState.Terminated;

                return AsyncProcessState.Failed;
            }
        }

        public void StartExecution()
        {
            lock (_lock)
            {
                if (_execution.IsRunning())
                    return;

                _startNotificationSource = new TaskCompletionSource<object>();
                _terminationNotificationSource = new TaskCompletionSource<object>();
                _cancellationSource = new CancellationTokenSource();
                _execution = Execute();
            }
        }

        public Task StartExecutionAndAwait()
        {
            StartExecution();

            return _startNotificationSource.Task;
        }

        public void TerminateExecution()
        {
            lock (_lock)
            {
                if (!_execution.IsRunning())
                    return;

                _cancellationSource.Cancel();
            }
        }

        public Task TerminateExecutionAndAwait()
        {
            TerminateExecution();

            return _terminationNotificationSource.Task;
        }

        private async Task Execute()
        {
            try
            {
                var cancellation = _cancellationSource.Token;

                await Task.Yield();

                try
                {
                    _startNotificationSource.TrySetResult(null);

                    await _operation(cancellation).ConfigureAwait(false);

                    throw new UnexpectedProcessTerminationException();
                }
                catch (TaskCanceledException)
                {
                    if (!_cancellationSource.IsCancellationRequested)
                    {
                        throw new UnexpectedProcessTerminationException();
                    }
                }
            }
            finally
            {
                _terminationNotificationSource.TrySetResult(null);
            }
        }
    }

    [Serializable]
    public sealed class UnexpectedProcessTerminationException : Exception
    {
        public UnexpectedProcessTerminationException() : base("The process terminated unexpectedly.") { }

        public UnexpectedProcessTerminationException(string message) : base(message) { }

        public UnexpectedProcessTerminationException(string message, Exception innerException) : base(message, innerException) { }

        private UnexpectedProcessTerminationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
