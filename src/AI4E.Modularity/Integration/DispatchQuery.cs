using System;

namespace AI4E.Modularity.Integration
{
    public sealed class DispatchQuery
    {
        public DispatchQuery(Type queryType, object query)
        {
            QueryType = queryType;
            Query = query;
        }

        public Type QueryType { get; }
        public object Query { get; }
    }
}
