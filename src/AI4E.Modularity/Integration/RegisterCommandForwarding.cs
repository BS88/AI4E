using System;

namespace AI4E.Modularity.Integration
{
    public sealed class RegisterCommandForwarding
    {
        public RegisterCommandForwarding(Type commandType)
        {
            CommandType = commandType;
        }

        public Type CommandType { get; }
    }
}
