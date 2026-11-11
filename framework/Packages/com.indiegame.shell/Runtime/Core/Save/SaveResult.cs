namespace IndieGame.Shell
{
    public enum SaveError { None, InvalidData, Io, FutureVersion, Migration, InvalidArgument }

    public sealed class SaveResult<T>
    {
        public T Value { get; }
        public SaveError Error { get; }
        public string Message { get; }
        public bool Recovered { get; }
        public bool Reset { get; }
        public bool Success => Error == SaveError.None;
        public SaveResult(T value, SaveError error = SaveError.None, string message = "",
            bool recovered = false, bool reset = false)
        { Value = value; Error = error; Message = message; Recovered = recovered; Reset = reset; }
    }
}
