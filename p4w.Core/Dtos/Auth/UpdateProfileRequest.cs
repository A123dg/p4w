namespace p4w.Api.Dtos.Auth;

public class UpdateProfileRequest
{
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime? DateOfBirth { get; set; }
    public string? Password { get; set; }
}
