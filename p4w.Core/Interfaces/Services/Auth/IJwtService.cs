using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using p4w.Core.Models;
namespace p4w.Core.Interfaces.Services.Auth;
public interface IJwtService {
    string GenerateToken(User user);
    string GenerateRefreshToken(User user);
    ClaimsPrincipal ValidateToken(string token);

    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}