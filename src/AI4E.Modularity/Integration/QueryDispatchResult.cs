using AI4E.Integration;

namespace AI4E.Modularity.Integration
{
    public sealed class QueryDispatchResult
    {
        public QueryDispatchResult(IQueryResult queryResult)
        {
            QueryResult = queryResult;
        }

        public IQueryResult QueryResult { get; }
    }
}
