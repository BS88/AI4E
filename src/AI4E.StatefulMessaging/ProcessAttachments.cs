using AI4E.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AI4E.Integration
{
    internal class ProcessAttachments<TState> : IProcessAttachments<TState> where TState : class
    {
        private readonly IQueryableDataStore _dataStore;
        private readonly Dictionary<Type, object> _attachments = new Dictionary<Type, object>();

        public ProcessAttachments(IQueryableDataStore dataStore)
        {
            if (dataStore == null)
                throw new ArgumentNullException(nameof(dataStore));

            _dataStore = dataStore;
        }

        public IProcessAttachment<TState, TEvent> Attach<TEvent>(Expression<Func<TEvent, TState, bool>> predicate)
        {
            var key = typeof(TEvent);

            if (_attachments.ContainsKey(key))
            {
                throw new InvalidOperationException($"Cannot attach to event-type '{key.FullName}' because for this type of event an attachment is already present.");
            }

            var attachment = new ProcessAttachment<TState, TEvent>(_dataStore);
            attachment.Attach(predicate);

            _attachments.Add(key, attachment);

            return attachment;
        }

        //public Task<TState> GetStateAsync(object evt)
        //{
        //    if (evt == null)
        //        throw new ArgumentNullException(nameof(evt));

        //    if (_mappings.TryGetValue(evt.GetType(), out var mapping))
        //    {
        //        return mapping(evt);
        //    }

        //    return Task.FromResult<TState>(default);
        //}
    }

    internal class ProcessAttachment<TState, TEvent> : IProcessAttachment<TState, TEvent>
        where TState : class
    {
        private readonly IQueryableDataStore _dataStore;
        private Func<object, Task<TState>> _mapping;
        private bool _canStartProcess;
        private Func<TEvent, TState> _stateFactory;

        public ProcessAttachment(IQueryableDataStore dataStore)
        {
            if (dataStore == null)
                throw new ArgumentNullException(nameof(dataStore));

            _dataStore = dataStore;
        }

        public IProcessAttachment<TState, TEvent> Attach(Expression<Func<TEvent, TState, bool>> predicate)
        {
            Func<object, Task<TState>> mapping = o =>
            {
                var evt = (TEvent)o;
                var dBPredicate = ParameterReplacer.Replace<Func<TEvent, TState, bool>, Func<TState, bool>>(predicate, predicate.Parameters.First(), Expression.Constant(evt));
                return _dataStore.QueryAsync<TState, TState>(queryable => queryable.Where(dBPredicate)).FirstOrDefault();
            };

            _mapping = mapping;

            return this;
        }

        public IProcessAttachment<TState, TEvent> CanStartProcess(Func<TEvent, TState> stateFactory)
        {
            if (stateFactory == null)
                throw new ArgumentNullException(nameof(stateFactory));

            _canStartProcess = true;
            _stateFactory = stateFactory;

            return this;
        }
    }
}
