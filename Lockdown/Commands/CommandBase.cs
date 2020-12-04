namespace Lockdown.Commands
{
    using McMaster.Extensions.CommandLineUtils;

    public abstract class CommandBase
    {
        protected virtual int OnExecute(CommandLineApplication app)
        {
            return 0;
        }
    }
}
