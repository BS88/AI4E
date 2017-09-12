using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace AI4E.Modularity
{
    public sealed class MessageSerializer : IMessageSerializer
    {
        private readonly JsonSerializer _jsonSerializer;
        private readonly BinaryFormatter _binaryFormatter;

        public MessageSerializer()
        {
            _jsonSerializer = new JsonSerializer { TypeNameHandling = TypeNameHandling.Auto };
            _binaryFormatter = new BinaryFormatter();
        }

        public MessageEncoding SupportedEncodings => MessageEncoding.Bson | MessageEncoding.Json | MessageEncoding.BinarySerialized;

        public byte[] Serialize(object obj, MessageEncoding encoding)
        {
            using (var stream = new MemoryStream())
            {
                if (encoding == MessageEncoding.Json)
                {
                    using (var writer = new JsonTextWriter(new StreamWriter(stream, Encoding.UTF8)))
                    {
                        _jsonSerializer.Serialize(writer, obj, typeof(object));
                    }
                }
                else if (encoding == MessageEncoding.Bson)
                {
                    using (var writer = new BsonDataWriter(stream))
                    {
                        _jsonSerializer.Serialize(writer, obj, typeof(object));
                    }
                }
                else if (encoding == MessageEncoding.BinarySerialized)
                {
                    _binaryFormatter.Serialize(stream, obj);
                }
                else
                {
                    throw new UnsupportedEncodingException();
                }

                return stream.ToArray();
            }
        }

        public object Deserialize(byte[] data, MessageEncoding encoding)
        {
            using (var stream = new MemoryStream(data))
            {
                if (encoding == MessageEncoding.Json)
                {
                    using (var reader = new JsonTextReader(new StreamReader(stream, Encoding.UTF8)))
                    {
                        return _jsonSerializer.Deserialize(reader, typeof(object));
                    }
                }
                else if (encoding == MessageEncoding.Bson)
                {
                    using (var reader = new BsonDataReader(stream))
                    {
                        return _jsonSerializer.Deserialize(reader, typeof(object));
                    }
                }
                else if (encoding == MessageEncoding.BinarySerialized)
                {
                    return _binaryFormatter.Deserialize(stream);
                }
                else
                {
                    throw new UnsupportedEncodingException();
                }
            }
        }
    }
}
