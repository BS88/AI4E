using System.Threading.Tasks;

namespace AI4E.Modularity.Integration
{
    public interface IViewExtensionRenderer<TViewExtension>
    {
        Task<string> RenderViewExtensionAsync(TViewExtension viewExtension);
    }
}
