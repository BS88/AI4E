using System;
using System.Threading.Tasks;

namespace AI4E
{
    public interface IEventProcessor
    {
        Task<TEvent> PreProcessAsync<TEvent>(TEvent evt);

        Task<TEventResult> PostProcessAsync<TEventResult>(TEventResult eventResult)
            where TEventResult : IEventResult;
    }

    public abstract class EventProcessor : IEventProcessor
    {
        [EventProcessorContext]
        protected internal IEventProcessorContext Context { get; internal set; }

        public virtual Task<TEvent> PreProcessAsync<TEvent>(TEvent evt)
        {
            return Task.FromResult(evt);
        }

        public virtual Task<TEventResult> PostProcessAsync<TEventResult>(TEventResult eventResult) where TEventResult : IEventResult
        {
            return Task.FromResult(eventResult);
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class EventProcessorContextAttribute : Attribute
    {
        public EventProcessorContextAttribute() { }
    }

    public interface IEventProcessorContext
    {
        object EventHandler { get; }
        Type EventType { get; }
    }
}
