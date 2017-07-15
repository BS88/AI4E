using System;

namespace AI4E.Modularity.Integration
{
    public sealed class RegisterQueryForwarding
    {
        public RegisterQueryForwarding(Type queryType)
        {
            QueryType = queryType;
        }

        public Type QueryType { get; }
    }
}
