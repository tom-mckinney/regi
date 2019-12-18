using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Regi.CommandLine.Commands;
using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Regi.CommandLine
{
    public static class Program
    {
        public static System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return new HostBuilder()
                .UseDefaultServiceProvider((options) =>
                {
                    options.ValidateOnBuild = true;
                    options.ValidateScopes = false;
                })
                .ConfigureServices(ConfigureServices);
        }

        public static Task Main(string[] args)
        {
            watch.Start();

            var parser = new CommandLineBuilder(
                new RootCommand
                {
                    Handler = CommandHandler.Create<IHost>(Run),
                })
                .UseDefaults()
                .UseHost(CreateHostBuilder)

                // Comands
                .AddCommand<StartCommand>()

                .Build();

            return parser.InvokeAsync(args);
        }

        private static void Run(IHost host)
        {
            watch.Stop();

            Console.WriteLine(watch.ElapsedMilliseconds);
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
        }
    }
}
