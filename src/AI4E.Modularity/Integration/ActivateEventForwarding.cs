using System;

namespace AI4E.Modularity.Integration
{
    public sealed class ActivateEventForwarding
    {
        public ActivateEventForwarding(Type eventType)
        {
            EventType = eventType;
        }

        public Type EventType { get; }
    }
}
