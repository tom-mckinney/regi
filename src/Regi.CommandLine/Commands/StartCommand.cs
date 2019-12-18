using Regi.CommandLine.Models;
using Regi.Models;
using System;
using System.CommandLine;
using System.Threading.Tasks;

namespace Regi.CommandLine.Commands
{
    public class StartCommand : CommandBase<RegiOptions>
    {
        protected override string Name => "start";

        public override Task RunAsync(RegiOptions options, IServiceProvider services)
        {
            Console.WriteLine("Hello?");
            Console.WriteLine(options.Name);
            Console.WriteLine(options.ConfigurationPath?.ToString());
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Error.WriteLine("OH NO!");
            Console.ResetColor();

            Console.WriteLine("Last test.");

            if (options.Exclude?.Count > 0)
            {
                Console.WriteLine(string.Join(", ", options.Exclude));
            }

            Console.WriteLine(options.Type?.ToString());

            Program.watch.Stop();
            Console.WriteLine(Program.watch.ElapsedMilliseconds);

            return Task.CompletedTask;
        }
    }
}
