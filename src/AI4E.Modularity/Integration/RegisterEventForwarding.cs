using System;

namespace AI4E.Modularity.Integration
{
    public sealed class RegisterEventForwarding
    {
        public RegisterEventForwarding(Type eventType)
        {
            EventType = eventType;
        }

        public Type EventType { get; }
    }
}
