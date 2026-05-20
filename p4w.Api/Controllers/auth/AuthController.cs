using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using p4w.Api.Dtos.Auth;
using p4w.Core.Constants;
using p4w.Core.Dtos.User;
using p4w.Core.Exceptions;
using p4w.Core.Interfaces.Repositories.Auth;
using p4w.Core.Interfaces.Services.Auth;
using p4w.Core.Paginations;

namespace p4w.Api.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtService _jwtService;
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthController(
        IAuthService authService,
        IJwtService jwtService,
        IUserRepository userRepository,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _authService = authService;
        _jwtService = jwtService;
        _userRepository = userRepository;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Sign in with internal credentials (username/password).
    /// </summary>
    /// <remarks>
    /// Returns access token and refresh token for authenticated API calls.
    /// </remarks>
    [HttpPost("admin-login")]
    public async Task<ApiResponse<LoginResponse>> LoginAsync([FromBody] LoginRequest request)
    {
        return await _authService.LoginAsync(request.UserName, request.Password);
    }

    /// <summary>
    /// Sign in with a Google ID token.
    /// </summary>
    /// <remarks>
    /// Client submits an idToken from Google Sign-In. The API validates it and issues internal tokens.
    /// </remarks>
    [HttpPost("login-google")]
    public async Task<ApiResponse<LoginResponse>> LoginWithGoogleAsync([FromBody] GoogleLoginRequest request)
    {
        return await _authService.LoginWithGoogleAsync(request.IdToken);
    }

    /// <summary>
    /// Redirect user to Google OAuth authorization page.
    /// </summary>
    /// <remarks>
    /// Optional redirectUri is used to send the user back to frontend after success or failure.
    /// </remarks>
    [HttpGet("google-login")]
    public IActionResult GoogleLogin([FromQuery] string? redirectUri = null)
    {
        var clientId = _configuration["Authentication:Google:ClientId"]
            ?? Environment.GetEnvironmentVariable("Authentication__Google__ClientId")
            ?? throw new InvalidOperationException("Google ClientId is missing.");

        var effectiveRedirectUri = ResolveGoogleRedirectUri(redirectUri);
        var callbackUri = BuildGoogleCallbackUri();
        var state = BuildGoogleState(effectiveRedirectUri);
        var authUrl = QueryHelpers.AddQueryString(
            "https://accounts.google.com/o/oauth2/v2/auth",
            new Dictionary<string, string?>
            {
                ["client_id"] = clientId,
                ["redirect_uri"] = callbackUri,
                ["response_type"] = "code",
                ["scope"] = "openid email profile",
                ["access_type"] = "offline",
                ["prompt"] = "select_account",
                ["state"] = state
            });

        return Redirect(authUrl);
    }

    /// <summary>
    /// Callback endpoint to complete Google sign-in.
    /// </summary>
    /// <remarks>
    /// Exchanges authorization code for id_token, signs in internally, then redirects to redirectUri when provided.
    /// </remarks>
    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallbackAsync([FromQuery] string? code, [FromQuery] string? error = null, [FromQuery] string? redirectUri = null, [FromQuery] string? state = null)
    {
        var effectiveRedirectUri = ResolveGoogleRedirectUri(redirectUri, state);

        if (!string.IsNullOrWhiteSpace(error))
        {
            return BuildGoogleRedirectResult(effectiveRedirectUri, success: false, message: error);
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            return BuildGoogleRedirectResult(effectiveRedirectUri, success: false, message: MessageConstant.AuthMessage.GOOGLE_MISSING_CODE);
        }

        var clientId = _configuration["Authentication:Google:ClientId"]
            ?? Environment.GetEnvironmentVariable("Authentication__Google__ClientId")
            ?? throw new InvalidOperationException("Google ClientId is missing.");

        var clientSecret = _configuration["Authentication:Google:ClientSecret"]
            ?? Environment.GetEnvironmentVariable("Authentication__Google__ClientSecret")
            ?? throw new InvalidOperationException("Google ClientSecret is missing.");

        var callbackUri = BuildGoogleCallbackUri();
        var httpClient = _httpClientFactory.CreateClient();

        using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["redirect_uri"] = callbackUri,
                ["grant_type"] = "authorization_code"
            })
        };
        tokenRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var tokenResponse = await httpClient.SendAsync(tokenRequest);
        var tokenJson = await tokenResponse.Content.ReadAsStringAsync();

        if (!tokenResponse.IsSuccessStatusCode)
        {
            return BuildGoogleRedirectResult(effectiveRedirectUri, success: false, message: MessageConstant.AuthMessage.GOOGLE_TOKEN_EXCHANGE_FAILED);
        }

        var tokenPayload = JsonSerializer.Deserialize<GoogleTokenResponse>(tokenJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (string.IsNullOrWhiteSpace(tokenPayload?.IdToken))
        {
            return BuildGoogleRedirectResult(effectiveRedirectUri, success: false, message: MessageConstant.AuthMessage.GOOGLE_MISSING_ID_TOKEN);
        }

        var loginResponse = await _authService.LoginWithGoogleAsync(tokenPayload.IdToken);

        if (string.IsNullOrWhiteSpace(effectiveRedirectUri))
        {
            return Ok(loginResponse);
        }

        if (!loginResponse.Success || loginResponse.Data == null)
        {
            return BuildGoogleRedirectResult(effectiveRedirectUri, success: false, message: loginResponse.Message ?? MessageConstant.AuthMessage.LOGIN_FAILED);
        }

        var finalRedirect = QueryHelpers.AddQueryString(
            effectiveRedirectUri,
            new Dictionary<string, string?>
            {
                ["success"] = "true",
                ["accessToken"] = string.IsNullOrWhiteSpace(loginResponse.Data.accessToken) ? null : loginResponse.Data.accessToken,
                ["refreshToken"] = string.IsNullOrWhiteSpace(loginResponse.Data.refreshToken) ? null : loginResponse.Data.refreshToken,
                ["expiresAt"] = loginResponse.Data.expiresAt.ToString("O"),
                ["refreshTokenExpiryTime"] = loginResponse.Data.RefreshTokenExpiryTime.ToString("O")
            });

        return Redirect(finalRedirect);
    }

    /// <summary>
    /// Register a new account.
    /// </summary>
    /// <remarks>
    /// Creates a new user in the system from client registration data.
    /// </remarks>
    [HttpPost("register")]
    public async Task<ApiResponse<bool>> RegisterAsync([FromBody] RegisterRequest request)
    {
        return await _authService.RegisterAsync(request);
    }

    /// <summary>
    /// Sign out the current account.
    /// </summary>
    /// <remarks>
    /// Revokes refresh token for the currently authenticated user.
    /// </remarks>
    [Authorize]
    [HttpPost("logout")]
    public async Task<ApiResponse<bool>> LogoutAsync()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            throw new AppException(MessageConstant.AuthMessage.INVALID_TOKEN, ErrorCodes.Unauthorized, StatusCodes.Status401Unauthorized);
        }

        return await _authService.LogoutAsync(Guid.Parse(userId));
    }

    /// <summary>
    /// Update profile for the current user.
    /// </summary>
    /// <remarks>
    /// Requires valid JWT and only updates profile of the user in the access token.
    /// </remarks>
    [Authorize]
    [HttpPut("update-profile")]
    public async Task<ApiResponse<UserProfileDto>> UpdateProfileAsync([FromBody] UpdateProfileRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            throw new AppException(MessageConstant.AuthMessage.INVALID_TOKEN, ErrorCodes.Unauthorized, StatusCodes.Status401Unauthorized);
        }

        return await _authService.UpdateProfileAsync(Guid.Parse(userId), request);
    }

    /// <summary>
    /// Refresh access token using refresh token.
    /// </summary>
    /// <remarks>
    /// Validates refresh token, then issues new token pair and updates refresh token expiry.
    /// </remarks>
    [HttpPost("refresh-token")]
    public async Task<ApiResponse<LoginResponse>> RefreshTokenAsync([FromBody] RefreshTokenRequest request)
    {
        var principal = _jwtService.GetPrincipalFromExpiredToken(request.RefreshToken);
        if (principal == null)
        {
            throw new AppException(MessageConstant.AuthMessage.INVALID_TOKEN, ErrorCodes.Unauthorized, StatusCodes.Status401Unauthorized);
        }

        var type = principal.Claims.FirstOrDefault(x => x.Type == "type")?.Value;
        if (type != "refresh")
        {
            throw new AppException(MessageConstant.AuthMessage.INVALID_REFRESH_TOKEN, ErrorCodes.Unauthorized, StatusCodes.Status401Unauthorized);
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            throw new AppException(MessageConstant.AuthMessage.INVALID_TOKEN, ErrorCodes.Unauthorized, StatusCodes.Status401Unauthorized);
        }

        var user = await _userRepository.GetUserByIdAsync(Guid.Parse(userId));
        if (user.RefreshToken != request.RefreshToken)
        {
            throw new AppException(MessageConstant.AuthMessage.REFRESH_TOKEN_MISMATCH, ErrorCodes.Unauthorized, StatusCodes.Status401Unauthorized);
        }

        if (user.RefreshTokenExpiryTime == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new AppException(MessageConstant.AuthMessage.REFRESH_TOKEN_EXPIRED, ErrorCodes.Unauthorized, StatusCodes.Status401Unauthorized);
        }

        var newAccessToken = _jwtService.GenerateToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken(user);
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(3);
        var accessTokenExpiry = DateTime.UtcNow.AddMinutes(5);

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = refreshTokenExpiry;
        await _userRepository.UpdateAsync(user);

        return new ApiResponse<LoginResponse>
        {
            Code = 200,
            Success = true,
            Data = new LoginResponse
            {
                accessToken = newAccessToken,
                refreshToken = newRefreshToken,
                expiresAt = accessTokenExpiry,
                RefreshTokenExpiryTime = refreshTokenExpiry
            }
        };
    }

    private string BuildGoogleCallbackUri()
    {
        var callbackBaseUrl = ResolveGoogleCallbackBaseUrl();
        return $"{callbackBaseUrl}/api/Auth/google-callback";
    }

    private string ResolveGoogleCallbackBaseUrl()
{
    return "https://transit-circle-leather-lead.trycloudflare.com";
}

    private string? ResolveGoogleRedirectUri(string? redirectUri, string? state = null)
    {
        if (!string.IsNullOrWhiteSpace(redirectUri))
        {
            return redirectUri;
        }

        var redirectUriFromState = ReadRedirectUriFromState(state);
        if (!string.IsNullOrWhiteSpace(redirectUriFromState))
        {
            return redirectUriFromState;
        }

        return _configuration["Authentication:Google:RedirectUri"]
            ?? Environment.GetEnvironmentVariable("Authentication__Google__RedirectUri");
    }

    private static string? BuildGoogleState(string? redirectUri)
    {
        if (string.IsNullOrWhiteSpace(redirectUri))
        {
            return null;
        }

        var json = JsonSerializer.Serialize(new GoogleAuthState
        {
            RedirectUri = redirectUri
        });

        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(json));
    }

    private static string? ReadRedirectUriFromState(string? state)
    {
        if (string.IsNullOrWhiteSpace(state))
        {
            return null;
        }

        try
        {
            var json = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(state));
            var payload = JsonSerializer.Deserialize<GoogleAuthState>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return payload?.RedirectUri;
        }
        catch
        {
            return null;
        }
    }

    private IActionResult BuildGoogleRedirectResult(string? redirectUri, bool success, string message)
    {
        if (string.IsNullOrWhiteSpace(redirectUri))
        {
            return BadRequest(new ApiResponse<LoginResponse>
            {
                Success = success,
                Message = message,
                Data = null
            });
        }

        var finalRedirect = QueryHelpers.AddQueryString(
            redirectUri,
            new Dictionary<string, string?>
            {
                ["success"] = success ? "true" : "false",
                ["message"] = message
            });

        return Redirect(finalRedirect);
    }

    private sealed class GoogleTokenResponse
    {
        [JsonPropertyName("id_token")]
        public string? IdToken { get; set; }

        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public int? ExpiresIn { get; set; }
    }

    private sealed class GoogleAuthState
    {
        public string? RedirectUri { get; set; }
    }
}
