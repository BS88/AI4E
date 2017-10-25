using System;
using System.IO;
using System.Threading.Tasks;
using AI4E.Async;
using AI4E;
using AI4E.Modularity.Integration;
using Microsoft.Extensions.DependencyInjection;

namespace AI4E.Modularity
{
    // Manages the connection between the host infrastructure and the module
    public sealed class ModuleConnection : IAsyncInitialization, IAsyncCompletion
    {
        private readonly Stream _stream;
        private readonly IServiceProvider _serviceProvider;
        private IServiceProvider _sandboxedServiceProvider;
        private IMessageEndPoint _messageEndPoint;

        private readonly Task _initialization;
        private readonly TaskCompletionSource<object> _completionSource = new TaskCompletionSource<object>();
        private Task _completing;

        public ModuleConnection(Stream stream, IServiceProvider serviceProvider)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            _stream = stream;
            _serviceProvider = serviceProvider;

            CreateServices();

            _initialization = DoInitialization();
        }

        private void CreateServices()
        {
            var sandboxedServices = new ServiceCollection();

            sandboxedServices.AddSingleton(_serviceProvider.GetRequiredService<ICommandDispatcher>());
            sandboxedServices.AddSingleton(_serviceProvider.GetRequiredService<IQueryDispatcher>());
            sandboxedServices.AddSingleton(_serviceProvider.GetRequiredService<IEventDispatcher>());

            sandboxedServices.AddSingleton<IHostCommandDispatcher, HostCommandDispatcher>();
            sandboxedServices.AddSingleton<IHostQueryDispatcher, HostQueryDispatcher>();
            sandboxedServices.AddSingleton<IHostEventDispatcher, HostEventDispatcher>();

            sandboxedServices.AddSingleton<IMessageEndPoint>(p => new MessageEndPoint(_stream, p.GetRequiredService<IMessageSerializer>(), p));
            sandboxedServices.AddTransient<IMessageSerializer, MessageSerializer>();
            _sandboxedServiceProvider = sandboxedServices.BuildServiceProvider();
            _messageEndPoint = _sandboxedServiceProvider.GetRequiredService<IMessageEndPoint>();
        }

        #region Initialization

        public Task Initialization => _initialization;

        public Task Completion => _completionSource.Task;

        public void Complete()
        {
            if (_completing != null)
                return;

            
            _completing = DoCompletion();
        }

        private async Task DoInitialization()
        {
            await _messageEndPoint.RegisterAsync(new DefaultHandlerFactory<CommandMessageBroker>());
            await _messageEndPoint.RegisterAsync<RegisterCommandForwarding>(new DefaultHandlerFactory<CommandMessageBroker>());
            await _messageEndPoint.RegisterAsync<UnregisterCommandForwarding>(new DefaultHandlerFactory<CommandMessageBroker>());

            await _messageEndPoint.RegisterAsync(new DefaultHandlerFactory<QueryMessageBroker>());
            await _messageEndPoint.RegisterAsync<RegisterQueryForwarding>(new DefaultHandlerFactory<QueryMessageBroker>());
            await _messageEndPoint.RegisterAsync<UnregisterQueryForwarding>(new DefaultHandlerFactory<QueryMessageBroker>());

            await _messageEndPoint.RegisterAsync<RegisterEventForwarding>(new DefaultHandlerFactory<EventMessageBroker>());
            await _messageEndPoint.RegisterAsync<UnregisterEventForwarding>(new DefaultHandlerFactory<EventMessageBroker>());
            await _messageEndPoint.RegisterAsync<DispatchEvent>(new DefaultHandlerFactory<EventMessageBroker>());

            await _messageEndPoint.Initialization;

            var setupMessage = new SetupModule();

            await _messageEndPoint.SendAsync(setupMessage);
        }

        private async Task DoCompletion()
        {
            try
            {
                await Initialization;

                var tearDownMessage = new TearDownModule();

                await _messageEndPoint.SendAsync(tearDownMessage);

                _messageEndPoint.Complete();
                await _messageEndPoint.Completion;
            }
            catch (Exception exc)
            {
                _completionSource.TrySetException(exc);
            }

            _completionSource.TrySetResult(null);
        }

        #endregion
    }
}
