namespace p4w.Core.Dtos.Comment;

public class CommentDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string AvatarUrl { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public List<CommentDto> Children { get; set; } = [];
}
