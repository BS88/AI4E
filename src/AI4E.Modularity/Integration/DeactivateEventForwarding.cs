using System;

namespace AI4E.Modularity.Integration
{
    public sealed class DeactivateEventForwarding
    {
        public DeactivateEventForwarding(Type eventType)
        {
            EventType = eventType;
        }

        public Type EventType { get; }
    }
}
