using System;
using System.Threading.Tasks;
using AI4E;

namespace AI4E.Modularity.Integration
{
    public sealed class QueryHandlerProxy<TQuery> :
        IQueryHandler<TQuery>,
        IContextualProvider<IQueryHandler<TQuery>>,
        IActivationNotifyable,
        IDeactivationNotifyable
    {
        private readonly IMessageEndPoint _messageEndPoint;

        public QueryHandlerProxy(IMessageEndPoint messageEndPoint)
        {
            if (messageEndPoint == null)
                throw new ArgumentNullException(nameof(messageEndPoint));

            _messageEndPoint = messageEndPoint;
        }

        public async Task<IQueryResult> HandleAsync(TQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var message = new DispatchQuery(typeof(TQuery), query);

            Console.WriteLine($"Sending 'DispatchQuery' for query type '{message.QueryType.FullName}' with query '{message.Query}'.");

            var answer = await _messageEndPoint.SendAsync<DispatchQuery, QueryDispatchResult>(message);

            return answer.QueryResult;
        }

        IQueryHandler<TQuery> IContextualProvider<IQueryHandler<TQuery>>.ProvideInstance(IServiceProvider serviceProvider)
        {
            return this;
        }

        public Task NotifyActivationAsync()
        {
            var message = new ActivateQueryForwarding(typeof(TQuery));

            Console.WriteLine($"Sending 'ActivateQueryForwarding' for query type '{message.QueryType.FullName}'.");

            return _messageEndPoint.SendAsync(message);
        }

        public Task NotifyDeactivationAsync()
        {
            var message = new DeactivateQueryForwarding(typeof(TQuery));

            Console.WriteLine($"Sending 'DeactivateQueryForwarding' for query type '{message.QueryType.FullName}'.");

            return _messageEndPoint.SendAsync(message);
        }
    }
}
