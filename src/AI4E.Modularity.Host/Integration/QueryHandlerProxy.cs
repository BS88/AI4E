using System;
using System.Threading.Tasks;
using AI4E.Integration;

namespace AI4E.Modularity.Integration
{
    public sealed class QueryHandlerProxy<TQuery, TResult> :
        IQueryHandler<TQuery, TResult>,
        IHandlerFactory<IQueryHandler<TQuery, TResult>>,
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

        public async Task<TResult> HandleAsync(TQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var message = new DispatchQuery(typeof(TQuery), typeof(TResult), query);

            Console.WriteLine($"Sending 'DispatchQuery' for query type '{message.QueryType.FullName}' and result '{message.ResultType.FullName}' with query '{message.Query}'.");

            var answer = await _messageEndPoint.SendAsync<DispatchQuery, QueryDispatchResult>(message);

            return (TResult)answer.QueryResult;
        }

        IQueryHandler<TQuery, TResult> IHandlerFactory<IQueryHandler<TQuery, TResult>>.GetHandler(IServiceProvider serviceProvider)
        {
            return this;
        }

        public Task NotifyActivationAsync()
        {
            var message = new ActivateQueryForwarding(typeof(TQuery), typeof(TResult));

            Console.WriteLine($"Sending 'ActivateQueryForwarding' for query type '{message.QueryType.FullName}' and result '{message.ResultType.FullName}'.");

            return _messageEndPoint.SendAsync(message);
        }

        public Task NotifyDeactivationAsync()
        {
            var message = new DeactivateQueryForwarding(typeof(TQuery), typeof(TResult));

            Console.WriteLine($"Sending 'DeactivateQueryForwarding' for query type '{message.QueryType.FullName}' and result '{message.ResultType.FullName}'.");

            return _messageEndPoint.SendAsync(message);
        }
    }
}
