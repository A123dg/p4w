using p4w.Core.Dtos.Dashboard;
using p4w.Core.Interfaces.Repositories.Dashboard;
using p4w.Core.Interfaces.Services.Dashboard;

namespace p4w.Service.Services.Dashboard;

public sealed class AdminDashboardService : IAdminDashboardService
{
    private readonly IAdminDashboardRepository _adminDashboardRepository;

    public AdminDashboardService(IAdminDashboardRepository adminDashboardRepository)
    {
        _adminDashboardRepository = adminDashboardRepository;
    }

    public Task<AdminDashboardDto> GetDashboardAsync(string? period, int? month, int? year)
    {
        return _adminDashboardRepository.GetDashboardAsync(period, month, year);
    }
}
