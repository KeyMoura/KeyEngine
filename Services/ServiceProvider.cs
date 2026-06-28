using System.Reflection;

namespace KeyEngine.Services;

/// <summary>
/// Default implementation of <see cref="IServiceResolver"/>.
/// </summary>
internal sealed class ServiceProvider : IServiceResolver
{
    private readonly Dictionary<Type, ServiceDescriptor> _services;

    public ServiceProvider(IEnumerable<ServiceDescriptor> services)
    {
        _services = services.ToDictionary(s => s.ServiceType);
    }

    public TService GetService<TService>()
        where TService : class
    {
        return (TService)GetService(typeof(TService));
    }

    public object GetService(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);

        if (!_services.TryGetValue(serviceType, out ServiceDescriptor? descriptor))
        {
            throw new InvalidOperationException(
                $"Service '{serviceType.FullName}' is not registered.");
        }

        if (descriptor.Lifetime == ServiceLifetime.Singleton)
        {
            descriptor.Instance ??= CreateInstance(descriptor);

            return descriptor.Instance;
        }

        return CreateInstance(descriptor);
    }

    private object CreateInstance(ServiceDescriptor descriptor)
    {
        ConstructorInfo constructor =
            descriptor.ImplementationType
                .GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length)
                .First();

        ParameterInfo[] parameters = constructor.GetParameters();

        if (parameters.Length == 0)
        {
            return Activator.CreateInstance(
                descriptor.ImplementationType)!;
        }

        object?[] arguments = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].ParameterType ==
                typeof(IServiceResolver))
            {
                arguments[i] = this;

                continue;
            }

            arguments[i] =
                GetService(parameters[i].ParameterType);
        }

        return constructor.Invoke(arguments);
    }
}