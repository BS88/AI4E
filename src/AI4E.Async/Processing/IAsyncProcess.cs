using System.Threading.Tasks;

namespace AI4E.Async.Processing
{
    public interface IAsyncProcess
    {
        Task Execution { get; }

        AsyncProcessState State { get; }

        Task Initialization { get; }

        Task Termination { get; }

        void StartExecution();

        void TerminateExecution();

        Task StartExecutionAndAwait();

        Task TerminateExecutionAndAwait();
    }

    public enum AsyncProcessState
    {
        Terminated = 0,
        Running = 1,
        Failed = 2
    }
}
