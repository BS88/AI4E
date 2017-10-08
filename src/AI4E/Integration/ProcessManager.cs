using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using AI4E.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace AI4E.Integration
{
    [ProcessManager]
    public abstract class ProcessManager<TState> : EventHandler where TState : class
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
        protected virtual void AttachProcess(IProcessAttachments<TState> attachments) { }

        [NoEventHandlerAction]
        protected virtual bool CanInitiateProcess<TEvent>(TEvent evt) { return false; }

        [NoEventHandlerAction]
        protected virtual TState CreateInitialState<TEvent>(TEvent initiatingEvent, Type stateType)
        {
            Debug.Assert(initiatingEvent != null);
            Debug.Assert(stateType != null);
            Debug.Assert(typeof(TState).IsAssignableFrom(stateType));

            var state = Activator.CreateInstance(stateType);

            Debug.Assert(state != null);

            return (TState)state;
        }
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

    public interface IProcessAttachments<TState> where TState : class
    {
        IProcessEventAttachment<TState, TEvent> Attach<TEvent>(Expression<Func<TEvent, TState, bool>> predicate);
    }

    public interface IProcessEventAttachment<TState, TEvent> : IProcessAttachments<TState>
        where TState : class
    {
        IProcessEventAttachment<TState, TEvent> CanStartProcess(Func<TEvent, TState> stateFactory);
    }

    public class ProcessManagerAttachment<TState> : IProcessAttachments<TState> where TState : class
    {
        private readonly IQueryableDataStore _dataStore;
        private readonly Dictionary<Type, Func<object, Task<TState>>> _mappings = new Dictionary<Type, Func<object, Task<TState>>>();

        public ProcessManagerAttachment(IQueryableDataStore dataStore)
        {
            if (dataStore == null)
                throw new ArgumentNullException(nameof(dataStore));

            _dataStore = dataStore;
        }

        public IProcessAttachments<TState> Attach<TEvent>(Expression<Func<TEvent, TState, bool>> predicate)
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

    public sealed class ProcessManagerEventProcessor : EventProcessor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDataStore _dataStore;

        private bool _created;
        private object _state;

        public ProcessManagerEventProcessor(IServiceProvider serviceProvider, IDataStore dataStore)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            if (dataStore == null)
                throw new ArgumentNullException(nameof(dataStore));

            _serviceProvider = serviceProvider;
            _dataStore = dataStore;
        }

        public override async Task<TEvent> PreProcessAsync<TEvent>(TEvent evt)
        {
            if (!IsProcessManager)
            {
                return evt;
            }

            var processManagerStateProperty = EventHandlerType.GetProperties().SingleOrDefault(p => p.CanWrite && p.IsDefined<ProcessManagerStateAttribute>());

            if (processManagerStateProperty != null)
            {
                var stateType = processManagerStateProperty.PropertyType;
                {
                    var customType = processManagerStateProperty.GetCustomAttribute<ProcessManagerStateAttribute>().StateType;

                    if (customType != null)
                    {
                        if (!stateType.IsAssignableFrom(customType))
                        {
                            throw new InvalidOperationException(); // TODO
                        }
                        stateType = customType;
                    }
                }

                var attachments = Activator.CreateInstance(typeof(ProcessManagerAttachment<>).MakeGenericType(stateType), _dataStore);

                Debug.Assert(attachments != null);

                var attachMethod = EventHandlerType.GetMethod("AttachProcess", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (attachMethod == null)
                {
                    throw new InvalidOperationException(); // TODO
                }

                attachMethod.Invoke(EventHandler, new[] { attachments });

                //EventHandler.AttachProcessManager(attachments);

                _state = (object)(await ((dynamic)attachments).GetStateAsync(evt));

                // TODO: Not all events are allowed to start a process.
                if (_state == null)
                {
                    var canInitMethodDef = EventHandlerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(p => p.Name == "CanInitiateProcess" && p.IsGenericMethodDefinition && p.GetGenericArguments().Length == 1);
                    var canInitMethod = canInitMethodDef?.MakeGenericMethod(typeof(TEvent));


                    if (canInitMethod != null && !(bool)canInitMethod.Invoke(EventHandler, new object[] { evt })) //!(bool)(EventHandler.CanInitiateProcess(evt)))
                    {
                        return evt;

                        // return new FailureEventResult(""); // TODO: Maybe the events are out of order? What to do about that?
                    }

                    // Create state

                    var createInitStateMethodDef = EventHandlerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(p => p.Name == "CreateInitialState" && p.IsGenericMethodDefinition && p.GetGenericArguments().Length == 1);
                    var createInitStateMethod = createInitStateMethodDef?.MakeGenericMethod(typeof(TEvent));

                    _state = createInitStateMethod?.Invoke(EventHandler, new object[] { evt, stateType }); //(object)(EventHandler.CreateInitialState(evt, stateType));
                    _created = true;
                }

                processManagerStateProperty.SetValue(EventHandler, _state);
            }

            return evt;
        }

        private object EventHandler => Context.EventHandler;

        private bool IsProcessManager
        {
            get
            {
                return (EventHandlerType.IsClass || EventHandlerType.IsValueType && !EventHandlerType.IsEnum) &&
                       !EventHandlerType.IsAbstract &&
                       EventHandlerType.IsPublic &&
                       !EventHandlerType.ContainsGenericParameters &&
                       !EventHandlerType.IsDefined<NoProcessManagerAttribute>() &&
                       (EventHandlerType.Name.EndsWith("ProcessManager", StringComparison.OrdinalIgnoreCase) || EventHandlerType.IsDefined<ProcessManagerAttribute>());
            }
        }

        private Type EventHandlerType => Context.EventHandler.GetType();

        public override async Task<TEventResult> PostProcessAsync<TEventResult>(TEventResult eventResult)
        {
            if (!IsProcessManager)
            {
                return eventResult;
            }

            var isTerminatedProperty = EventHandlerType.GetProperty("IsTerminated", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            var terminated = isTerminatedProperty != null &&
                             isTerminatedProperty.CanRead &&
                             isTerminatedProperty.PropertyType == typeof(bool) &&
                             isTerminatedProperty.GetIndexParameters().Length == 0 &&
                             (bool)isTerminatedProperty.GetValue(EventHandler);//(bool)EventHandler.IsTerminated;

            if (_created && !terminated)
            {
                ((dynamic)_dataStore).Add(_state);
            }
            else if (!_created && terminated)
            {
                ((dynamic)_dataStore).Remove(_state);
            }
            else if (!_created && !terminated)
            {
                ((dynamic)_dataStore).Update(_state);
            }
            // else if (_created && terminated)
            // {
            //     Nothing to store, because data is not yet in the db and shall not be in the db from now on.
            // }

            await _dataStore.SaveChangesAsync();
            return eventResult;
        }
    }
}
