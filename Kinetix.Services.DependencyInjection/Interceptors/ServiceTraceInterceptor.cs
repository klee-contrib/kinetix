using System.Diagnostics;
using AspectCore.DynamicProxy;
using Microsoft.Extensions.Logging;

namespace Kinetix.Services.DependencyInjection.Interceptors;

/// <summary>
/// Intercepteur pour tracer les appels de service via OpenTelemetry.
/// </summary>
public class ServiceTraceInterceptor(ILogger<Service> logger) : AbstractInterceptor
{
    public const string ActivitySourceName = $"{nameof(Kinetix)}.{nameof(Service)}";

    private static readonly ActivitySource Source = new(ActivitySourceName);

    /// <inheritdoc cref="IInterceptor.Invoke" />
    [DebuggerNonUserCode]
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        var name = $"{context.ServiceMethod.DeclaringType?.Name}.{context.ServiceMethod.Name}";
        var disabled = Attribute.IsDefined(context.ServiceMethod, typeof(NoServiceTraceAttribute), inherit: true);
        using var activity = disabled ? null : Source.StartActivity(name);
        var start = Stopwatch.GetTimestamp();

        try
        {
            await next(context);
            if (!disabled)
            {
                logger.LogInformation(
                    "{ServiceMethod} ({Duration} ms)",
                    name,
                    Stopwatch.GetElapsedTime(start).TotalMilliseconds
                );
            }
        }
        catch (Exception ex)
        {
            while (ex is AggregateException { InnerException: not null })
            {
                ex = ex.InnerException;
            }

            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            if (ex.Data["logged"] == null)
            {
                activity?.AddException(ex);
                ex.Data["logged"] = true;
            }

            throw;
        }
    }
}
