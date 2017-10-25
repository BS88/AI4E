using System;

namespace AI4E
{
    public interface IQueryResult : IDispatchResult
    {

    }

    public interface IQueryResult<TResult> : IQueryResult, IDispatchResult<TResult>
    {

    }
}
