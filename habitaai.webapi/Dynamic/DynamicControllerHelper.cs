// Dynamic/DynamicControllerHelper.cs
namespace habitaai.webapi.Dynamic;

public static class DynamicControllerHelper
{
    public static Dictionary<string, RequestDelegate> RegisteredHandlers { get; } = new();

    public static void Register(string route, RequestDelegate handler)
    {
        RegisteredHandlers[route] = handler;
    }
}
