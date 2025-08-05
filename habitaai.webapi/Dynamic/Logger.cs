// Dynamic/Logger.cs
using System.Text.Json;

namespace habitaai.webapi.Dynamic;

public static class Logger
{
    private static readonly string logPath = Path.Combine(AppContext.BaseDirectory, "Logs");

    public static void Log(string route, object? request, object? response = null, Exception? ex = null)
    {
        Directory.CreateDirectory(logPath);

        var file = Path.Combine(logPath, $"{DateTime.UtcNow:yyyyMMdd}_dynamic.log");

        var log = new
        {
            Timestamp = DateTime.UtcNow,
            Route = route,
            Request = request,
            Response = response,
            Exception = ex?.ToString()
        };

        File.AppendAllText(file, JsonSerializer.Serialize(log) + Environment.NewLine);
    }
}
