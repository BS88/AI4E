namespace AI4E.Modularity
{
    public interface ISerializer
    {
        byte[] Serialize(object obj);
        object Deserialize(byte[] data);
    }
}
