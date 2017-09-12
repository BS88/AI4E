using System;
using System.Runtime.Serialization;

namespace AI4E.Modularity
{
    [Serializable]
    public class UnsupportedEncodingException : Exception
    {
        public UnsupportedEncodingException() { }

        protected UnsupportedEncodingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
