using AI4E;

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
