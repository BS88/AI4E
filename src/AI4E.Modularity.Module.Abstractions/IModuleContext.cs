﻿using System;
using AI4E;

namespace AI4E.Modularity
{
    public interface IModuleContext
    {
        ICommandDispatcher CommandDispatcher { get; }

        IQueryDispatcher QueryDispatcher { get; }

        IEventDispatcher EventDispatcher { get; }

        IServiceProvider ModuleServices { get; }
    }
}
