using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using p4w.Core.Constants.Statuses;
using p4w.Core.Dtos.User;
using p4w.Api.Dtos.Auth;
using p4w.Core.Interfaces.Repositories.Auth;
using p4w.Core.Interfaces.Repositories.MediaRepo;
using p4w.Core.Interfaces.Services.Auth;
using p4w.Core.Models;
using p4w.Core.Paginations;
using p4w.Service.Helpers;

namespace p4w.Service.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly IJwtService _jwtService;

    private readonly IMediaRepository _mediaRepository;

    public AuthService(IUserRepository userRepository, IConfiguration configuration, IJwtService jwtService, IMediaRepository mediaRepository)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _jwtService = jwtService;
        _mediaRepository = mediaRepository;
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(string userName, string password)
    {
        User user = await _userRepository.GetUserByUserNameAsync(userName);
        if (user == null || user.Status == UserStatuses.Inactive || !PasswordHelper.VerifyPassword(password, user.Password))
        {
            return new ApiResponse<LoginResponse>
            {
                Success = false,
                Message = "Invalid username or password",
                Data = null,
                MetaData = null
            };
        }

        if (user.Status == UserStatuses.Locked)
        {
            return new ApiResponse<LoginResponse>
            {
                Success = false,
                Message = "User is locked",
                Data = null,
                MetaData = null
            };
        }

        return await BuildLoginResponseAsync(user);
    }

    public async Task<ApiResponse<LoginResponse>> LoginWithGoogleAsync(string idToken)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
        var email = payload.Email;
        var name = payload.Name;

        User user = await _userRepository.GetUserByGoogleUserIdAsync(payload.Subject);
        if (user != null && user.Status == UserStatuses.Locked)
        {
            return new ApiResponse<LoginResponse>
            {
                Success = false,
                Message = "User is locked",
                Data = null,
                MetaData = null
            };
        }

        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                UserName = name,
                GoogleUserId = payload.Subject,
                CreatedAt = DateTime.UtcNow,
                Status = UserStatuses.Active,
                RoleId = Guid.Parse("F8D2EE70-5C68-4390-A18E-11943A86142A")
            };
            await _userRepository.AddAsync(user);
        }
        else
        {
            user.GoogleUserId = payload.Subject;
        }

        return await BuildLoginResponseAsync(user);
    }

    public async Task<ApiResponse<bool>> RegisterAsync(RegisterRequest request)
    {
        var exists = await _userRepository.ExistsByEmailAsync(request.Email);
        if (exists)
            throw new Exception("Email already in use");
        var userId = Guid.NewGuid();
        var newUser = new User
        {
            Id = userId,
            Email = request.Email,
            UserName = request.UserName,
            DateOfBirth = request.DateOfBirth,
            // Password = PasswordHelper.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            Status = UserStatuses.Active,
            RoleId = Guid.Parse("F8D2EE70-5C68-4390-A18E-11943A86142A"),
        };
        if (!string.IsNullOrEmpty(request.MediaLinkUrl))
    {
        var media = new Media
        {
            Id        = Guid.NewGuid(),
            Url       = request.MediaLinkUrl,
            MimeType  = "image/jpeg",
            Size      = 0,
            Status    = UserStatuses.Active,
            CreatedAt = DateTime.UtcNow
        };

        var mediaLink = new MediaLink
        {
            Id         = Guid.NewGuid(),
            UserId     = userId,
            EntityType = "avatar",
            EntityId   = userId,
            MediaType  = "image",
            SortOrder  = 0,
            MediaId    = media.Id,
            Media      = media
        };
        newUser.MediaLinks.Add(mediaLink);
    }
        await _userRepository.AddAsync(newUser);

        return new ApiResponse<bool> { Success = true, Data = true };
    }
    public async Task<ApiResponse<bool>> LogoutAsync(Guid userId)
{
    var user = await _userRepository.GetUserByIdAsync(userId);
    if (user == null)
        throw new Exception("User not found");

    user.RefreshToken = null!;
    user.RefreshTokenExpiryTime = null;
    await _userRepository.UpdateAsync(user);

    return new ApiResponse<bool>
    {
        Success = true,
        Message = "Đăng xuất thành công",
        Data = true
    };
}

    public async Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
            throw new Exception("User not found");

        if (string.IsNullOrWhiteSpace(request.UserName))
            throw new Exception("User name is required");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new Exception("Email is required");

        var exists = await _userRepository.ExistsByEmailAsync(request.Email, userId);
        if (exists)
            throw new Exception("Email already in use");

        user.UserName = request.UserName.Trim();
        user.Email = request.Email.Trim();
        user.DateOfBirth = request.DateOfBirth;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.Password = PasswordHelper.HashPassword(request.Password);
        }

        await _userRepository.UpdateAsync(user);

        user = await _userRepository.GetUserByIdAsync(userId);

        var profile = new UserProfileDto
        {
            Id = user.Id,
            RoleId = user.RoleId,
            GoogleUserId = user.GoogleUserId,
            Email = user.Email,
            UserName = user.UserName,
            DateOfBirth = user.DateOfBirth,
            Password = user.Password,
            Status = user.Status,
            RefreshTokenExpiryTime = user.RefreshTokenExpiryTime,
            CreatedAt = user.CreatedAt,
            MediaLinkUrl = user.MediaLinks
                .Where(m => m.EntityType == "avatar")
                .OrderBy(m => m.SortOrder)
                .Select(m => m.Media.Url)
                .FirstOrDefault() ?? string.Empty
        };

        return new ApiResponse<UserProfileDto>
        {
            Success = true,
            Message = "Profile updated successfully",
            Data = profile
        };
    }

    private async Task<ApiResponse<LoginResponse>> BuildLoginResponseAsync(User user)
    {
        user = await _userRepository.GetUserByIdAsync(user.Id);
        var accessToken = _jwtService.GenerateToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken(user);
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(3);
        var accessTokenExpiry = DateTime.UtcNow.AddMinutes(5);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = refreshTokenExpiry;
        await _userRepository.UpdateAsync(user);

        return new ApiResponse<LoginResponse>
        {
            Success = true,
            Message = "Login successful",
            MetaData = null,
            Data = new LoginResponse
            {
                accessToken = accessToken,
                refreshToken = refreshToken,
                expiresAt = accessTokenExpiry,
                RefreshTokenExpiryTime = refreshTokenExpiry
            }
        };
    }
}
