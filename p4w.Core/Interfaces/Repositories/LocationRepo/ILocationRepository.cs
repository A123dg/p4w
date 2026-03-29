

using p4w.Core.Dtos.Comment;
using p4w.Core.Dtos.Location;
using p4w.Core.Dtos.Review;
using p4w.Core.Models;
using p4w.Core.Paginations;

namespace p4w.Core.Interfaces.Repositories.LocationRepo;

public interface ILocationRepository
{
    Task<PagedResult<LocationCardDto>> GetLocationsAsync(string? search, int? type, int page, int pageSize);
    Task<LocationDetailDto?> GetLocationDetailAsync(Guid locationId);
    Task<PagedResult<ReviewDto>> GetLocationReviewsAsync(Guid locationId, int page, int pageSize);
    Task<PagedResult<CommentDto>> GetReviewCommentsAsync(Guid reviewId, int page, int pageSize);
    Task<CommentDto?> GetCommentDetailAsync(Guid commentId);
    Task<Location?> GetLocationEntityAsync(Guid locationId);
    Task<Review?> GetReviewEntityAsync(Guid reviewId);
    Task<Comment?> GetCommentEntityAsync(Guid commentId);
    Task<Location?> GetLocationEntityForAdminAsync(Guid locationId);
    Task<Review?> GetReviewEntityForAdminAsync(Guid reviewId);
    Task AddReviewAsync(Review review);
    Task UpdateReviewAsync(Review review);
    Task AddCommentAsync(Comment comment);
    Task<PagedResult<AdminLocationDto>> GetAdminLocationsAsync(string? search, int? type, int? status, int page, int pageSize);
    Task<PagedResult<AdminReviewDto>> GetAdminReviewsAsync(string? search, int? status, int? minRating, int page, int pageSize);
    Task<AdminLocationDto?> GetAdminLocationDetailAsync(Guid locationId);
    Task<AdminReviewDto?> GetAdminReviewDetailAsync(Guid reviewId);
    Task AddLocationAsync(p4w.Core.Models.Location location);
    Task UpdateLocationAsync(p4w.Core.Models.Location location);
}
