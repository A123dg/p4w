using System.ComponentModel.DataAnnotations;

namespace p4w.Api.Dtos.Auth;
public class RefreshTokenRequest {
    [Required]
    public string RefreshToken { get; set; } = null!;
}