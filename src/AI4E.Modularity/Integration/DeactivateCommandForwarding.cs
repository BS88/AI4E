using System;

namespace AI4E.Modularity.Integration
{
    public sealed class DeactivateCommandForwarding
    {
        public DeactivateCommandForwarding(Type commandType)
        {
            CommandType = commandType;
        }

        public Type CommandType { get; }
    }
}
