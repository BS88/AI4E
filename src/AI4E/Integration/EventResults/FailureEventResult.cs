using System;

namespace AI4E.Integration.EventResults
{
    public class FailureEventResult : IEventResult
    {
        public static FailureEventResult Unknown { get; } = new FailureEventResult("Unkown failure");

        public FailureEventResult(string message)
        {
            Message = message;
        }

        bool IEventResult.IsSuccess => throw new NotImplementedException();

        public string Message { get; }

        public override string ToString()
        {
            return Message;
        }
    }
}
