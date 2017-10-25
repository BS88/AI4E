using System;

namespace AI4E.QueryResults
{
    public class TimeoutQueryResult : FailureQueryResult
    {
        public TimeoutQueryResult(DateTime dueTime) : base("The event was not handled in due time.")
        {
            DueTime = dueTime;
        }

        public DateTime DueTime { get; }
    }
}
