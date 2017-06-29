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
            private readonly MessageEndPoint _messageEndPoint;

            public MessageReceiver(MessageEndPoint messageEndPoint)
            {
                Debug.Assert(messageEndPoint != null);

                _messageEndPoint = messageEndPoint;
            }

            public async Task<IHandlerRegistration> RegisterAsync<TResponse>(IHandlerFactory<IMessageHandler<TMessage, TResponse>> handlerFactory)
            {
                return await HandlerRegistration.CreateRegistrationAsync(_handler, new MessageReplyerFactory<TMessage, TResponse>(_messageEndPoint, handlerFactory));
            }

            public async Task<IHandlerRegistration> RegisterAsync(IHandlerFactory<IMessageHandler<TMessage>> handlerFactory)
            {
                return await HandlerRegistration.CreateRegistrationAsync(_handler, new MessageReplyerFactory<TMessage>(_messageEndPoint, handlerFactory));
            }

            public async Task HandleMessage(object message, uint seqNum)
            {
                var typedMessage = (TMessage)message;

                if (!_handler.TryGetHandler(out var handlerFactory))
                {
                    throw new MessageHandlerNotFoundException();
                }

                using (var scope = _messageEndPoint._serviceProvider.CreateScope())
                {
                    await handlerFactory.GetHandler(scope.ServiceProvider).ReplyToAsync(typedMessage, seqNum);
                }
            }
        }
    }
}
