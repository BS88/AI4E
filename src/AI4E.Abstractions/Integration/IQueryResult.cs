using System;

namespace AI4E.Integration
{
    public interface IQueryResult : IDispatchResult, IEquatable<IQueryResult>
    {

    }

    public interface IQueryResult<TResult> : IQueryResult, IDispatchResult<TResult>, IEquatable<IQueryResult<TResult>>
    {

    }
}
