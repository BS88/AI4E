using System;

namespace AI4E.EventResults
{
    public class FailureEventResult : IEventResult
    {
        public static FailureEventResult UnknownFailure { get; } = new FailureEventResult("Unkown failure");

        public FailureEventResult(string message)
        {
            Message = message;
        }

        bool IEventResult.IsSuccess => false;

        public string Message { get; }

        public override string ToString()
        {
            return Message;
        }
    }
}
