using System;
using System.Linq.Expressions;

namespace AI4E.Integration
{
    public interface IProcessAttachments<TState>
        where TState : class
    {
        IProcessAttachment<TState, TEvent> Attach<TEvent>(Expression<Func<TEvent, TState, bool>> predicate);
    }

    public interface IProcessAttachment<TState, TEvent>
        where TState : class
    {
        IProcessAttachment<TState, TEvent> CanStartProcess(Func<TEvent, TState> stateFactory);
    }
}
