using System.Diagnostics;
using System.Threading.Tasks;

namespace AI4E.Modularity
{
    public sealed partial class MessageEndPoint
    {
        private sealed class MessageReplyer<TMessage, TResponse> : IMessageReplyer<TMessage>
        {
            private readonly MessageEndPoint _endPoint;
            private readonly IMessageHandler<TMessage, TResponse> _messageHandler;

            internal MessageReplyer(MessageEndPoint endPoint, IMessageHandler<TMessage, TResponse> messageHandler)
            {
                Debug.Assert(endPoint != null);
                Debug.Assert(messageHandler != null);

                _endPoint = endPoint;
                _messageHandler = messageHandler;
            }

            public async Task ReplyToAsync(TMessage message, uint seqNum)
            {
                var response = await _messageHandler.HandleAsync(message);
                var payload = _endPoint._serializer.Serialize(response, usedEncoding);
                await _endPoint.SendPayloadAsync(payload, _endPoint.GetNextSeqNum(), seqNum, MessageType.MessageHandled, usedEncoding, default);
            }
        }

        private sealed class MessageReplyer<TMessage> : IMessageReplyer<TMessage>
        {
            private readonly MessageEndPoint _endPoint;
            private readonly IMessageHandler<TMessage> _messageHandler;

            internal MessageReplyer(MessageEndPoint endPoint, IMessageHandler<TMessage> messageHandler)
            {
                Debug.Assert(endPoint != null);
                Debug.Assert(messageHandler != null);

                _endPoint = endPoint;
                _messageHandler = messageHandler;
            }

            public async Task ReplyToAsync(TMessage message, uint seqNum)
            {
                await _messageHandler.HandleAsync(message);
                await _endPoint.SendPayloadAsync(_emptyPayload, _endPoint.GetNextSeqNum(), seqNum, MessageType.MessageHandled, MessageEncoding.Unkown, default);
            }
        }
    }
}
