using System.Collections.Generic;

namespace ChessLibrary.ConsoleApp
{
    public static class Commands
    {
        public const string Move = "move";
        public const string Help = "help";
        public const string Exit = "exit";
        public const string Hint = "hint";

        public static Dictionary<string, string> Descriptions => new Dictionary<string, string>()
        {
            { Move, "Move a piece using algebraic notation. Ex: 'move a4' or 'move Nd2'" },
            { Hint, "Show valid moves for a square. Ex: 'hint a2'" },
            { Help, "Print help text" },
            { Exit, "Exit the program" }
        };
    }
}
