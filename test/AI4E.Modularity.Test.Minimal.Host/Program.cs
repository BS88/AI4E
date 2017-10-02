using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace AI4E.Modularity.Test.Minimal.Host
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var services = new ServiceCollection();

            ConfigureServices(services);

            var serviceProvider = services.BuildServiceProvider();

            var moduleHost = serviceProvider.GetRequiredService<IModuleHost>();

            await moduleHost.Completion;
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddMessaging();
            services.AddModularity();
        }
    }
}
