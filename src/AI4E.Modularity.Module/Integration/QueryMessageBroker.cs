using System;
using System.Threading.Tasks;
using AI4E.Async;

namespace AI4E.Modularity.Integration
{
    public sealed class QueryMessageBroker :
        IMessageHandler<DispatchQuery, QueryDispatchResult>,
        IMessageHandler<ActivateQueryForwarding>,
        IMessageHandler<DeactivateQueryForwarding>
    {
        private readonly INonGenericRemoteQueryDispatcher _remoteQueryDispatcher;

        public QueryMessageBroker(INonGenericRemoteQueryDispatcher remoteQueryDispatcher)
        {
            if (remoteQueryDispatcher == null)
                throw new ArgumentNullException(nameof(remoteQueryDispatcher));

            _remoteQueryDispatcher = remoteQueryDispatcher;
        }

        public async ICovariantAwaitable<QueryDispatchResult> HandleAsync(DispatchQuery message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Console.WriteLine($"Received 'DispatchQuery' for query type '{message.QueryType.FullName}' with query '{message.Query}'.");

            var answer = new QueryDispatchResult(await _remoteQueryDispatcher.LocalDispatchAsync(message.QueryType, message.Query));

            return answer;
        }

        public Task HandleAsync(ActivateQueryForwarding message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Console.WriteLine($"Received 'ActivateQueryForwarding' for query type '{message.QueryType.FullName}'.");

            _remoteQueryDispatcher.NotifyForwardingActive(message.QueryType);

            return Task.CompletedTask;
        }

        public Task HandleAsync(DeactivateQueryForwarding message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Console.WriteLine($"Received 'DeactivateQueryForwarding' for query type '{message.QueryType.FullName}'.");

            _remoteQueryDispatcher.NotifyForwardingInactive(message.QueryType);

            return Task.CompletedTask;
        }
    }
}
