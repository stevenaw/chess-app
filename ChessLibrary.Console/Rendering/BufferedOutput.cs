using System;
using System.IO;

namespace ChessLibrary.ConsoleApp.Rendering
{
    public class BufferedOutput : IOutputMethod
    {
        private bool disposedValue;
        private StreamWriter Output { get; } = new StreamWriter(Console.OpenStandardOutput());

        public void Write(string s) => Output.Write(s);

        public void Write(char c) => Output.Write(c);

        public void Flush() => Output.Flush();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                    Output.Dispose();

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
