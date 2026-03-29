namespace p4w.Core.Dtos.Report;

public class ReportDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ReportedBy { get; set; } = null!;
    public string Reason { get; set; } = null!;
    public string ReportedItemType { get; set; } = null!;
    public string ReportedItemId { get; set; } = null!;
    public string ReportedItem { get; set; } = null!;
    public int Status { get; set; }
    public string StatusName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
