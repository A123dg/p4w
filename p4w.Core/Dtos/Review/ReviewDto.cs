namespace p4w.Core.Dtos.Review;

public class ReviewDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string AvatarUrl { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public int CommentCount { get; set; }
}
