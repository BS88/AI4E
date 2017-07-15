using System;
using System.Diagnostics;

namespace AI4E.Integration.EventResults
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
            Debug.Assert(obj is SuccessEventResult);

            return Message == ((SuccessEventResult)obj).Message;
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode() ^ Message?.GetHashCode()??0;
        }

        bool IEquatable<IEventResult>.Equals(IEventResult other)
        {
            return Equals(other);
        }

        public static bool operator ==(SuccessEventResult left, SuccessEventResult right)
        {
            if (ReferenceEquals(left, null))
                return ReferenceEquals(right, null);

            if (ReferenceEquals(right, null))
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(SuccessEventResult left, SuccessEventResult right)
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
