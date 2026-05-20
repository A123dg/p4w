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

    public async Task<AdminDashboardDto> GetDashboardAsync(string? period, int? month, int? year)
    {
        var normalizedPeriod = NormalizePeriod(period);
        var (rangeStartUtc, rangeEndUtc, resolvedMonth, resolvedYear) = ResolveRange(normalizedPeriod, month, year);

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
            Month = resolvedMonth,
            Year = resolvedYear,
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

    private static (DateTime RangeStartUtc, DateTime RangeEndUtc, int? Month, int Year) ResolveRange(
        string period,
        int? month,
        int? year)
    {
        var now = DateTime.UtcNow;
        var resolvedYear = ResolveYear(year, now.Year);

        return period switch
        {
            DashboardPeriods.Week => BuildWeekRange(now),
            DashboardPeriods.Year => BuildYearRange(resolvedYear),
            _ => BuildMonthRange(ResolveMonth(month, now.Month), resolvedYear)
        };
    }

    private static (DateTime RangeStartUtc, DateTime RangeEndUtc, int? Month, int Year) BuildWeekRange(DateTime now)
    {
        var rangeStartUtc = StartOfWeekUtc(now);
        return (rangeStartUtc, rangeStartUtc.AddDays(7), null, now.Year);
    }

    private static (DateTime RangeStartUtc, DateTime RangeEndUtc, int? Month, int Year) BuildMonthRange(int month, int year)
    {
        var rangeStartUtc = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        return (rangeStartUtc, rangeStartUtc.AddMonths(1), month, year);
    }

    private static (DateTime RangeStartUtc, DateTime RangeEndUtc, int? Month, int Year) BuildYearRange(int year)
    {
        var rangeStartUtc = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (rangeStartUtc, rangeStartUtc.AddYears(1), null, year);
    }

    private static int ResolveMonth(int? month, int fallbackMonth)
    {
        return month is >= 1 and <= 12 ? month.Value : fallbackMonth;
    }

    private static int ResolveYear(int? year, int fallbackYear)
    {
        return year is >= 1 and <= 9999 ? year.Value : fallbackYear;
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
