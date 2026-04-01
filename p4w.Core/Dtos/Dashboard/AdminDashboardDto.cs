namespace p4w.Core.Dtos.Dashboard;

public sealed class AdminDashboardDto
{
    public int TotalUsers { get; set; }
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
