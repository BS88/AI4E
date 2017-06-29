using System.Threading.Tasks;

namespace AI4E.Modularity
{
    public interface IModule
    {
        Task ActivateAsync();

        Task DeactivateAsync();
    }
}
