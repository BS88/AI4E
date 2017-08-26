using System;
using System.Threading.Tasks;
using AI4E.Integration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

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

            Console.ReadLine();
        }

        public static void AddServices(IServiceCollection services)
        {
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

            await Task.Delay(2000);

            return Success(123, "This is a success");
        }
    }

    public interface IEmailSender
    {
        Task SendAsync(string s);
    }

    public class EmailSender : IEmailSender
    {
        public async Task SendAsync(string s)
        {
            await Task.Delay(4000);
            Console.WriteLine(s);
        }
    }
}
