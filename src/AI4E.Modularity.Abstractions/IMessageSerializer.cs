namespace AI4E.Modularity
{
    public interface IMessageSerializer
    {
        MessageEncoding SupportedEncodings { get; }

        byte[] Serialize(object obj, MessageEncoding encoding);
        object Deserialize(byte[] data, MessageEncoding encoding);
    }
}
