using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace AI4E.Modularity
{
    public sealed class Serializer : ISerializer
    {
        private readonly JsonSerializer _jsonSerializer;

        public Serializer()
        {
            _jsonSerializer = new JsonSerializer
            {
                TypeNameHandling = TypeNameHandling.Auto
            };

            // _jsonSerializer.Converters.Add(new CommandResultJsonConverter());
        }

        public byte[] Serialize(object obj)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BsonDataWriter(stream))
                {
                    _jsonSerializer.Serialize(writer, obj, typeof(object));
                }
                return stream.ToArray();
            }
        }

        public object Deserialize(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                using (var reader = new BsonDataReader(stream))
                {
                    return _jsonSerializer.Deserialize(reader, typeof(object));
                }
            }
        }
    }
}
