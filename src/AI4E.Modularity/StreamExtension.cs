using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AI4E.Modularity
{
    public static class StreamExtension
    {
        public static async Task ReadExactAsync(this Stream stream, byte[] buffer, int offset, int count, CancellationToken cancellation)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            while (count > 0)
            {
                var readBytes = await stream.ReadAsync(buffer, offset, count, cancellation);

                if (readBytes == 0)
                    throw new EndOfStreamException();

                count -= readBytes;
                offset += readBytes;

                Debug.Assert(!(count < 0));
            }
        }
    }
}
