namespace AI4E.Modularity.Integration
{
    public sealed class QueryDispatchResult
    {
        public QueryDispatchResult(object queryResult)
        {
            QueryResult = queryResult;
        }

        public object QueryResult { get; }
    }
}
