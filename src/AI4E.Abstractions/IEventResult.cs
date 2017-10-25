using System.Collections.Immutable;

namespace AI4E
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
