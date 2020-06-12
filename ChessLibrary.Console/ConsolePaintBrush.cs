using System;

namespace ChessLibrary.ConsoleApp
{
    public readonly struct ConsolePaintBrush
    {
        public readonly ConsoleColor Foreground;
        public readonly ConsoleColor Background;

        public ConsolePaintBrush(ConsoleColor foreground, ConsoleColor background)
        {
            Foreground = foreground;
            Background = background;
        }

        private static ConsolePaintBrush current = new ConsolePaintBrush(Console.ForegroundColor, Console.BackgroundColor);
        public static ConsolePaintBrush Current
        {
            get
            {
                return current;
            }
            set
            {
                current = value;

                Console.ForegroundColor = value.Foreground;
                Console.BackgroundColor = value.Background;
            }
        }

        public static ConsolePaintBrush Highlight => new ConsolePaintBrush(ConsoleColor.Black, ConsoleColor.Yellow);
    }
}
