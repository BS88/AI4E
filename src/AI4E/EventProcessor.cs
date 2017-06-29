using System.Collections.Generic;
using System.Linq;

namespace AI4E
{
    public sealed class EventProcessor<TEventBase, TEntityBase> : IEventProcessor<TEventBase, TEntityBase>
    {
        private readonly List<(TEventBase evt, TEntityBase entity)> _uncommittedEvents = new List<(TEventBase, TEntityBase)>();
        private readonly Dictionary<TEntityBase, List<TEventBase>> _uncommittedEntities = new Dictionary<TEntityBase, List<TEventBase>>();

        // Instances should be scoped

        public EventProcessor()
        {

        }

        public IEnumerable<TEventBase> GetUncommittedEvents(TEntityBase entity)
        {
            if (!_uncommittedEntities.TryGetValue(entity, out var events))
                return Enumerable.Empty<TEventBase>();

            return events;
        }

        public IEnumerable<TEntityBase> UpdatedEntities => _uncommittedEntities.Keys;

        public IEnumerable<TEventBase> UncommittedEvents => _uncommittedEvents.Select(p => p.evt);

        public void RegisterEvent<TEvent>(TEntityBase entity, TEvent evt) where TEvent : TEventBase
        {
            _uncommittedEvents.Add((evt, entity));

            if (!_uncommittedEntities.TryGetValue(entity, out var events))
            {
                events = new List<TEventBase>();
                _uncommittedEntities.Add(entity, events);
            }

            events.Add(evt);
        }

        public void Commit()
        {
            _uncommittedEvents.Clear();
            _uncommittedEntities.Clear();
        }

        public void Commit(TEntityBase entity)
        {
            _uncommittedEntities.Remove(entity);
            foreach (var p in _uncommittedEvents.Where(p => p.entity.Equals(entity)).ToArray())
            {
                _uncommittedEvents.Remove(p);
            }
        }
    }
}
