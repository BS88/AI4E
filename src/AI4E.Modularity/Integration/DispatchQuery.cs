using System;

namespace AI4E.Modularity.Integration
{
    public sealed class DispatchQuery
    {
        public DispatchQuery(Type queryType, Type resultType, object query)
        {
            QueryType = queryType;
            ResultType = resultType;
            Query = query;
        }

        public Type QueryType { get; }
        public Type ResultType { get; }
        public object Query { get; }
    }
}
