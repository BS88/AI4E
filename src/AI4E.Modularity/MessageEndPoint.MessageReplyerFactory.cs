using System;
using System.Diagnostics;

namespace AI4E.Modularity
{
    public sealed partial class MessageEndPoint
    {
        private sealed class MessageReplyerFactory<TMessage, TResponse> : IHandlerFactory<IMessageReplyer<TMessage>>
        {
            private readonly MessageEndPoint _endPoint;
            private readonly IHandlerFactory<IMessageHandler<TMessage, TResponse>> _handlerFactory;

            internal MessageReplyerFactory(MessageEndPoint endPoint, IHandlerFactory<IMessageHandler<TMessage, TResponse>> handlerFactory)
            {
                Debug.Assert(endPoint != null);
                Debug.Assert(handlerFactory != null);
                _endPoint = endPoint;
                _handlerFactory = handlerFactory;
            }

            public IMessageReplyer<TMessage> GetHandler(IServiceProvider serviceProvider)
            {
                return new MessageReplyer<TMessage, TResponse>(_endPoint, _handlerFactory.GetHandler(serviceProvider));
            }
        }

        private sealed class MessageReplyerFactory<TMessage> : IHandlerFactory<IMessageReplyer<TMessage>>
        {
            private readonly MessageEndPoint _endPoint;
            private readonly IHandlerFactory<IMessageHandler<TMessage>> _handlerFactory;

            internal MessageReplyerFactory(MessageEndPoint endPoint, IHandlerFactory<IMessageHandler<TMessage>> handlerFactory)
            {
                Debug.Assert(endPoint != null);
                Debug.Assert(handlerFactory != null);
                _endPoint = endPoint;
                _handlerFactory = handlerFactory;
            }

            public IMessageReplyer<TMessage> GetHandler(IServiceProvider serviceProvider)
            {
                return new MessageReplyer<TMessage>(_endPoint, _handlerFactory.GetHandler(serviceProvider));
            }
        }
    }
}
