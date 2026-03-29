namespace p4w.Core.Dtos.Review;

public class CreateReviewRequest
{
    public Guid LocationId { get; set; }
    public int Rating { get; set; }
    public string Content { get; set; } = null!;
}
