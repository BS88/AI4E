﻿using System.Threading.Tasks;
using AI4E.Integration;

namespace AI4E.Modularity.Integration
{
    public interface IQueryMessageTranslator
    {
        Task RegisterForwardingAsync<TQuery>();
        Task UnregisterForwardingAsync<TQuery>();

        Task<IQueryResult> DispatchAsync<TQuery>(TQuery query);
    }
}
