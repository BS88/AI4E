using System;
using System.Diagnostics;

namespace AI4E.Integration
{
    [ProcessManager]
    public abstract class ProcessManager<TState> : EventHandler where TState : class, new()
    {
        [ProcessManagerState]
        public TState State { get; internal set; }

        [ProcessManagerTerminated]
        public bool IsTerminated { get; private set; }

        [NoEventHandlerAction]
        protected void TerminateProcess()
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

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ProcessManagerTerminatedAttribute : Attribute { }
}
