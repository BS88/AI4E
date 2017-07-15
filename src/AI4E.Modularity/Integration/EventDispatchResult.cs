using AI4E.Integration;

namespace AI4E.Modularity.Integration
{
    public sealed class EventDispatchResult
    {
        public EventDispatchResult(IAggregateEventResult eventResult)
        {
            EventResult = eventResult;
        }

        public IAggregateEventResult EventResult { get; }
    }
}
