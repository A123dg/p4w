namespace p4w.Core.Dtos.Dashboard;

public sealed class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public string Period { get; set; } = DashboardPeriods.Month;
    public DateTime RangeStartUtc { get; set; }
    public DateTime RangeEndUtc { get; set; }
    public DashboardRatioDto Locations { get; set; } = new();
    public DashboardRatioDto Reports { get; set; } = new();
}

public sealed class DashboardRatioDto
{
    public int ApprovedCount { get; set; }
    public int PendingCount { get; set; }
    public double ApprovedPercentage { get; set; }
    public double PendingPercentage { get; set; }
}

public static class DashboardPeriods
{
    public const string Week = "week";
    public const string Month = "month";
    public const string Year = "year";

    public static readonly string[] All = [Week, Month, Year];
}
