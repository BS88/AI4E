using System;

namespace AI4E.Modularity.Integration
{
    public sealed class UnregisterEventForwarding
    {
        public UnregisterEventForwarding(Type eventType)
        {
            EventType = eventType;
        }

        public Type EventType { get; }
    }
}
