using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using AI4E.Integration;
using AI4E.Modularity.Integration;
using Microsoft.Extensions.DependencyInjection;

namespace AI4E.Modularity
{
    internal sealed class ModuleRunner : IModuleRunner
    {
        private readonly IServiceCollection _services;
        private readonly int _port = 0;
        private readonly Type _moduleType;

        private Task _runner;

        public ModuleRunner(Type moduleType, int port, IServiceCollection services)
        {
            if (moduleType == null)
                throw new ArgumentNullException(nameof(moduleType));

            if (services == null)
                throw new ArgumentNullException(nameof(services));

            _services = services;
            _moduleType = moduleType;
            _port = port;

        }

        public void Run()
        {
            if (_runner != null)
                return;

            _runner = RunAsync();
        }

        private async Task RunAsync()
        {
            _services.AddSingleton(typeof(IModule), _moduleType);
            _services.AddSingleton<RemoteCommandDispatcher>();
            _services.AddSingleton<IRemoteCommandDispatcher>(p => p.GetRequiredService<RemoteCommandDispatcher>());
            _services.AddSingleton<ICommandDispatcher>(p => p.GetRequiredService<RemoteCommandDispatcher>());
            _services.AddSingleton<INonGenericRemoteCommandDispatcher>(p => p.GetRequiredService<RemoteCommandDispatcher>());
            _services.AddSingleton<INonGenericCommandDispatcher>(p => p.GetRequiredService<RemoteCommandDispatcher>());
            _services.AddSingleton<ICommandMessageTranslator, CommandMessageTranslator>();

            _services.AddSingleton<RemoteQueryDispatcher>();
            _services.AddSingleton<IRemoteQueryDispatcher>(p => p.GetRequiredService<RemoteQueryDispatcher>());
            _services.AddSingleton<IQueryDispatcher>(p => p.GetRequiredService<RemoteQueryDispatcher>());
            _services.AddSingleton<INonGenericRemoteQueryDispatcher>(p => p.GetRequiredService<RemoteQueryDispatcher>());
            _services.AddSingleton<INonGenericQueryDispatcher>(p => p.GetRequiredService<RemoteQueryDispatcher>());
            _services.AddSingleton<IQueryMessageTranslator, QueryMessageTranslator>();

            _services.AddSingleton<RemoteEventDispatcher>();
            _services.AddSingleton<IRemoteEventDispatcher>(p => p.GetRequiredService<RemoteEventDispatcher>());
            _services.AddSingleton<IEventDispatcher>(p => p.GetRequiredService<RemoteEventDispatcher>());
            _services.AddSingleton<INonGenericRemoteEventDispatcher>(p => p.GetRequiredService<RemoteEventDispatcher>());
            _services.AddSingleton<INonGenericEventDispatcher>(p => p.GetRequiredService<RemoteEventDispatcher>());
            _services.AddSingleton<IEventMessageTranslator, EventMessageTranslator>();

            var client = new TcpClient();
            client.Connect(IPAddress.Loopback, _port);

            _services.AddSingleton<IMessageEndPoint>(provider =>
            {
                var endPoint = new MessageEndPoint(client.GetStream(), provider.GetRequiredService<ISerializer>(), provider);

                endPoint.RegisterAsync(new DefaultHandlerFactory<CommandMessageBroker>());
                endPoint.RegisterAsync<ActivateCommandForwarding>(new DefaultHandlerFactory<CommandMessageBroker>());
                endPoint.RegisterAsync<DeactivateCommandForwarding>(new DefaultHandlerFactory<CommandMessageBroker>());

                endPoint.RegisterAsync(new DefaultHandlerFactory<QueryMessageBroker>());
                endPoint.RegisterAsync<ActivateQueryForwarding>(new DefaultHandlerFactory<QueryMessageBroker>());
                endPoint.RegisterAsync<DeactivateQueryForwarding>(new DefaultHandlerFactory<QueryMessageBroker>());

                endPoint.RegisterAsync<DispatchEvent>(new DefaultHandlerFactory<EventMessageBroker>());
                endPoint.RegisterAsync<ActivateEventForwarding>(new DefaultHandlerFactory<EventMessageBroker>());
                endPoint.RegisterAsync<DeactivateEventForwarding>(new DefaultHandlerFactory<EventMessageBroker>());

                endPoint.RegisterAsync<SetupModule>(new DefaultHandlerFactory<ModuleHandler>());
                endPoint.RegisterAsync<TearDownModule>(new DefaultHandlerFactory<ModuleHandler>());

                return endPoint;
            });

            _services.AddTransient<ISerializer, Serializer>();

            //var serviceProvider = _services.BuildServiceProvider();
            //var messageEndPoint = serviceProvider.GetRequiredService<IMessageEndPoint>();

            //await messageEndPoint.Initialization;



            //await messageEndPoint.Completion;
        }
    }

    internal sealed class ModuleHandler :
        IMessageHandler<SetupModule>,
        IMessageHandler<TearDownModule>
    {
        private readonly IModule _module;

        public ModuleHandler(IModule module)
        {
            if (module == null)
                throw new ArgumentNullException(nameof(module));

            _module = module;
        }

        public Task HandleAsync(SetupModule message)
        {
            return _module.ActivateAsync();
        }

        public Task HandleAsync(TearDownModule message)
        {
            return _module.DeactivateAsync();
        }
    }
}
