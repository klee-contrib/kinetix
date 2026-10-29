using System.Diagnostics;
using AspectCore.DynamicProxy;
using Microsoft.Extensions.Logging;
using OpenTelemetry;

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
        if (Sdk.SuppressInstrumentation)
        {
            await next(context);
            return;
        }
        else if (Attribute.IsDefined(context.ServiceMethod, typeof(NoTraceAttribute), inherit: true))
        {
            using (SuppressInstrumentationScope.Begin())
            {
                await next(context);
            }

            return;
        }

        var name = $"{context.ServiceMethod.DeclaringType?.Name}.{context.ServiceMethod.Name}";

        using var activity = Source.StartActivity(name);
        var start = Stopwatch.GetTimestamp();

        try
        {
            await next(context);
            logger.LogInformation(
                "{ServiceMethod} ({Duration} ms)",
                name,
                Stopwatch.GetElapsedTime(start).TotalMilliseconds
            );
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
