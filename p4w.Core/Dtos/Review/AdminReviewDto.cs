namespace p4w.Core.Dtos.Review;

public class AdminReviewDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    public Guid LocationId { get; set; }
    public string LocationName { get; set; } = null!;
    public int Rating { get; set; }
    public string Content { get; set; } = null!;
    public int Status { get; set; }
    public string StatusName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
