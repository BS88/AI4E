using System.Threading.Tasks;
using AI4E.Async;

namespace AI4E.Modularity
{
    public interface IMessageHandler<in TMessage>
    {
        Task HandleAsync(TMessage message);
    }

    public interface IMessageHandler<in TMessage, out TResponse>
    {
        ICovariantAwaitable<TResponse> HandleAsync(TMessage message);
    }
}
