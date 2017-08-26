namespace AI4E.Integration.QueryResults
{
    public class NotFoundQueryResult : FailureQueryResult
    {
        public NotFoundQueryResult(string message) : base(message) { }

        public NotFoundQueryResult() : base("Not found") { }
    }
}
