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

    public async Task<AdminDashboardDto> GetDashboardAsync(string? period)
    {
        var normalizedPeriod = NormalizePeriod(period);
        var (rangeStartUtc, rangeEndUtc) = ResolveRange(normalizedPeriod);

        var totalUsers = await _context.Users.CountAsync(x =>
            x.Status != UserStatuses.Inactive &&
            x.CreatedAt >= rangeStartUtc &&
            x.CreatedAt < rangeEndUtc);

        var locationApprovedCount = await _context.Locations.CountAsync(x =>
            (x.Status == LocationStatuses.Approved || x.Status == LocationStatuses.Active) &&
            x.CreatedAt >= rangeStartUtc &&
            x.CreatedAt < rangeEndUtc);
        var locationPendingCount = await _context.Locations.CountAsync(x =>
            x.Status == LocationStatuses.Pending &&
            x.CreatedAt >= rangeStartUtc &&
            x.CreatedAt < rangeEndUtc);

        var reportApprovedCount = await _context.Reports.CountAsync(x =>
            x.Status == ReportStatuses.Approved &&
            x.CreatedAt >= rangeStartUtc &&
            x.CreatedAt < rangeEndUtc);
        var reportPendingCount = await _context.Reports.CountAsync(x =>
            x.Status == ReportStatuses.Pending &&
            x.CreatedAt >= rangeStartUtc &&
            x.CreatedAt < rangeEndUtc);

        return new AdminDashboardDto
        {
            TotalUsers = totalUsers,
            Period = normalizedPeriod,
            RangeStartUtc = rangeStartUtc,
            RangeEndUtc = rangeEndUtc,
            Locations = BuildRatio(locationApprovedCount, locationPendingCount),
            Reports = BuildRatio(reportApprovedCount, reportPendingCount)
        };
    }

    private static string NormalizePeriod(string? period)
    {
        var normalized = period?.Trim().ToLowerInvariant();
        return DashboardPeriods.All.Contains(normalized)
            ? normalized!
            : DashboardPeriods.Month;
    }

    private static (DateTime RangeStartUtc, DateTime RangeEndUtc) ResolveRange(string period)
    {
        var now = DateTime.UtcNow;

        return period switch
        {
            DashboardPeriods.Week => (StartOfWeekUtc(now), StartOfWeekUtc(now).AddDays(7)),
            DashboardPeriods.Year => (new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(now.Year + 1, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            _ => (new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1))
        };
    }

    private static DateTime StartOfWeekUtc(DateTime value)
    {
        var offset = ((int)value.DayOfWeek + 6) % 7;
        var date = value.Date.AddDays(-offset);
        return DateTime.SpecifyKind(date, DateTimeKind.Utc);
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
