using System;
using System.Diagnostics;

namespace AI4E.Integration.QueryResults
{
    // TODO: Allow SuccessQueryResult instances without result value?
    public abstract class SuccessQueryResult : IQueryResult
    {
        protected SuccessQueryResult(string message)
        {
            Message = message;
        }

        bool IDispatchResult.IsSuccess => true;

        public string Message { get; }

        public override string ToString()
        {
            return Message;
        }
    }

    public class SuccessQueryResult<TResult> : SuccessQueryResult, IQueryResult<TResult>
    {
        public SuccessQueryResult(string message, TResult result) : base(message)
        {
            Result = result;
        }

        public SuccessQueryResult(TResult result) : this("Success", result) { }

        public TResult Result { get; }
    }
}
