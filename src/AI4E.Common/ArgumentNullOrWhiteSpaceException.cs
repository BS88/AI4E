using System;
using System.Runtime.Serialization;

namespace AI4E
{
    [Serializable]
    public class ArgumentNullOrWhiteSpaceException : ArgumentException
    {
        public ArgumentNullOrWhiteSpaceException() : base("The argument must neither be null nor an empty string or a string that consists of whitespace only.") { }

        public ArgumentNullOrWhiteSpaceException(string paramName) : base("The argument must neither be null nor an empty string or a string that consists of whitespace only.", paramName) { }

        public ArgumentNullOrWhiteSpaceException(string message, Exception innerException) : base(message, "The argument must neither be null nor an empty string or a string that consists of whitespace only.", innerException) { }

        public ArgumentNullOrWhiteSpaceException(string paramName, string message) : base(message, paramName) { }

        protected ArgumentNullOrWhiteSpaceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
