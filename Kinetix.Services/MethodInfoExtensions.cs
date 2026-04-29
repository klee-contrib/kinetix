using System.Reflection;

namespace Kinetix.Services;

public static class MethodInfoExtensions
{
    public static async Task<T> InvokeAsync<T>(this MethodInfo methodInfo, object obj, params object?[] parameters)
    {
        dynamic awaitable = methodInfo.Invoke(obj, parameters)!;
        await awaitable;
        return (T)awaitable.GetAwaiter().GetResult();
    }
}
