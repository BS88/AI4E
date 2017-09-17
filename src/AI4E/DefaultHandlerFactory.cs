using System;
using Microsoft.Extensions.DependencyInjection;

namespace AI4E
{
    [Obsolete("Use ContextualProvider<T>")]
    public sealed class DefaultHandlerFactory<THandler> : IContextualProvider<THandler>
    {
        private readonly THandler _singletonHandler;
        private readonly bool _useSingletonHandler;


        public DefaultHandlerFactory(THandler singletonHandler)
        {
            _singletonHandler = singletonHandler;
            _useSingletonHandler = true;
        }

        public DefaultHandlerFactory()
        {
            _useSingletonHandler = false;
        }

        public THandler ProvideInstance(IServiceProvider serviceProvider)
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

    public sealed class ContextualProvider<T> : IContextualProvider<T>
    {
        private readonly Func<IServiceProvider, T> _factory;

        public ContextualProvider(Func<IServiceProvider, T> factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            _factory = factory;
        }

        private static Lazy<ContextualProvider<T>> _fromContext = new Lazy<ContextualProvider<T>>(
            () => new ContextualProvider<T>(provider => ActivatorUtilities.CreateInstance<T>(provider)),
            isThreadSafe: true);

        public static ContextualProvider<T> FromContext => _fromContext.Value;

        public static ContextualProvider<T> FromValue(T value)
        {
            return new ContextualProvider<T>(provider => value);
        }

        public T ProvideInstance(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            return _factory(serviceProvider);
        }

        public static implicit operator ContextualProvider<T>(Func<IServiceProvider, T> factory)
        {
            return new ContextualProvider<T>(factory);
        }
    }

    public static class ContextualProvider
    {
        public static ContextualProvider<T> FromValue<T>(T value)
        {
            return new ContextualProvider<T>(provider => value);
        }
    }
}
