using p4w.Core.Dtos.Dashboard;

namespace p4w.Core.Interfaces.Services.Dashboard;

public interface IAdminDashboardService
{
    Task<AdminDashboardDto> GetDashboardAsync();
}
