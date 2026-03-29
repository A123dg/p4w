namespace p4w.Core.Dtos.User;

public class AdminUpsertUserRequest
{
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public Guid RoleId { get; set; }
    public int Status { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? MediaLinkUrl { get; set; }
}
