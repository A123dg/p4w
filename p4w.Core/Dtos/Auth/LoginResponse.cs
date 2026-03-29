namespace p4w.Api.Dtos.Auth;
public class LoginResponse {
    public string accessToken {get;set;} = null!;
    public string refreshToken {get;set;} = null!;
    public DateTime expiresAt {get;set;}

    public DateTime RefreshTokenExpiryTime { get; set; }
}