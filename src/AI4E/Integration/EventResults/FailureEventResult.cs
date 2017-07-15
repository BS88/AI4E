using System;
using System.Diagnostics;

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

        #region Equality

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;

            if (ReferenceEquals(obj, this))
                return true;

            if (GetType() != obj.GetType())
                return false;

            return IsEqualByValue(obj);
        }

        protected virtual bool IsEqualByValue(object obj)
        {
            Debug.Assert(obj is FailureEventResult);

            return Message == ((FailureEventResult)obj).Message;
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode() ^ Message?.GetHashCode()??0;
        }

        bool IEquatable<IEventResult>.Equals(IEventResult other)
        {
            return Equals(other);
        }

        public static bool operator ==(FailureEventResult left, FailureEventResult right)
        {
            if (ReferenceEquals(left, null))
                return ReferenceEquals(right, null);

            if (ReferenceEquals(right, null))
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(FailureEventResult left, FailureEventResult right)
        {
            if (ReferenceEquals(left, null))
                return !ReferenceEquals(right, null);

            if (ReferenceEquals(right, null))
                return true;

            return !left.Equals(right);
        }

        #endregion

        public override string ToString()
        {
            return Message;
        }
    }
}
