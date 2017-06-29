using System;
using System.Net;
using Microsoft.Extensions.DependencyInjection;

namespace AI4E.Modularity
{
    public sealed class ModuleBuilder<TModule> where TModule : IModule
    {
        private int _port;

        public ModuleBuilder<TModule> UsePort(int port)
        {
            if (IPEndPoint.MinPort > port || IPEndPoint.MaxPort < port)
                throw new ArgumentOutOfRangeException(nameof(port));

            _port = port;

            return this;
        }

        public IModuleRunner Build(IServiceCollection services)
        {
            return new ModuleRunner(typeof(TModule), _port, services);
        }
    }
}
