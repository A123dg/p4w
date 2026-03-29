using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using p4w.Core.Interfaces.Repositories.Auth;
using p4w.Core.Interfaces.Repositories.LocationRepo;
using p4w.Core.Interfaces.Repositories.MediaRepo;
using p4w.Core.Interfaces.Repositories.Report;
using p4w.Data.Persistence;
using p4w.Data.Repositories.Location;
using p4w.Data.Repositories.Report;

namespace p4w.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IUserRepository,UserRepository>();
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IMediaRepository, MediaRepository>();
        // services.AddSingleton<ITaskRepository, InMemoryTaskRepository>();
        // services.AddScoped<ITaskRepository, InMemoryTaskRepository>();

        return services;
    }
}
