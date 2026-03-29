namespace p4w.Core.Dtos.Comment;

public class CreateCommentRequest
{
    public Guid ReviewId { get; set; }
    public Guid? ParentId { get; set; }
    public string Content { get; set; } = null!;
}
