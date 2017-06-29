using System;

namespace AI4E.Modularity.Integration
{
    public sealed class UnregisterCommandForwarding
    {
        public UnregisterCommandForwarding(Type commandType)
        {
            CommandType = commandType;
        }

        public Type CommandType { get; }
    }
}
