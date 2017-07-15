using System;

namespace AI4E.Integration
{
    public interface IQueryResult : IDispatchResult
    {

    }

    public interface IQueryResult<TResult> : IQueryResult, IDispatchResult<TResult>
    {

    }
}
