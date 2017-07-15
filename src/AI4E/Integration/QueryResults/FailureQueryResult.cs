using System;
using System.Diagnostics;

namespace AI4E.Integration.QueryResults
{
    public class FailureQueryResult : IQueryResult
    {
        public static FailureQueryResult UnkownFailure { get; } = new FailureQueryResult("Unknown failure");

        public FailureQueryResult(string message)
        {
            Message = message;
        }

        bool IDispatchResult.IsSuccess => false;

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
            Debug.Assert(obj is FailureQueryResult);

            return Message == ((FailureQueryResult)obj).Message;
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode() ^ Message?.GetHashCode() ?? 0;
        }

        bool IEquatable<IDispatchResult>.Equals(IDispatchResult other)
        {
            return Equals(other);
        }

        bool IEquatable<IQueryResult>.Equals(IQueryResult other)
        {
            return Equals(other);
        }

        public static bool operator ==(FailureQueryResult left, FailureQueryResult right)
        {
            if (ReferenceEquals(left, null))
                return ReferenceEquals(right, null);

            if (ReferenceEquals(right, null))
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(FailureQueryResult left, FailureQueryResult right)
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
