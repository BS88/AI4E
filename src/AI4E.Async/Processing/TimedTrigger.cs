using System;
using System.Threading;
using System.Threading.Tasks;

namespace AI4E.Async.Processing
{
    public sealed class TimedTrigger : ITrigger
    {
        public TimedTrigger(TimeSpan interval)
        {
            if (interval < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException();

            Interval = interval;
        }

        public TimeSpan Interval { get; }

        public Task NextTrigger(CancellationToken cancellation)
        {
            return Task.Delay(Interval, cancellation);
        }
    }
}
