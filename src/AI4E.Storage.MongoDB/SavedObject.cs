using System;

namespace AI4E.Storage.MongoDB
{
    internal sealed class SavedObject<T>
    {
        public Guid Id { get; set; }

        public T Data { get; set; }
    }
}
