namespace p4w.Core.Dtos.Report;

public class CreateReportRequest
{
    public string Reason { get; set; } = null!;
    public string TargetType { get; set; } = null!;
    public string TargetId { get; set; } = null!;
}
