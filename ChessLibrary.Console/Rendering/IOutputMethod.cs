using System;

namespace ChessLibrary.ConsoleApp.Rendering
{
    public interface IOutputMethod : IDisposable
    {
        void Write(string s);
        void Write(char c);
        void Flush();
    }
}
