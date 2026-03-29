using p4w.Data;
using p4w.Service;

namespace p4w.Api.Configurations;

public static class ApplicationContainer
{
    public static IServiceCollection AddApplicationContainer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddData(configuration);
        services.AddServices();
        return services;
    }
}
