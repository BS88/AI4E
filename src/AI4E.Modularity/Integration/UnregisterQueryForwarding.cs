using System;

namespace AI4E.Modularity.Integration
{
    public sealed class UnregisterQueryForwarding
    {
        public UnregisterQueryForwarding(Type queryType)
        {
            QueryType = queryType;
        }

        public Type QueryType { get; }
    }
}
