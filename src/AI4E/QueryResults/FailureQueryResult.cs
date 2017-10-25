namespace AI4E.Integration.QueryResults
{
    public class FailureQueryResult : IQueryResult
    {
        public static FailureQueryResult UnknownFailure { get; } = new FailureQueryResult("Unknown failure");

        public FailureQueryResult(string message)
        {
            Message = message;
        }

        bool IDispatchResult.IsSuccess => false;

        public string Message { get; }

        public override string ToString()
        {
            return Message;
        }
    }
}
