using Microsoft.EntityFrameworkCore;
using p4w.Core.Constants.Statuses;
using p4w.Core.Dtos.Dashboard;
using p4w.Core.Interfaces.Repositories.Dashboard;
using p4w.Data.Persistence;

namespace p4w.Data.Repositories.Dashboard;

public sealed class AdminDashboardRepository : IAdminDashboardRepository
{
    private readonly AppDbContext _context;

    public AdminDashboardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AdminDashboardDto> GetDashboardAsync()
    {
        var totalUsers = await _context.Users.CountAsync(x => x.Status != UserStatuses.Inactive);

        var locationApprovedCount = await _context.Locations.CountAsync(x =>
            x.Status == LocationStatuses.Approved || x.Status == LocationStatuses.Active);
        var locationPendingCount = await _context.Locations.CountAsync(x => x.Status == LocationStatuses.Pending);

        var reportApprovedCount = await _context.Reports.CountAsync(x => x.Status == ReportStatuses.Approved);
        var reportPendingCount = await _context.Reports.CountAsync(x => x.Status == ReportStatuses.Pending);

        return new AdminDashboardDto
        {
            TotalUsers = totalUsers,
            Locations = BuildRatio(locationApprovedCount, locationPendingCount),
            Reports = BuildRatio(reportApprovedCount, reportPendingCount)
        };
    }

    private static DashboardRatioDto BuildRatio(int approvedCount, int pendingCount)
    {
        var total = approvedCount + pendingCount;
        if (total == 0)
        {
            return new DashboardRatioDto
            {
                ApprovedCount = 0,
                PendingCount = 0,
                ApprovedPercentage = 0,
                PendingPercentage = 0
            };
        }

        return new DashboardRatioDto
        {
            ApprovedCount = approvedCount,
            PendingCount = pendingCount,
            ApprovedPercentage = Math.Round(approvedCount * 100d / total, 2),
            PendingPercentage = Math.Round(pendingCount * 100d / total, 2)
        };
    }
}
