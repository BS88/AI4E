﻿using System;
using System.Collections.Generic;

namespace AI4E.Integration
{
    public static class CommandResult
    {
        private static readonly SuccessCommandResult _success = new SuccessCommandResult();
        private static readonly FailureCommandResult _failure = new FailureCommandResult();
        private static readonly ValidationFailureCommandResult _validationFailure = new ValidationFailureCommandResult();
        private static readonly UnauthorizedCommandResult _unauthorized = new UnauthorizedCommandResult();
        private static readonly UnauthenticatedCommandResult _unauthenticated = new UnauthenticatedCommandResult();
        private static readonly ConcurrencyIssueCommandResult _concurrencyIssue = new ConcurrencyIssueCommandResult();

        public static SuccessCommandResult Success()
        {
            return _success;
        }

        public static SuccessCommandResult Success(string message)
        {
            return new SuccessCommandResult(message);
        }

        public static FailureCommandResult Failure()
        {
            return _failure;
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

        public static EntityNotFoundCommandResult EntityNotFound(string entityType, Guid id)
        {
            return new EntityNotFoundCommandResult(entityType, id);
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

    public class EntityNotFoundCommandResult : FailureCommandResult
    {
        public EntityNotFoundCommandResult(string entityType, Guid id) : base($"The entity '{entityType}' with the id '{id}' was not found.")
        {
            EntityType = entityType;
            Id = id;
        }

        public EntityNotFoundCommandResult(Type entityType, Guid id) : this(entityType.FullName, id) { }

        public string EntityType { get; }

        public Guid Id { get; }
    }
}
