using System.IO;
using System.Text;

namespace IndieGame.Shell
{
    // Implementations must preserve destination on failed Write/Replace/Move.
    public interface ISaveFileSystem
    {
        bool Exists(string path);
        string ReadAllText(string path);
        void WriteAllText(string path, string text);
        void Replace(string source, string destination, string? backup);
        void Move(string source, string destination);
    }

    public sealed class SystemSaveFileSystem : ISaveFileSystem
    {
        public bool Exists(string path) => File.Exists(path);
        public string ReadAllText(string path) => File.ReadAllText(path);
        public void WriteAllText(string path, string text)
        {
            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false), 1024, leaveOpen: true))
            { writer.Write(text); writer.Flush(); }
            stream.Flush(flushToDisk: true);
        }
        public void Replace(string source, string destination, string? backup) => File.Replace(source, destination, backup);
        public void Move(string source, string destination) => File.Move(source, destination);
    }
}
