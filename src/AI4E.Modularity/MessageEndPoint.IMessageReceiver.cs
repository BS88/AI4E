using System.Threading.Tasks;

namespace AI4E.Modularity
{
    public sealed partial class MessageEndPoint
    {
        private interface IMessageReceiver
        {
            Task HandleMessage(object message, uint seqNum);
        }
    }
}
