namespace AI4E.Modularity
{
    internal enum MessageType : ushort
    {
        /// <summary>
        /// An unknown message type.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// An initialize message.
        /// </summary>
        Init = 1,

        /// <summary>
        /// An initialize message ack.
        /// </summary>
        InitAck = 2,

        /// <summary>
        /// A terminate message.
        /// </summary>
        Terminate = 3,

        /// <summary>
        /// A terminate message ack.
        /// </summary>
        TerminateAck = 4,

        /// <summary>
        /// A normal (user) message.
        /// </summary>
        Message = 5,

        /// <summary>
        /// A message was received and will be handled. The payload is the seq-num of the message in raw-format.
        /// </summary>
        MessageReceived = 6,

        /// <summary>
        /// The answer to a message that was handled successfully. The payload is the answer, if any.
        /// </summary>
        MessageHandled = 7,

        /// <summary>
        /// The answer to a message that caused an error. The payload is the encoded error.
        /// </summary>
        MessageError = 8,

        /// <summary>
        /// The protocol of a received message is not supported. The payload is the seq-num of the message in raw format.
        /// </summary>
        ProtocolNotSupportedError = 9,

        BadMessage = 10,

        UnknownError = 11,

        MaxEnum = 11
    }
}
