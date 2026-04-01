namespace p4w.Core.Dtos.Comment;

public class AdminCommentDto
{
    public Guid Id { get; set; }
    public Guid ReviewId { get; set; }
    public Guid? ParentId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    public Guid LocationId { get; set; }
    public string LocationName { get; set; } = null!;
    public string ReviewContent { get; set; } = null!;
    public string Content { get; set; } = null!;
    public int Status { get; set; }
    public string StatusName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
