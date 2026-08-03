using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Onspay.Cqrs;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCqrs(configuration, typeof(DependencyInjection).Assembly);
        return services;
    }
}