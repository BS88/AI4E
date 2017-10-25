namespace AI4E.EventResults
{
    public class SuccessEventResult : IEventResult
    {
        public static SuccessEventResult Default { get; } = new SuccessEventResult("Success");

        public SuccessEventResult(string message)
        {
            Message = message;
        }

        bool IEventResult.IsSuccess => true;

        public string Message { get; }

        public override string ToString()
        {
            return Message;
        }
    }
}
