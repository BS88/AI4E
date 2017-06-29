using System.Threading.Tasks;

namespace AI4E.Modularity.Integration
{
    public interface IQueryMessageTranslator
    {
        Task RegisterForwardingAsync<TQuery, TResult>();
        Task UnregisterForwardingAsync<TQuery, TResult>();

        Task<TResult> DispatchAsync<TQuery, TResult>(TQuery query);
    }
}
