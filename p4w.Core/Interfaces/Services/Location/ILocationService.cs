using p4w.Core.Dtos.Comment;
using p4w.Core.Dtos.Location;
using p4w.Core.Dtos.Review;
using p4w.Core.Paginations;

namespace p4w.Core.Interfaces.Services.Location;

public interface ILocationService
{
    Task<PagedResult<LocationCardDto>> GetLocationsAsync(string? search, int? type, int page, int pageSize);
    Task<LocationDetailDto> GetLocationDetailAsync(Guid locationId);
    Task<PagedResult<ReviewDto>> GetLocationReviewsAsync(Guid locationId, int page, int pageSize);
    Task<PagedResult<CommentDto>> GetReviewCommentsAsync(Guid reviewId, int page, int pageSize);
    Task<AdminLocationDto> CreateLocationAsync(Guid userId, CreateLocationRequest request);
    Task<ReviewDto> CreateReviewAsync(Guid userId, CreateReviewRequest request);
    Task<CommentDto> CreateCommentAsync(Guid userId, CreateCommentRequest request);
    Task<PagedResult<AdminLocationDto>> GetAdminLocationsAsync(string? search, int? type, int? status, int page, int pageSize);
    Task<AdminLocationDto> GetAdminLocationDetailAsync(Guid locationId);
    Task<AdminLocationDto> CreateAdminLocationAsync(AdminUpsertLocationRequest request);
    Task<AdminLocationDto> UpdateAdminLocationAsync(Guid locationId, AdminUpsertLocationRequest request);
    Task<AdminLocationDto> HideAdminLocationAsync(Guid locationId);
    Task<PagedResult<AdminReviewDto>> GetAdminReviewsAsync(string? search, int? status, int? minRating, int page, int pageSize);
    Task<AdminReviewDto> GetAdminReviewDetailAsync(Guid reviewId);
    Task<AdminReviewDto> UpdateAdminReviewStatusAsync(Guid reviewId, AdminUpdateReviewStatusRequest request);
    Task<AdminReviewDto> HideAdminReviewAsync(Guid reviewId);
}
