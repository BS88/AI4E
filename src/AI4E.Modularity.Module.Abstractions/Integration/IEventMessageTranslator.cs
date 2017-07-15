using System.Threading.Tasks;
using AI4E.Integration;

namespace AI4E.Modularity.Integration
{
    public interface IEventMessageTranslator
    {
        Task RegisterForwardingAsync<TEvent>();
        Task UnregisterForwardingAsync<TEvent>();
        Task<IAggregateEventResult> DispatchAsync<TEvent>(TEvent evt);
    }
}
