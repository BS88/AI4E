using System;
using System.Linq;
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
            var moduleInstaller = serviceProvider.GetRequiredService<IModuleInstaller>();

            await Console.Out.WriteAsync("> ");

            for (var line = await Console.In.ReadLineAsync(); !IsExitStatement(line); line = await Console.In.ReadLineAsync())
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;

                    var moduleManager = scopedServices.GetRequiredService<IModuleManager>();

                    if (IsModuleSourceListStatement(line))
                    {
                        foreach (var source in await moduleManager.GetModuleSourcesAsync())
                        {
                            await Console.Out.WriteLineAsync($"{source.Name}: {source.Source}");
                        }
                    }
                    else if (IsModuleSourceAddStatement(line, out var name, out var uri))
                    {
                        await moduleManager.AddModuleSourceAsync(name, uri);
                    }
                    else if (IsModuleSourceUpdateStatement(line, out name, out uri))
                    {
                        await moduleManager.UpdateModuleSourceAsync(name, uri);
                    }
                    else if (IsModuleSourceRemoveStatement(line, out name))
                    {
                        await moduleManager.RemoveModuleSourceAsync(name);
                    }
                    else
                    {
                        await Console.Out.WriteLineAsync("Unknown command");
                    }
                }

                await Console.Out.WriteAsync("> ");
            }

            moduleHost.Complete();

            await moduleHost.Completion;
        }

        private static bool IsExitStatement(string line)
        {
            return string.Equals(line, "exit", StringComparison.OrdinalIgnoreCase) || string.Equals(line, "e", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsModuleSourceListStatement(string line)
        {
            return string.Equals(line, "sources", StringComparison.OrdinalIgnoreCase) || string.Equals(line, "s", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsModuleSourceAddStatement(string line, out string moduleSourceName, out string moduleSourceUri)
        {
            moduleSourceName = null;
            moduleSourceUri = null;
            string[] args;

            if (line.StartsWith("add source", StringComparison.OrdinalIgnoreCase))
            {
                args = line.Substring("add source".Length).Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            }
            else if (line.StartsWith("as", StringComparison.OrdinalIgnoreCase))
            {
                args = line.Substring("as".Length).Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            }
            else
            {
                return false;
            }

            if (args.Length != 2)
                return false;

            moduleSourceName = args[0];
            moduleSourceUri = args[1];
            return true;
        }

        private static bool IsModuleSourceRemoveStatement(string line, out string moduleSourceName)
        {
            moduleSourceName = null;
            string[] args;

            if (line.StartsWith("remove source", StringComparison.OrdinalIgnoreCase))
            {
                args = line.Substring("remove source".Length).Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            }
            else if (line.StartsWith("rs", StringComparison.OrdinalIgnoreCase))
            {
                args = line.Substring("rs".Length).Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            }
            else
            {
                return false;
            }

            if (args.Length != 1)
                return false;

            moduleSourceName = args[0];
            return true;
        }

        private static bool IsModuleSourceUpdateStatement(string line, out string moduleSourceName, out string moduleSourceUri)
        {
            moduleSourceName = null;
            moduleSourceUri = null;
            string[] args;

            if (line.StartsWith("update source", StringComparison.OrdinalIgnoreCase))
            {
                args = line.Substring("update source".Length).Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            }
            else if (line.StartsWith("us", StringComparison.OrdinalIgnoreCase))
            {
                args = line.Substring("us".Length).Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            }
            else
            {
                return false;
            }

            if (args.Length != 2)
                return false;

            moduleSourceName = args[0];
            moduleSourceUri = args[1];
            return true;
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddMessaging();
            services.AddModularity();
        }
    }
}
