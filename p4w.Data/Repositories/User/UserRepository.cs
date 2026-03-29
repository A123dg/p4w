using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using p4w.Core.Constants.Statuses;
using p4w.Core.Dtos.User;
using p4w.Core.Exceptions;
using p4w.Core.Interfaces.Repositories.Auth;
using p4w.Core.Models;
using p4w.Core.Paginations;
using p4w.Data.Persistence;
public class UserRepository : IUserRepository {
    private readonly AppDbContext _context;
    public UserRepository(AppDbContext context) {
        _context = context;
    }
    public async Task<bool> ExistsByEmailAsync(string email) {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }
    public async Task<bool> ExistsByEmailAsync(string email, Guid excludeUserId) {
        return await _context.Users.AnyAsync(u => u.Email == email && u.Id != excludeUserId);
    }
    public async Task AddAsync(User user) {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }
    public async Task<User> GetUserByGoogleUserIdAsync(string googleUserId) {
        User? user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.MediaLinks)
            .ThenInclude(m => m.Media)
            .Include(u => u.OwnedLocations)
            .FirstOrDefaultAsync(u => u.GoogleUserId == googleUserId && u.Status != UserStatuses.Inactive);
        // if (user == null) {
        //     throw new AppException("User not found", ErrorCodes.NotFound, StatusCodes.Status404NotFound);
        // }
        return user;
    }
    public async Task<User> GetUserByUserNameAsync(string userName) {
        User? user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.MediaLinks)
            .ThenInclude(m => m.Media)
            .FirstOrDefaultAsync(u => u.UserName == userName && u.Status != UserStatuses.Inactive);
        // if (user == null) {
        //     throw new AppException("User not found", ErrorCodes.NotFound, StatusCodes.Status404NotFound);
        // }
        return user;
    }
    public async Task UpdateAsync(User user) {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
    public async Task<User> GetUserByEmailAsync(string email) {
        User? user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.MediaLinks)
            .ThenInclude(m => m.Media)
            .Include(u => u.OwnedLocations)
            .FirstOrDefaultAsync(u => u.Email == email && u.Status != UserStatuses.Inactive);
        // if (user == null) {
        //     throw new AppException("User not found", ErrorCodes.NotFound, StatusCodes.Status404NotFound);
        // }
        return user;
    }
    public async Task<User> GetUserByIdAsync(Guid id) {
        User? user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.MediaLinks)
            .ThenInclude(m => m.Media)
            .Include(u => u.OwnedLocations)
            .FirstOrDefaultAsync(u => u.Id == id && u.Status == UserStatuses.Active);
        if (user == null) {
            throw new AppException("User not found", ErrorCodes.NotFound, StatusCodes.Status404NotFound);
        }
        return user;
    }

    public async Task<User> GetAdminUserByIdAsync(Guid id) {
        User? user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.MediaLinks)
            .ThenInclude(m => m.Media)
            .Include(u => u.OwnedLocations)
            .FirstOrDefaultAsync(u => u.Id == id && u.Status != UserStatuses.Inactive);
        if (user == null) {
            throw new AppException("User not found", ErrorCodes.NotFound, StatusCodes.Status404NotFound);
        }
        return user;
    }

    public async Task<RecentLocationDto?> GetRecentLocationByUserIdAsync(Guid userId)
    {
        var recentReviewInteraction = await _context.Reviews
            .Where(x => x.UserId == userId && x.Status == ReviewStatuses.Active)
            .Select(x => new
            {
                x.LocationId,
                x.CreatedAt,
                InteractionType = "review"
            })
            .ToListAsync();

        var recentCommentInteraction = await _context.Comments
            .Where(x => x.UserId == userId && x.Status == CommentStatuses.Active)
            .Join(
                _context.Reviews.Where(x => x.Status == ReviewStatuses.Active),
                comment => comment.ReviewId,
                review => review.Id,
                (comment, review) => new
                {
                    review.LocationId,
                    comment.CreatedAt,
                    InteractionType = "comment"
                })
            .ToListAsync();

        var recentInteraction = recentReviewInteraction
            .Concat(recentCommentInteraction)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault();

        if (recentInteraction == null)
        {
            return null;
        }

        var location = await _context.Locations
            .Include(x => x.Reviews)
            .FirstOrDefaultAsync(x => x.Id == recentInteraction.LocationId && x.Status == LocationStatuses.Active);

        if (location == null)
        {
            return null;
        }

        return new RecentLocationDto
        {
            Id = location.Id,
            LocationName = location.LocationName,
            Description = location.Description,
            Address = location.Address,
            AddressLink = location.AddressLink,
            OpeningHours = location.OpeningHours.HasValue ? location.OpeningHours.Value.ToString(@"hh\:mm\:ss") : null,
            ClosingHours = location.ClosingHours.HasValue ? location.ClosingHours.Value.ToString(@"hh\:mm\:ss") : null,
            AverageRating = location.Reviews.Where(x => x.Status == ReviewStatuses.Active).Any() ? Math.Round(location.Reviews.Where(x => x.Status == ReviewStatuses.Active).Average(r => r.Rating), 1) : 0,
            ReviewCount = location.Reviews.Count(x => x.Status == ReviewStatuses.Active),
            LastInteractionAt = recentInteraction.CreatedAt,
            LastInteractionType = recentInteraction.InteractionType
        };
    }

    public async Task<PagedResult<UserResponseDto>> GetUsersAsync(string? search, Guid? roleId, int? status, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;

        IQueryable<User> query = _context.Users
            .Include(x => x.Role)
            .Include(x => x.OwnedLocations)
            .Where(x => x.Status != UserStatuses.Inactive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
query = query.Where(x => 
    EF.Functions.Collate(x.UserName, "Latin1_General_CI_AI").Contains(normalizedSearch) || 
    EF.Functions.Collate(x.Email, "Latin1_General_CI_AI").Contains(normalizedSearch));        }

        if (roleId.HasValue)
        {
            query = query.Where(x => x.RoleId == roleId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UserResponseDto
            {
                Id = x.Id,
                UserName = x.UserName,
                Email = x.Email,
                RoleId = x.RoleId,
                RoleName = x.Role.Name,
                Status = x.Status,
                StatusName = x.Status == UserStatuses.Active
                    ? "active"
                    : x.Status == UserStatuses.Locked
                        ? "locked"
                        : "inactive",
                DateOfBirth = x.DateOfBirth,
                MediaLinkUrl = x.MediaLinks
                    .Where(m => m.EntityType == "avatar")
                    .OrderBy(m => m.SortOrder)
                    .Select(m => m.Media.Url)
                    .FirstOrDefault() ?? string.Empty,
                CreatedAt = x.CreatedAt,
                OwnedLocations = x.OwnedLocations
                    .OrderBy(l => l.LocationName)
                    .Select(l => new OwnedLocationDto
                    {
                        Id = l.Id,
                        LocationName = l.LocationName,
                        Address = l.Address,
                        Status = l.Status,
                        StatusName = l.Status == LocationStatuses.Pending
                            ? "pending"
                            : l.Status == LocationStatuses.Approved
                                ? "approved"
                                : l.Status == LocationStatuses.Rejected
                                    ? "rejected"
                                    : l.Status == LocationStatuses.Active
                                        ? "active"
                                        : "inactive"
                    })
                    .ToList()
            })
            .ToListAsync();

        return new PagedResult<UserResponseDto>
        {
            Items = items,
            MetaData = new MetaData
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                TotalPage = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize)
            }
        };
    }
}
