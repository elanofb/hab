namespace habitaai.webapi.Dynamic
{
    public static class DebugLogger
    {
        private static readonly List<string> _entries = new();

        public static void Log(string message)
        {
            _entries.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
        }

        public static string Dump()
        {
            return string.Join("\n", _entries);
        }

        public static void Clear()
        {
            _entries.Clear();
        }
    }

}
