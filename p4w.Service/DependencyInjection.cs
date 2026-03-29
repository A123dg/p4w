using Microsoft.Extensions.DependencyInjection;
using p4w.Core.Interfaces.Services.Auth;
using p4w.Core.Interfaces.Services.Cloudinary;
using p4w.Core.Interfaces.Services.Location;
using p4w.Core.Interfaces.Services.Report;
using p4w.Service.Services.Auth;
using p4w.Service.Services.CloudinaryService;
using p4w.Service.Services.Location;
using p4w.Service.Services.Report;

namespace p4w.Service;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<ICloudinaryService, CloudinaryService>();

        return services;
    }
}
