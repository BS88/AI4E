using System;

namespace AI4E.Modularity.Integration
{
    public sealed class DispatchEvent
    {
        public DispatchEvent(Type eventType, object @event)
        {
            EventType = eventType;
            Event = @event;
        }

        public Type EventType { get; }
        public object Event { get; }
    }
}
