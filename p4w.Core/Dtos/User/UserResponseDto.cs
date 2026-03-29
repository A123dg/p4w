namespace p4w.Core.Dtos.User;

public class UserResponseDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = null!;
    public int Status { get; set; }
    public string StatusName { get; set; } = null!;
    public DateTime? DateOfBirth { get; set; }
    public string MediaLinkUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<OwnedLocationDto> OwnedLocations { get; set; } = [];
}
