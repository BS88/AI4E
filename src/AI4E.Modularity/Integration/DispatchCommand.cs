using System;

namespace AI4E.Modularity.Integration
{
    public sealed class DispatchCommand
    {
        public DispatchCommand(Type commandType, object command)
        {
            CommandType = commandType;
            Command = command;
        }

        public Type CommandType { get; }

        public object Command { get; }
    }
}
