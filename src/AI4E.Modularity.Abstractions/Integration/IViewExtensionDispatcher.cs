using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace AI4E.Modularity.Integration
{
    public interface IViewExtensionDispatcher
    {
        Task<IHandlerRegistration<IViewExtensionRenderer<TViewExtension>>> RegisterAsync<TViewExtension>(IContextualProvider<IViewExtensionRenderer<TViewExtension>> handlerFactory);

        Task<ImmutableArray<string>> RenderAsync<TViewExtension>(TViewExtension viewExtension);

        IViewExtensionDispatcher<TViewExtension> GetTypedDispatcher<TViewExtension>();
    }

    public interface IViewExtensionDispatcher<TViewExtension>
    {
        Task<IHandlerRegistration<IViewExtensionRenderer<TViewExtension>>> RegisterAsync(IContextualProvider<IViewExtensionRenderer<TViewExtension>> handlerFactory);

        Task<ImmutableArray<string>> RenderAsync(TViewExtension viewExtension);
    }

    public interface INonGenericViewExtensionDispatcher
    {
        Task<ImmutableArray<string>> RenderAsync(object viewExtension);

        ITypedNonGenericViewExtensionDispatcher GetTypedDispatcher();
    }

    public interface ITypedNonGenericViewExtensionDispatcher
    {
        Type ViewExtensionType { get; }

        Task<ImmutableArray<string>> RenderAsync(object viewExtension);
    }
}
