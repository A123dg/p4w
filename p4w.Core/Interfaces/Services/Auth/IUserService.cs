using p4w.Core.Dtos.User;
using p4w.Core.Paginations;

namespace p4w.Core.Interfaces.Services.Auth
{
    public interface IUserService
    {
        Task<UserResponseDto> GetUserByIdAsync(Guid userId);
        Task<UserResponseDto> GetUserByEmailAsync(string email);
        Task CreateUserAsync(UserDto userCreateDto);
        Task UpdateUserAsync(Guid userId, UserDto userUpdateDto);
        Task DeleteUserAsync(Guid userId);

        Task<UserProfileDto> GetUserProfileAsync(Guid userId);
        Task<RecentLocationDto?> GetRecentLocationAsync(Guid userId);
        Task<PagedResult<UserResponseDto>> GetUsersAsync(string? search, Guid? roleId, int? status, int page, int pageSize);
        Task<UserResponseDto> GetAdminUserByIdAsync(Guid userId);
        Task<UserResponseDto> CreateAdminUserAsync(AdminUpsertUserRequest request);
        Task<UserResponseDto> UpdateAdminUserAsync(Guid userId, AdminUpsertUserRequest request);
        Task<UserResponseDto> LockUserAsync(Guid userId);
        Task<UserResponseDto> UnlockUserAsync(Guid userId);
    }
}
