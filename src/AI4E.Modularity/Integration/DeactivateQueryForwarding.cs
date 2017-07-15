using System;

namespace AI4E.Modularity.Integration
{
    public sealed class DeactivateQueryForwarding
    {
        public DeactivateQueryForwarding(Type queryType)
        {
            QueryType = queryType;
        }

        public Type QueryType { get; }
    }
}
