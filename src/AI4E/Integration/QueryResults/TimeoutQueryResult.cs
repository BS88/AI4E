using System;
using System.Diagnostics;

namespace AI4E.Integration.QueryResults
{
    public class TimeoutQueryResult : FailureQueryResult
    {
        public TimeoutQueryResult(DateTime dueTime) : base("The event was not handled in due time.")
        {
            DueTime = dueTime;
        }

        public DateTime DueTime { get; }

        protected override bool IsEqualByValue(object obj)
        {
            Debug.Assert(obj is TimeoutQueryResult);

            return DueTime == ((TimeoutQueryResult)obj).DueTime;
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode() ^ DueTime.GetHashCode();
        }
    }
}
