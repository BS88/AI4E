using System;
using System.Collections.Immutable;

namespace AI4E.Integration
{
    public interface IEventResult : IEquatable<IEventResult>
    {
        bool IsSuccess { get; }

        string Message { get; }
    }

    public interface IEventAggregateResult : IDispatchResult, IEquatable<IEventAggregateResult>
    {
        ImmutableArray<IEventResult> EventResults { get; }
    }
}
