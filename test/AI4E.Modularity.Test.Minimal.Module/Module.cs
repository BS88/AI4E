using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace AI4E.Modularity.Test.Minimal.Module
{
    public class Module : IModule
    {
        protected static void Main(string[] args)
        {
            var services = new ServiceCollection();

            ConfigureServices(services);

            var moduleRunner = new ModuleBuilder<Module>()
                                .UsePort(54321)
                                .Build(services);

            moduleRunner.Run();
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            // Currently nothing to do here.
        }

        public async Task ActivateAsync()
        {
            Console.WriteLine("Activated...");
        }

        public async Task DeactivateAsync()
        {
            Console.WriteLine("Deactivated...");
        }
    }
}
