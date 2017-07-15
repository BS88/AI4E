using System;

namespace AI4E.Modularity.Integration
{
    public sealed class ActivateQueryForwarding
    {
        public ActivateQueryForwarding(Type queryType)
        {
            QueryType = queryType;
        }

        public Type QueryType { get; }
    }
}
