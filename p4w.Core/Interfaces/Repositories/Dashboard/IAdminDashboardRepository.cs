using p4w.Core.Dtos.Dashboard;

namespace p4w.Core.Interfaces.Repositories.Dashboard;

public interface IAdminDashboardRepository
{
    Task<AdminDashboardDto> GetDashboardAsync(string? period, int? month, int? year);
}
