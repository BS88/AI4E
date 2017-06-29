using System;

namespace AI4E.Modularity.Integration
{
    public sealed class ActivateCommandForwarding
    {
        public ActivateCommandForwarding(Type commandType)
        {
            CommandType = commandType;
        }

        public Type CommandType { get; set; }
    }
}
