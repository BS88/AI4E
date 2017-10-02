using Microsoft.Extensions.DependencyInjection;

namespace AI4E.Modularity
{
    public interface IModularityBuilder
    {
        IServiceCollection Services { get; }
    }
}
