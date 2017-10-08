using System;
using System.Threading.Tasks;
using AI4E;
using AI4E.EventResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using AI4E.Storage;
using AI4E.Integration;

namespace AI4E.Messaging.Test
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();

            AddServices(services);

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            var commandDispatcher = serviceProvider.GetRequiredService<ICommandDispatcher>();

            var commandResult = await commandDispatcher.DispatchAsync(new TestCommand { X = "abc" });

            Console.WriteLine("Command-handler returned: " + commandResult.ToString());

            var queryDispatcher = serviceProvider.GetRequiredService<IQueryDispatcher>();
            var queryResult = await queryDispatcher.QueryAsync(new TestQuery(Guid.NewGuid()));

            Console.WriteLine("Query-handler returned: " + queryResult.ToString());

            var eventDispatcher = serviceProvider.GetRequiredService<IEventDispatcher>();
            var eventResult = await eventDispatcher.NotifyAsync(new TestEvent { Value = Guid.NewGuid().ToString() });

            Console.WriteLine("Event-handler returned: " + eventResult.ToString());

            Console.ReadLine();
        }

        public static void AddServices(IServiceCollection services)
        {
            services.AddOptions();

            services.AddScoped<IDataStore, InMemoryDataStore>();

            services.AddMessaging(options =>
            {
                // TODO: Configure messaging
            });

            services.AddScoped<IEmailSender, EmailSender>();
        }
    }

    public class TestCommandBase
    {
        public string X { get; set; }
    }

    public class TestCommand : TestCommandBase
    {

    }

    public class TestCommandHandler : CommandHandler
    {
        public TestCommandHandler()
        {

        }

        [CommandHandlerAction(CommandType = typeof(TestCommand))]
        public async Task<ICommandResult> MyFancyCustomName(TestCommandBase command, [FromServices]IEmailSender emailSender)
        {
            await emailSender.SendAsync($"Handling command (X = {command.X})");

            await Task.Delay(0);

            return Success(123, "This is a success");
        }
    }

    public class TestQuery
    {
        public TestQuery(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class TestQueryHandler : QueryHandler
    {
        private readonly IEventDispatcher _eventDispatcher;

        public TestQueryHandler(IEventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
        }
        public async Task<Guid> HandleAsync(TestQuery query, [FromServices]IEmailSender emailSender, double d)
        {
            await emailSender.SendAsync("D: " + d);

            return query.Id;
        }
    }

    public class TestEvent
    {
        public string Value { get; set; }
    }

    public class TestEventHandler : AI4E.EventHandler
    {
        public void Handle(TestEvent evt)
        {
            Console.WriteLine(evt.Value);
        }
    }

    [EventHandler]
    public class TestEvtHdl_FancyName /*: Integration.EventHandler*/
    {
        [NoEventHandlerAction]
        public IEventResult Handle(TestEvent evt)
        {
            Console.WriteLine("Failed");

            return FailureEventResult.UnknownFailure;
        }

        [EventHandlerAction(EventType = typeof(TestEvent))]
        public async Task<IEventResult> HandleAsync(object evt)
        {
            Console.WriteLine("OK");

            return SuccessEventResult.Default;
        }
    }

    public class AnotherTestEvent
    {
        public AnotherTestEvent(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class TestProcessManager : ProcessManager<TestProcessManagerState>
    {
        public TestProcessManager()
        {

        }

        protected override void AttachProcess(IProcessAttachments<TestProcessManagerState> attachments)
        {
            attachments.Attach<TestEvent>((e, s) => e.Value == s.Id.ToString())
                       .CanStartProcess(e => new TestProcessManagerState(Guid.Parse(e.Value)));

            attachments.Attach<AnotherTestEvent>((e, s) => e.Id == s.Id);
        }

        public void Handle(TestEvent evt)
        {
            State.Count++;
        }

        public void Handle(AnotherTestEvent evt)
        {
            if (State.Count > 3)
            {
                Console.WriteLine("Shutting down...");
                TerminateProcess();
            }
        }
    }

    public class TestProcessManagerState
    {
        public TestProcessManagerState(Guid id)
        {
            Id = id;
            Count = 0;
        }

        public TestProcessManagerState() : this(Guid.NewGuid()) { }

        public Guid Id { get; }

        public int Count { get; set; }
    }



    public interface IEmailSender
    {
        Task SendAsync(string s);
    }

    public class EmailSender : IEmailSender
    {
        public async Task SendAsync(string s)
        {
            await Task.Delay(0);
            Console.WriteLine(s);
        }
    }

    //public class LoggingEventProcessor
    //{
    //    private readonly ILogger _logger;

    //    public LoggingEventProcessor(ILogger logger)
    //    {
    //        if (logger == null)
    //            throw new ArgumentNullException(nameof(logger));

    //        _logger = logger;
    //    }

    //    [EventDispatchContext]
    //    public EventDispatchContext EventDispatchContext { get; }

    //    [EventProcessorContext]
    //    public IEventProcessorContext EventProcessorContext { get; }

    //    public void PreProcess(object evt)
    //    {

    //    }

    //    public void PostProcess(object evt)
    //    {

    //    }
    //}

    //public abstract class EventProcessor
    //{
    //    [EventDispatchContext]
    //    public virtual EventDispatchContext EventDispatchContext { get; }

    //    [EventProcessorContext]
    //    public virtual EventProcessorContext EventProcessorContext { get; }

    //    public virtual Task PreProcessAsync(object evt) { return Task.CompletedTask; }

    //    public virtual Task PostProcessAsync(object evt) { return Task.CompletedTask; }
    //}

    //public class EventProcessorContext
    //{

    //}

    //[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    //public class EventProcessorContextAttribute : Attribute
    //{

    //}
}
