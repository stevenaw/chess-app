using System;
using System.Text;

namespace ChessLibrary.ConsoleApp.Rendering
{
    public class BufferedOutput : IOutputMethod
    {
        // This could instead just be a char[] or Span<char>
        // At 2 EOL markers (\r\n) it's 380 in length
        private StringBuilder Buffer = new StringBuilder(
            // row = 8 squares + row number. Each has a delimiter (so *2)
            ((8 + 1) * 2 + Environment.NewLine.Length + 1)
            // rows to output = 8 squares + delimiter for each
            * (8 + 1) * 2
             + Environment.NewLine.Length
        );

        public void Write(string s)
        {
            Buffer.Append(s);
        }

        public void Write(char c)
        {
            Buffer.Append(c);
        }

        public void Flush()
        {
            Console.WriteLine(Buffer.ToString());
        }
    }
}
