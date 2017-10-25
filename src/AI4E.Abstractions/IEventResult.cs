using System.Collections.Immutable;

namespace AI4E.Integration
{
    public interface IEventResult
    {
        bool IsSuccess { get; }

        string Message { get; }
    }

    public interface IAggregateEventResult : IEventResult, IDispatchResult
    {
        ImmutableArray<IEventResult> EventResults { get; }
    }
}
