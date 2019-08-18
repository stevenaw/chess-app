using System;

namespace ChessLibrary.ConsoleApp
{
    public class ConsolePaintBrush
    {
        public ConsoleColor Foreground { get; set; }
        public ConsoleColor Background { get; set; }

        public static ConsolePaintBrush Current
        {
            get
            {
                return new ConsolePaintBrush()
                {
                    Foreground = Console.ForegroundColor,
                    Background = Console.BackgroundColor
                };
            }
            set
            {
                Console.ForegroundColor = value.Foreground;
                Console.BackgroundColor = value.Background;
            }
        }

        public static ConsolePaintBrush Highlight => new ConsolePaintBrush()
        {
            Foreground = ConsoleColor.Black,
            Background = ConsoleColor.Yellow
        };
    }
}
