using System.Threading.Tasks;

namespace AI4E.Modularity
{
    public sealed partial class MessageEndPoint
    {
        private interface IMessageReplyer<TMessage>
        {
            Task ReplyToAsync(TMessage message, uint seqNum);
        }
    }
}
