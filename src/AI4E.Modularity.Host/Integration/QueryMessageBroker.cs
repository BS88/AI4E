using System;
using System.Threading.Tasks;
using AI4E.Async;

namespace AI4E.Modularity.Integration
{
    public sealed class QueryMessageBroker :
        IMessageHandler<RegisterQueryForwarding>,
        IMessageHandler<UnregisterQueryForwarding>,
        IMessageHandler<DispatchQuery, QueryDispatchResult>
    {
        private readonly IHostQueryDispatcher _queryDispatcher;

        public QueryMessageBroker(IHostQueryDispatcher queryDispatcher)
        {
            if (queryDispatcher == null)
                throw new ArgumentNullException(nameof(queryDispatcher));

            _queryDispatcher = queryDispatcher;
        }

        public Task HandleAsync(RegisterQueryForwarding message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Console.WriteLine($"Receiving 'RegisterQueryForwarding' for query type '{message.QueryType.FullName}'.");

            return _queryDispatcher.RegisterForwardingAsync(message.QueryType);
        }

        public Task HandleAsync(UnregisterQueryForwarding message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Console.WriteLine($"Receiving 'UnregisterQueryForwarding' for query type '{message.QueryType.FullName}'.");

            return _queryDispatcher.UnregisterForwardingAsync(message.QueryType);
        }

        public async ICovariantAwaitable<QueryDispatchResult> HandleAsync(DispatchQuery message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Console.WriteLine($"Receiving 'DispatchQuery' for query type '{message.QueryType.FullName}' with query '{message.Query}'.");

            var answer = new QueryDispatchResult(await _queryDispatcher.DispatchAsync(message.QueryType, message.Query));

            return answer;
        }
    }
}
