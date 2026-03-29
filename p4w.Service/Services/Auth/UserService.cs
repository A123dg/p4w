using Microsoft.Extensions.Configuration;
using p4w.Core.Constants.Statuses;
using p4w.Core.Dtos.User;
using p4w.Core.Interfaces.Repositories.Auth;
using p4w.Core.Interfaces.Repositories.MediaRepo;
using p4w.Core.Models;
using p4w.Core.Paginations;

namespace p4w.Core.Interfaces.Services.Auth
{

    
    public class UserService : IUserService 
    {
        private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;

    private readonly IMediaRepository _mediaRepository;
    public UserService(IConfiguration configuration, IUserRepository userRepository, IMediaRepository mediaRepository)
    {
        _configuration = configuration;
        _userRepository = userRepository;
        _mediaRepository = mediaRepository;

    }   

      public   async Task<UserResponseDto> GetUserByIdAsync(Guid userId)
        {
            User user = await _userRepository.GetUserByIdAsync(userId);
            return MapUserResponse(user);
        }

        async Task<UserResponseDto> IUserService.GetUserByEmailAsync(string email)
        {
            User user = await _userRepository.GetUserByEmailAsync(email);
            return MapUserResponse(user);
        }

        public async Task<UserProfileDto> GetUserProfileAsync(Guid userId)
{
    User user = await _userRepository.GetUserByIdAsync(userId);
    RecentLocationDto? recentLocation = await _userRepository.GetRecentLocationByUserIdAsync(userId);

    return new UserProfileDto
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
            .FirstOrDefault() ?? "",
        RecentLocation = recentLocation,
        OwnedLocations = user.OwnedLocations
            .OrderBy(x => x.LocationName)
            .Select(x => new OwnedLocationDto
            {
                Id = x.Id,
                LocationName = x.LocationName,
                Address = x.Address,
                Status = x.Status,
                StatusName = x.Status == LocationStatuses.Pending
                    ? "pending"
                    : x.Status == LocationStatuses.Approved
                        ? "approved"
                        : x.Status == LocationStatuses.Rejected
                            ? "rejected"
                            : x.Status == LocationStatuses.Active
                                ? "active"
                                : "inactive"
            })
            .ToList()
    };
}

        public async Task<RecentLocationDto?> GetRecentLocationAsync(Guid userId)
        {
            return await _userRepository.GetRecentLocationByUserIdAsync(userId);
        }

        public async Task<PagedResult<UserResponseDto>> GetUsersAsync(string? search, Guid? roleId, int? status, int page, int pageSize)
        {
            return await _userRepository.GetUsersAsync(search, roleId, status, page, pageSize);
        }

        public async Task<UserResponseDto> GetAdminUserByIdAsync(Guid userId)
        {
            var user = await _userRepository.GetAdminUserByIdAsync(userId);
            return MapUserResponse(user);
        }

        public async Task<UserResponseDto> CreateAdminUserAsync(AdminUpsertUserRequest request)
        {
            var exists = await _userRepository.ExistsByEmailAsync(request.Email);
            if (exists)
            {
                throw new Exception("Email already in use");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                UserName = request.UserName,
                DateOfBirth = request.DateOfBirth,
                CreatedAt = DateTime.UtcNow,
                Status = request.Status,
                RoleId = request.RoleId
            };

            if (!string.IsNullOrWhiteSpace(request.MediaLinkUrl))
            {
                var media = new Media
                {
                    Id = Guid.NewGuid(),
                    Url = request.MediaLinkUrl,
                    MimeType = "image/jpeg",
                    Size = 0,
                    Status = UserStatuses.Active,
                    CreatedAt = DateTime.UtcNow
                };

                user.MediaLinks.Add(new MediaLink
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    EntityType = "avatar",
                    EntityId = user.Id,
                    MediaType = "image",
                    SortOrder = 0,
                    MediaId = media.Id,
                    Media = media
                });
            }

            await _userRepository.AddAsync(user);
            return (await _userRepository.GetUsersAsync(user.Email, null, null, 1, 1)).Items.First(x => x.Id == user.Id);
        }

        public async Task<UserResponseDto> UpdateAdminUserAsync(Guid userId, AdminUpsertUserRequest request)
        {
            var user = await _userRepository.GetAdminUserByIdAsync(userId);
            var exists = await _userRepository.ExistsByEmailAsync(request.Email, userId);
            if (exists)
            {
                throw new Exception("Email already in use");
            }

            user.Email = request.Email;
            user.UserName = request.UserName;
            user.DateOfBirth = request.DateOfBirth;
            user.RoleId = request.RoleId;
            user.Status = request.Status;

            if (!string.IsNullOrWhiteSpace(request.MediaLinkUrl))
            {
                var existingAvatarLink = user.MediaLinks.FirstOrDefault(m => m.EntityType == "avatar");
                if (existingAvatarLink != null)
                {
                    existingAvatarLink.Media.Url = request.MediaLinkUrl;
                    await _mediaRepository.UpdateAsync(existingAvatarLink.Media);
                }
            }

            await _userRepository.UpdateAsync(user);
            return (await _userRepository.GetUsersAsync(user.Email, null, null, 1, 1)).Items.First(x => x.Id == user.Id);
        }

        public async Task<UserResponseDto> LockUserAsync(Guid userId)
        {
            var user = await _userRepository.GetAdminUserByIdAsync(userId);
            user.Status = UserStatuses.Locked;
            await _userRepository.UpdateAsync(user);
            return (await _userRepository.GetUsersAsync(user.Email, null, null, 1, 1)).Items.First(x => x.Id == user.Id);
        }

        public async Task<UserResponseDto> UnlockUserAsync(Guid userId)
        {
            var user = await _userRepository.GetAdminUserByIdAsync(userId);
            user.Status = UserStatuses.Active;
            await _userRepository.UpdateAsync(user);
            return (await _userRepository.GetUsersAsync(user.Email, null, null, 1, 1)).Items.First(x => x.Id == user.Id);
        }

        private static UserResponseDto MapUserResponse(User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                RoleId = user.RoleId,
                RoleName = user.Role?.Name ?? string.Empty,
                Status = user.Status,
                StatusName = user.Status == UserStatuses.Active
                    ? "active"
                    : user.Status == UserStatuses.Locked
                        ? "locked"
                        : "inactive",
                DateOfBirth = user.DateOfBirth,
                MediaLinkUrl = user.MediaLinks
                    .Where(m => m.EntityType == "avatar")
                    .OrderBy(m => m.SortOrder)
                    .Select(m => m.Media.Url)
                    .FirstOrDefault() ?? string.Empty,
                CreatedAt = user.CreatedAt,
                OwnedLocations = user.OwnedLocations
                    .OrderBy(x => x.LocationName)
                    .Select(x => new OwnedLocationDto
                    {
                        Id = x.Id,
                        LocationName = x.LocationName,
                        Address = x.Address,
                        Status = x.Status,
                        StatusName = x.Status == LocationStatuses.Pending
                            ? "pending"
                            : x.Status == LocationStatuses.Approved
                                ? "approved"
                                : x.Status == LocationStatuses.Rejected
                                    ? "rejected"
                                    : x.Status == LocationStatuses.Active
                                        ? "active"
                                        : "inactive"
                    })
                    .ToList()
            };
        }

        public async Task CreateUserAsync(UserDto userCreateDto)
        {
            User user = await _userRepository.GetUserByEmailAsync(userCreateDto.Email);
            if(user != null) {
                throw new Exception("User with this email already exists");
            }
            User createUser = new User
            {
                Id = Guid.NewGuid(),
                Email = userCreateDto.Email,
                UserName = userCreateDto.UserName,
                DateOfBirth = userCreateDto.DateOfBirth,
                CreatedAt = DateTime.UtcNow,
                Status = UserStatuses.Active,
                RoleId = Guid.Parse("8ACEA62A-E03E-47B9-89E5-9E4320085D7D")
            };
             await _userRepository.AddAsync(createUser);
            // return Task.CompletedTask;

        }

        public async Task UpdateUserAsync(Guid userId, UserDto userUpdateDto)
        {
            User user = await _userRepository.GetUserByIdAsync(userId);
            if(user == null) {
                throw new Exception("User not found");
            }
            user.Email = userUpdateDto.Email;
            user.UserName = userUpdateDto.UserName;
            user.DateOfBirth = userUpdateDto.DateOfBirth;
             if (!string.IsNullOrEmpty(userUpdateDto.mediaLinkUrl))
    {
        var existingAvatarLink = user.MediaLinks
            .FirstOrDefault(m => m.EntityType == "avatar");

        if (existingAvatarLink != null)
        {
            // Đã có avatar → update URL trong Media
            existingAvatarLink.Media.Url = userUpdateDto.mediaLinkUrl;
            await _mediaRepository.UpdateAsync(existingAvatarLink.Media);
        }
        else
        {
            var newMedia = new Media
            {
                Id = Guid.NewGuid(),
                Url = userUpdateDto.mediaLinkUrl,
                MimeType = "image/jpeg",
                Size = 0,
                Status = UserStatuses.Active,
                CreatedAt = DateTime.UtcNow
            };

            var newMediaLink = new MediaLink
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EntityType = "avatar",
                EntityId = userId,
                MediaType = "image",
                SortOrder = 0,
                MediaId = newMedia.Id,
                Media = newMedia
            };

            user.MediaLinks.Add(newMediaLink);
        }
    }
    await _userRepository.UpdateAsync(user);

        }
        

        public async Task DeleteUserAsync(Guid userId)
        {
            User user = await _userRepository.GetUserByIdAsync(userId);
            user.Status = UserStatuses.Inactive;
            await _userRepository.UpdateAsync(user);
        }
    }
}
