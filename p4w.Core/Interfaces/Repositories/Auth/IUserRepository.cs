using p4w.Core.Dtos.User;
using p4w.Core.Models;
using p4w.Core.Paginations;

namespace p4w.Core.Interfaces.Repositories.Auth;
public interface IUserRepository {
    Task<bool> ExistsByEmailAsync(string email);
    Task<User> GetUserByGoogleUserIdAsync(string googleUserId);
    Task  AddAsync(User user);
    Task UpdateAsync(User user);

    Task<User> GetUserByIdAsync(Guid id);
    Task<User> GetAdminUserByIdAsync(Guid id);

    Task<User> GetUserByUserNameAsync(string userName);
    Task<User> GetUserByEmailAsync(string email);
    Task<RecentLocationDto?> GetRecentLocationByUserIdAsync(Guid userId);
    Task<PagedResult<UserResponseDto>> GetUsersAsync(string? search, Guid? roleId, int? status, int page, int pageSize);
    Task<bool> ExistsByEmailAsync(string email, Guid excludeUserId);
}
