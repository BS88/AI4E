using System;
using System.Threading.Tasks;

namespace AI4E.Async
{
    public static class TaskExtension
    {
        public static bool IsRunning(this Task task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            return !(task.IsCanceled || task.IsCompleted || task.IsFaulted);
        }

        public static void HandleExceptions(this Task task) // TODO: Receive an instance of ILogger type
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            task.ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    Console.WriteLine(t.Exception.ToString());
                    Console.WriteLine();
                }
            });
        }
    }
}
