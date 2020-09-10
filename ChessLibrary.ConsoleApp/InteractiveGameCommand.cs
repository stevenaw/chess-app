namespace ChessLibrary.ConsoleApp
{
    internal readonly ref struct InteractiveGameCommand
    {
        public readonly string TotalInput { get; }
        public readonly string CommandName { get; }
        public readonly string CommandArgs { get; }

        public InteractiveGameCommand(string totalInput, string commandName, string commandArgs)
        {
            TotalInput = totalInput;
            CommandName = commandName;
            CommandArgs = commandArgs;
        }
    }
}
