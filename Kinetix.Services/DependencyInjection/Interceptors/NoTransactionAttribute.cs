using System;

namespace Kinetix.Services.DependencyInjection.Interceptors
{
    [AttributeUsage(AttributeTargets.Method)]
    public class NoTransactionAttribute : Attribute
    {
    }
}
