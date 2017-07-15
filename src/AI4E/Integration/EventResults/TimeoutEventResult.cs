using System;
using System.Diagnostics;

namespace AI4E.Integration.EventResults
{
    public class TimeoutEventResult : FailureEventResult
    {
        public TimeoutEventResult(DateTime dueTime) : base("The event was not handled in due time.")
        {
            DueTime = dueTime;
        }

        public DateTime DueTime { get; }

        protected override bool IsEqualByValue(object obj)
        {
            Debug.Assert(obj is TimeoutEventResult);

            return DueTime == ((TimeoutEventResult)obj).DueTime;
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode() ^ DueTime.GetHashCode();
        }
    }
}
