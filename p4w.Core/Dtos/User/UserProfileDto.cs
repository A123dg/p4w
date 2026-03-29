namespace p4w.Core.Dtos.User;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public string? GoogleUserId { get; set; }
    public string Email { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public DateTime? DateOfBirth { get; set; }
    public string? Password { get; set; }
    public int Status { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public string MediaLinkUrl { get; set; } = string.Empty;
    public RecentLocationDto? RecentLocation { get; set; }
    public List<OwnedLocationDto>? OwnedLocations { get; set; } = [];
}
