using System;
using System.Threading.Tasks;
using AI4E;

namespace AI4E.Modularity.Integration
{
    public sealed class QueryMessageTranslator : IQueryMessageTranslator
    {
        private readonly IMessageEndPoint _messageEndPoint;

        public QueryMessageTranslator(IMessageEndPoint messageEndPoint)
        {
            if (messageEndPoint == null)
                throw new ArgumentNullException(nameof(messageEndPoint));

            _messageEndPoint = messageEndPoint;
        }

        public Task RegisterForwardingAsync<TQuery>()
        {
            var message = new RegisterQueryForwarding(typeof(TQuery));

            Console.WriteLine($"Sending 'RegisterQueryForwarding' for query type '{message.QueryType.FullName}'.");

            return _messageEndPoint.SendAsync(message);
        }

        public Task UnregisterForwardingAsync<TQuery>()
        {
            var message = new UnregisterQueryForwarding(typeof(TQuery));

            Console.WriteLine($"Sending 'UnregisterQueryForwarding' for query type '{message.QueryType.FullName}'.");

            return _messageEndPoint.SendAsync(message);
        }

        public async Task<IQueryResult> DispatchAsync<TQuery>(TQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var message = new DispatchQuery(typeof(TQuery), query);

            Console.WriteLine($"Sending 'DispatchQuery' for query type '{message.QueryType.FullName}' with query '{message.Query}'.");

            var answer = await _messageEndPoint.SendAsync<DispatchQuery, QueryDispatchResult>(message);

            return answer.QueryResult;
        }
    }
}
