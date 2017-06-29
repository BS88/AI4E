using AI4E.Integration;

namespace AI4E.Modularity.Integration
{
    public sealed class CommandDispatchResult
    {
        public CommandDispatchResult(ICommandResult commandResult)
        {
            CommandResult = commandResult;
        }

        public ICommandResult CommandResult { get; }
    }
}
