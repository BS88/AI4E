using System;
using System.Threading.Tasks;

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

        public Task RegisterForwardingAsync<TQuery, TResult>()
        {
            var message = new RegisterQueryForwarding(typeof(TQuery), typeof(TResult));

            Console.WriteLine($"Sending 'RegisterQueryForwarding' for query type '{message.QueryType.FullName}' and result '{message.ResultType.FullName}'.");

            return _messageEndPoint.SendAsync(message);
        }

        public Task UnregisterForwardingAsync<TQuery, TResult>()
        {
            var message = new UnregisterQueryForwarding(typeof(TQuery), typeof(TResult));

            Console.WriteLine($"Sending 'UnregisterQueryForwarding' for query type '{message.QueryType.FullName}' and result '{message.ResultType.FullName}'.");

            return _messageEndPoint.SendAsync(message);
        }

        public async Task<TResult> DispatchAsync<TQuery, TResult>(TQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var message = new DispatchQuery(typeof(TQuery), typeof(TResult), query);

            Console.WriteLine($"Sending 'DispatchQuery' for query type '{message.QueryType.FullName}' and result '{message.ResultType.FullName}' with query '{message.Query}'.");

            var answer = await _messageEndPoint.SendAsync<DispatchQuery, QueryDispatchResult>(message);

            return (TResult)answer.QueryResult;
        }
    }
}
