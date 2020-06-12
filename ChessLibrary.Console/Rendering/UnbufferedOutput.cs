using System;

namespace ChessLibrary.ConsoleApp.Rendering
{
#pragma warning disable CA1063 // Implement IDisposable Correctly
    public class UnbufferedOutput : IOutputMethod
#pragma warning restore CA1063 // Implement IDisposable Correctly
    {
        public void Write(string s) => Console.Write(s);

        public void Write(char c) => Console.Write(c);

        public void Flush() { }

#pragma warning disable CA1063 // Implement IDisposable Correctly
        public void Dispose() { }
#pragma warning restore CA1063 // Implement IDisposable Correctly
    }
}
