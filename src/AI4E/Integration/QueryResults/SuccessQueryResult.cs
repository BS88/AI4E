using System;
using System.Diagnostics;

namespace AI4E.Integration.QueryResults
{
    // TODO: Allow SuccessQueryResult instances without result value?
    public abstract class SuccessQueryResult : IQueryResult
    {
        protected SuccessQueryResult(string message)
        {
            Message = message;
        }

        bool IDispatchResult.IsSuccess => true;

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

        protected abstract bool IsEqualByValue(object obj);

        public abstract override int GetHashCode();

        bool IEquatable<IDispatchResult>.Equals(IDispatchResult other)
        {
            return Equals(other);
        }

        bool IEquatable<IQueryResult>.Equals(IQueryResult other)
        {
            return Equals(other);
        }

        public static bool operator ==(SuccessQueryResult left, SuccessQueryResult right)
        {
            if (ReferenceEquals(left, null))
                return ReferenceEquals(right, null);

            if (ReferenceEquals(right, null))
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(SuccessQueryResult left, SuccessQueryResult right)
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

    public class SuccessQueryResult<TResult> : SuccessQueryResult, IQueryResult<TResult>
    {
        public SuccessQueryResult(string message, TResult result) : base(message)
        {
            Result = result;
        }

        public SuccessQueryResult(TResult result) : this("Success", result) { }

        public TResult Result { get; }

        protected override bool IsEqualByValue(object obj)
        {
            Debug.Assert(obj is SuccessQueryResult<TResult>);

            var other = (SuccessQueryResult<TResult>)obj;

            return Message == other.Message &&
                  (ReferenceEquals(Result, null) && ReferenceEquals(other.Result, null) ||
                   !ReferenceEquals(Result, null) && Result.Equals(other.Result));
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode() ^ Result?.GetHashCode() ?? 0 ^ Message?.GetHashCode() ?? 0;
        }

        bool IEquatable<IDispatchResult<TResult>>.Equals(IDispatchResult<TResult> other)
        {
            return Equals(other);
        }

        bool IEquatable<IQueryResult<TResult>>.Equals(IQueryResult<TResult> other)
        {
            return Equals(other);
        }
    }
}
