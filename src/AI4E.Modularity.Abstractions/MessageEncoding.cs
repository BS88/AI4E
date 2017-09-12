namespace AI4E.Modularity
{
    public enum MessageEncoding : byte
    {
        Unkown = 0b0000,
        Raw = 0b0001,
        BinarySerialized = 0b0010,
        Json = 0b0100,
        Bson = 0b1000
    }
}
