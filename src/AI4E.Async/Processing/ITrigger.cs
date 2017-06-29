using System.Threading;
using System.Threading.Tasks;

namespace AI4E.Async.Processing
{
    public interface ITrigger
    {
        Task NextTrigger(CancellationToken cancellation);
    }
}
