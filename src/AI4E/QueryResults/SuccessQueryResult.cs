namespace AI4E.Integration.QueryResults
{
    public class SuccessQueryResult : IQueryResult
    {
        public static SuccessQueryResult Default => new SuccessQueryResult("Success");

        public SuccessQueryResult(string message)
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
        public SuccessQueryResult(TResult result, string message) : base(message)
        {
            Result = result;
        }

        public SuccessQueryResult(TResult result) : this(result, "Success") { }

        public TResult Result { get; }

        public override string ToString()
        {
            return $"{Message} [Result: {Result}]";
        }
    }
}
