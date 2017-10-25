using System;

namespace AI4E.EventResults
{
    public class TimeoutEventResult : FailureEventResult
    {
        public TimeoutEventResult(DateTime dueTime) : base("The event was not handled in due time.")
        {
            DueTime = dueTime;
        }

        public DateTime DueTime { get; }
    }
}
