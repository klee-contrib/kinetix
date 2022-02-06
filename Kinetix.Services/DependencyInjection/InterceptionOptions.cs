using Castle.DynamicProxy;

namespace Kinetix.Services.DependencyInjection;

public class InterceptionOptions
{
    private readonly IDictionary<Type, Type> _interceptors = new Dictionary<Type, Type>();

    public InterceptionOptions With<TInterceptor>() where TInterceptor : IInterceptor
    {
        if (!_interceptors.ContainsKey(typeof(TInterceptor)))
        {
            _interceptors.Add(typeof(TInterceptor), typeof(TInterceptor));
        }

        return this;
    }

    public List<Type> Interceptors => _interceptors.Values.ToList();
}
