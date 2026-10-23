using System.Diagnostics;
using System.Reflection;
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
        var name = $"{context.ImplementationMethod.DeclaringType?.FullName}.{context.ImplementationMethod.Name}";
        var disabled = Attribute.IsDefined(
            context.ImplementationMethod,
            typeof(NoServiceTraceAttribute),
            inherit: true
        );
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
            if (ex is AggregateException)
            {
                ex = ex.InnerException!;
            }

            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("error.type", ex.GetType().FullName);
            activity?.AddException(ex);

            logger.LogError(ex, $"Erreur sur le service {name}");
            throw new TargetInvocationException($"Une erreur est survenue sur le service {name}", ex);
        }
    }
}
