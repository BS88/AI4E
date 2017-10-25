﻿using System.Threading.Tasks;
using AI4E;

namespace AI4E.Modularity.Integration
{
    public interface ICommandMessageTranslator
    {
        Task RegisterForwardingAsync<TCommand>();
        Task UnregisterForwardingAsync<TCommand>();
        Task<ICommandResult> DispatchAsync<TCommand>(TCommand command);
    }
}
