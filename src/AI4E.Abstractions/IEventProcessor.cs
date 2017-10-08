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

        public abstract Task<TEvent> PreProcessAsync<TEvent>(TEvent evt);
        public abstract Task<TEventResult> PostProcessAsync<TEventResult>(TEventResult eventResult) where TEventResult : IEventResult;
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
