using System.Threading.Tasks;

namespace AI4E.Modularity.Integration
{
    public interface IEventMessageTranslator
    {
        Task RegisterForwardingAsync<TEvent>();
        Task UnregisterForwardingAsync<TEvent>();
        Task DispatchAsync<TEvent>(TEvent evt);
    }
}
