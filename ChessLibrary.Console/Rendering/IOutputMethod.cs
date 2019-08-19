namespace ChessLibrary.ConsoleApp.Rendering
{
    public interface IOutputMethod
    {
        void Write(string s);
        void Write(char c);
        void Flush();
    }
}
