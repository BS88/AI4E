using System;
using System.IO;

namespace AI4E.Async
{
    public static class FileInfoExtensions
    {
        public static FileStream OpenReadAsync(this FileInfo fileInfo)
        {
            if (fileInfo == null)
                throw new ArgumentNullException(nameof(fileInfo));

            return new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        }
    }
}
