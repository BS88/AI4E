using System;
using Microsoft.Extensions.DependencyInjection;

namespace AI4E
{
    public sealed class DefaultHandlerFactory<THandler> : IHandlerProvider<THandler>
    {
        private readonly THandler _singletonHandler;
        private readonly bool _useSingletonHandler;

        [Obsolete] // TODO: Create a separate type 
        public DefaultHandlerFactory(THandler singletonHandler)
        {
            _singletonHandler = singletonHandler;
            _useSingletonHandler = true;
        }

        public DefaultHandlerFactory()
        {
            _useSingletonHandler = false;
        }

        public THandler GetHandler(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            if (_useSingletonHandler)
            {
                return _singletonHandler;
            }

            // Create a new instance of the handler type.
            return ActivatorUtilities.CreateInstance<THandler>(serviceProvider);
        }
    }
}
