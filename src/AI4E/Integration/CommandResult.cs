using System;
using System.Collections.Generic;
using AI4E.Integration.CommandResults;

namespace AI4E.Integration
{
    public static class CommandResult
    {
        private static readonly ValidationFailureCommandResult _validationFailure = new ValidationFailureCommandResult();
        private static readonly UnauthorizedCommandResult _unauthorized = new UnauthorizedCommandResult();
        private static readonly UnauthenticatedCommandResult _unauthenticated = new UnauthenticatedCommandResult();
        private static readonly ConcurrencyIssueCommandResult _concurrencyIssue = new ConcurrencyIssueCommandResult();

        public static SuccessCommandResult Success()
        {
            return SuccessCommandResult.Default;
        }

        public static SuccessCommandResult Success(string message)
        {
            return new SuccessCommandResult(message);
        }

        public static FailureCommandResult Failure()
        {
            return FailureCommandResult.UnknownFailure;
        }

        public static FailureCommandResult Failure(string message)
        {
            return new FailureCommandResult(message);
        }

        public static ValidationFailureCommandResult ValidationFailure()
        {
            return _validationFailure;
        }

        public static ValidationFailureCommandResult ValidationFailure(IEnumerable<ValidationResult> validationResults)
        {
            return new ValidationFailureCommandResult(validationResults);
        }

        public static UnauthorizedCommandResult Unauthorized()
        {
            return _unauthorized;
        }

        public static UnauthenticatedCommandResult Unauthenticated()
        {
            return _unauthenticated;
        }

        public static ConcurrencyIssueCommandResult ConcurrencyIssue()
        {
            return _concurrencyIssue;
        }

        public static CommandDispatchFailureCommandResult DispatchFailure(Type commandType)
        {
            return new CommandDispatchFailureCommandResult(commandType);
        }

        public static CommandDispatchFailureCommandResult DispatchFailure<TCommand>()
        {
            return new CommandDispatchFailureCommandResult(typeof(TCommand));
        }

        public static EntityNotFoundCommandResult EntityNotFound(Type entityType, Guid id)
        {
            return new EntityNotFoundCommandResult(entityType, id);
        }

        public static EntityNotFoundCommandResult EntityNotFound<TEntity>(Guid id)
        {
            return new EntityNotFoundCommandResult(typeof(TEntity), id);
        }
    }
}
