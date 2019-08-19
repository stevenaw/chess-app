using System;

namespace ChessLibrary.ConsoleApp.Rendering
{
    public class UnbufferedOutput : IOutputMethod
    {
        public void Write(string s)
        {
            Console.Write(s);
        }

        public void Write(char c)
        {
            Console.Write(c);
        }

        public void Flush() { }
    }
}
