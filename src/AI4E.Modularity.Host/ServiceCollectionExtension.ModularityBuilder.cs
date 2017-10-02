using Microsoft.Extensions.DependencyInjection;

namespace AI4E.Modularity
{
    public static partial class ServiceCollectionExtension
    {
        private sealed class ModularityBuilder : IModularityBuilder
        {
            public ModularityBuilder(IServiceCollection services)
            {
                Services = services;
            }

            public IServiceCollection Services { get; }
        }
    }
}
