using Regi.Models;

namespace Regi.Commands
{
    public abstract class CommandBase : CommandOptions
    {
        public CommandOptions Options
        {
            get => new CommandOptions
            {
                Name = this.Name,
                SearchPattern = this.SearchPattern
            };
        }

        public abstract int OnExecute();
    }
}
