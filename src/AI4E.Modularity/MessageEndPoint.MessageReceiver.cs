using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace AI4E.Modularity
{
    public sealed partial class MessageEndPoint
    {
        private sealed class MessageReceiver<TMessage> : IMessageReceiver
        {
            private readonly IAsyncSingleHandlerRegistry<IMessageReplyer<TMessage>> _handler = new AsyncSingleHandlerRegistry<IMessageReplyer<TMessage>>();
            private readonly MessageEndPoint _endPoint;

            public MessageReceiver(MessageEndPoint endPoint)
            {
                Debug.Assert(endPoint != null);

                _endPoint = endPoint;
            }

            public async Task<IHandlerRegistration> RegisterAsync<TResponse>(IContextualProvider<IMessageHandler<TMessage, TResponse>> handlerProvider)
            {
                var provider = new ContextualProvider<IMessageReplyer<TMessage>>(
                    serviceProvider => new MessageReplyer<TMessage, TResponse>(_endPoint, handlerProvider.ProvideInstance(serviceProvider)));

                return await HandlerRegistration.CreateRegistrationAsync(_handler, provider);
            }

            public async Task<IHandlerRegistration> RegisterAsync(IContextualProvider<IMessageHandler<TMessage>> handlerProvider)
            {
                var provider = new ContextualProvider<IMessageReplyer<TMessage>>(
                    serviceProvider => new MessageReplyer<TMessage>(_endPoint, handlerProvider.ProvideInstance(serviceProvider)));

                return await HandlerRegistration.CreateRegistrationAsync(_handler, provider);
            }

            public async Task HandleMessage(object message, uint seqNum)
            {
                var typedMessage = (TMessage)message;

                if (!_handler.TryGetHandler(out var handlerFactory))
                {
                    throw new MessageHandlerNotFoundException();
                }

                using (var scope = _endPoint._serviceProvider.CreateScope())
                {
                    await handlerFactory.ProvideInstance(scope.ServiceProvider).ReplyToAsync(typedMessage, seqNum);
                }
            }
        }
    }
}
