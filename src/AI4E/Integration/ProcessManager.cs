using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AI4E.Storage;

namespace AI4E.Integration
{
    [ProcessManager]
    public abstract class ProcessManager<TState>
    {
        [ProcessManagerState]
        public virtual TState State { get; internal set; }

        public bool IsTerminated { get; private set; }

        [NoEventHandlerAction]
        protected virtual void TerminateProcess()
        {
            IsTerminated = true;
        }

        [NoEventHandlerAction]
        protected virtual void AttachProcessManager(IProcessManagerAttachments<TState> attachments) { }

    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public class ProcessManagerAttribute : EventHandlerAttribute { }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public class NoProcessManagerAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ProcessManagerStateAttribute : Attribute
    {
        public Type StateType { get; set; }
    }

    public interface IProcessManagerAttachments<TState>
    {
        IProcessManagerAttachments<TState> Attach<TEvent>(Expression<Func<TEvent, TState, bool>> predicate);
    }

    public class ProcessManagerAttachment<TState> : IProcessManagerAttachments<TState> where TState : class
    {
        private readonly IQueryableDataStore _dataStore;
        private readonly Dictionary<Type, Func<object, Task<TState>>> _mappings = new Dictionary<Type, Func<object, Task<TState>>>();

        public ProcessManagerAttachment(IQueryableDataStore dataStore)
        {
            if (dataStore == null)
                throw new ArgumentNullException(nameof(dataStore));

            _dataStore = dataStore;
        }

        public IProcessManagerAttachments<TState> Attach<TEvent>(Expression<Func<TEvent, TState, bool>> predicate)
        {
            Func<object, Task<TState>> mapping = o =>
            {
                var evt = (TEvent)o;
                var dBPredicate = ParameterReplacer.Replace<Func<TEvent, TState, bool>, Func<TState, bool>>(predicate, predicate.Parameters.First(), Expression.Constant(evt));
                return _dataStore.QueryAsync<TState, TState>(queryable => queryable.Where(dBPredicate)).FirstOrDefault();
            };

            _mappings[typeof(TEvent)] = mapping;

            return this;
        }

        public Task<TState> GetStateAsync(object evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            if (_mappings.TryGetValue(evt.GetType(), out var mapping))
            {
                return mapping(evt);
            }

            return Task.FromResult<TState>(default);
        }
    }

    // Source: https://stackoverflow.com/questions/11159697/replace-parameter-in-lambda-expression
    public static class ParameterReplacer
    {
        // Produces an expression identical to 'expression'
        // except with 'source' parameter replaced with 'target' expression.     
        public static Expression<TOutput> Replace<TInput, TOutput>
                        (Expression<TInput> expression,
                        ParameterExpression source,
                        Expression target)
        {
            return new ParameterReplacerVisitor<TOutput>(source, target)
                        .VisitAndConvert(expression);
        }

        private class ParameterReplacerVisitor<TOutput> : ExpressionVisitor
        {
            private ParameterExpression _source;
            private Expression _target;

            public ParameterReplacerVisitor
                    (ParameterExpression source, Expression target)
            {
                _source = source;
                _target = target;
            }

            internal Expression<TOutput> VisitAndConvert<T>(Expression<T> root)
            {
                return (Expression<TOutput>)VisitLambda(root);
            }

            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                // Leave all parameters alone except the one we want to replace.
                var parameters = node.Parameters
                                     .Where(p => p != _source);

                return Expression.Lambda<TOutput>(Visit(node.Body), parameters);
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                // Replace the source with the target, visit other params as usual.
                return node == _source ? _target : base.VisitParameter(node);
            }
        }
    }
}
