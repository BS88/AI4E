namespace AI4E.Modularity
{
    public interface IMessageSerializer
    {
        byte[] Serialize(object obj);
        object Deserialize(byte[] data);
    }
}
