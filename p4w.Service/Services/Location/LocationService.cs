using Microsoft.AspNetCore.Http;
using p4w.Core.Constants.Statuses;
using p4w.Core.Dtos.Comment;
using p4w.Core.Dtos.Location;
using p4w.Core.Dtos.Review;
using p4w.Core.Exceptions;
using p4w.Core.Interfaces.Repositories.LocationRepo;
using p4w.Core.Interfaces.Services.Location;
using p4w.Core.Models;
using p4w.Core.Paginations;
using System.Globalization;

namespace p4w.Service.Services.Location;

public class LocationService : ILocationService
{
    private readonly ILocationRepository _locationRepository;

    public LocationService(ILocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
    }

    public async Task<PagedResult<LocationCardDto>> GetLocationsAsync(string? search, int? type, int page, int pageSize)
    {
        return await _locationRepository.GetLocationsAsync(search, type, page, pageSize);
    }

    public async Task<LocationDetailDto> GetLocationDetailAsync(Guid locationId)
    {
        var location = await _locationRepository.GetLocationDetailAsync(locationId);
        if (location == null)
        {
            throw new AppException("Location not found", ErrorCodes.NotFound, StatusCodes.Status404NotFound);
        }

        return location;
    }

    public async Task<PagedResult<ReviewDto>> GetLocationReviewsAsync(Guid locationId, int page, int pageSize)
    {
        var location = await _locationRepository.GetLocationEntityAsync(locationId);
        if (location == null)
        {
            throw new AppException("Location not found", ErrorCodes.NotFound, StatusCodes.Status404NotFound);
        }

        return await _locationRepository.GetLocationReviewsAsync(locationId, page, pageSize);
    }

    public async Task<PagedResult<CommentDto>> GetReviewCommentsAsync(Guid reviewId, int page, int pageSize)
    {
        var review = await _locationRepository.GetReviewEntityAsync(reviewId);
        if (review == null)
        {
            throw new AppException("Review not found", ErrorCodes.NotFound, StatusCodes.Status404NotFound);
        }

        return await _locationRepository.GetReviewCommentsAsync(reviewId, page, pageSize);
    }

    public async Task<AdminLocationDto> CreateLocationAsync(Guid userId, CreateLocationRequest request)
    {
        var openingHours = ParseOperatingHours(request.OpeningHours, nameof(request.OpeningHours));
        var closingHours = ParseOperatingHours(request.ClosingHours, nameof(request.ClosingHours));

        var location = new Core.Models.Location
        {
            Id = Guid.NewGuid(),
            OwnerId = userId,
            LocationName = request.LocationName.Trim(),
            Description = request.Description?.Trim(),
            Address = request.Address.Trim(),
            AddressLink = string.IsNullOrWhiteSpace(request.AddressLink) ? null : request.AddressLink.Trim(),
            OpeningHours = openingHours,
            ClosingHours = closingHours,
            Type = request.Type,
            Status = LocationStatuses.Pending
        };

        await _locationRepository.AddLocationAsync(location);
        return await GetAdminLocationDetailAsync(location.Id);
    }

    public async Task<ReviewDto> CreateReviewAsync(Guid userId, CreateReviewRequest request)
    {
        if (request.Rating < 1 || request.Rating > 5)
        {
            throw new AppException("Rating must be between 1 and 5", ErrorCodes.BadRequest, StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new AppException("Review content is required", ErrorCodes.BadRequest, StatusCodes.Status400BadRequest);
        }

        var location = await _locationRepository.GetLocationEntityAsync(request.LocationId);
        if (location == null)
        {
            throw new AppException("Location not found", ErrorCodes.NotFound, StatusCodes.Status404NotFound);
        }

        var review = new Review
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LocationId = request.LocationId,
            Rating = request.Rating,
            Content = request.Content.Trim(),
            CreatedAt = DateTime.UtcNow,
            Status = ReviewStatuses.Active
        };

        await _locationRepository.AddReviewAsync(review);

        var createdReview = await _locationRepository.GetReviewEntityAsync(review.Id);
        return new ReviewDto
        {
            Id = createdReview!.Id,
            UserId = createdReview.UserId,
            UserName = createdReview.User.UserName,
            AvatarUrl = createdReview.User.MediaLinks
                .Where(m => m.EntityType == "avatar")
                .OrderBy(m => m.SortOrder)
                .Select(m => m.Media.Url)
                .FirstOrDefault() ?? string.Empty,
            Rating = createdReview.Rating,
            Content = createdReview.Content,
            CreatedAt = createdReview.CreatedAt,
            CommentCount = createdReview.Comments.Count
        };
    }

    public async Task<CommentDto> CreateCommentAsync(Guid userId, CreateCommentRequest request)
    {
        var review = await _locationRepository.GetReviewEntityAsync(request.ReviewId);
        if (review == null)
        {
            throw new AppException("Review not found", ErrorCodes.NotFound, StatusCodes.Status404NotFound);
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new AppException("Comment content is required", ErrorCodes.BadRequest, StatusCodes.Status400BadRequest);
        }

        if (request.ParentId.HasValue)
        {
            var parentComment = await _locationRepository.GetCommentEntityAsync(request.ParentId.Value);
            if (parentComment == null || parentComment.ReviewId != request.ReviewId)
            {
                throw new AppException("Parent comment is invalid", ErrorCodes.BadRequest, StatusCodes.Status400BadRequest);
            }
        }

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            ReviewId = request.ReviewId,
            UserId = userId,
            ParentId = request.ParentId,
            Content = request.Content.Trim(),
            CreatedAt = DateTime.UtcNow,
            Status = CommentStatuses.Active
        };

        await _locationRepository.AddCommentAsync(comment);

        var createdComment = await _locationRepository.GetCommentDetailAsync(comment.Id);
        return createdComment!;
    }

    public async Task<PagedResult<AdminLocationDto>> GetAdminLocationsAsync(string? search, int? type, int? status, int page, int pageSize)
    {
        return await _locationRepository.GetAdminLocationsAsync(search, type, status, page, pageSize);
    }

    public async Task<AdminLocationDto> GetAdminLocationDetailAsync(Guid locationId)
    {
        var location = await _locationRepository.GetAdminLocationDetailAsync(locationId);
        if (location == null)
        {
            throw new AppException("Location not found", ErrorCodes.NotFound, StatusCodes.Status404NotFound);
        }

        return location;
    }

    public async Task<AdminLocationDto> CreateAdminLocationAsync(AdminUpsertLocationRequest request)
    {
        if (request.Status is not (LocationStatuses.Inactive or LocationStatuses.Pending or LocationStatuses.Approved or LocationStatuses.Rejected or LocationStatuses.Active))
        {
            throw new AppException("Location status is invalid", ErrorCodes.BadRequest, StatusCodes.Status400BadRequest);
        }

        var openingHours = ParseOperatingHours(request.OpeningHours, nameof(request.OpeningHours));
        var closingHours = ParseOperatingHours(request.ClosingHours, nameof(request.ClosingHours));

        var location = new Core.Models.Location
        {
            Id = Guid.NewGuid(),
            OwnerId = request.OwnerId,
            LocationName = request.LocationName.Trim(),
            Description = request.Description?.Trim(),
            Address = request.Address.Trim(),
            AddressLink = string.IsNullOrWhiteSpace(request.AddressLink) ? null : request.AddressLink.Trim(),
            OpeningHours = openingHours,
            ClosingHours = closingHours,
            Type = request.Type,
            Status = request.Status
        };

        await _locationRepository.AddLocationAsync(location);
        return (await _locationRepository.GetAdminLocationsAsync(location.LocationName, null, null, 1, 1)).Items.First(x => x.Id == location.Id);
    }

    public async Task<AdminLocationDto> UpdateAdminLocationAsync(Guid locationId, AdminUpsertLocationRequest request)
    {
        if (request.Status is not (LocationStatuses.Inactive or LocationStatuses.Pending or LocationStatuses.Approved or LocationStatuses.Rejected or LocationStatuses.Active))
        {
            throw new AppException("Location status is invalid", ErrorCodes.BadRequest, StatusCodes.Status400BadRequest);
        }

        var openingHours = ParseOperatingHours(request.OpeningHours, nameof(request.OpeningHours));
        var closingHours = ParseOperatingHours(request.ClosingHours, nameof(request.ClosingHours));

        var entity = await _locationRepository.GetLocationEntityForAdminAsync(locationId);
        if (entity == null)
        {
            throw new AppException("Location not found", ErrorCodes.NotFound, StatusCodes.Status404NotFound);
        }

        entity.LocationName = request.LocationName.Trim();
        entity.OwnerId = request.OwnerId;
        entity.Description = request.Description?.Trim();
        entity.Address = request.Address.Trim();
        entity.AddressLink = string.IsNullOrWhiteSpace(request.AddressLink) ? null : request.AddressLink.Trim();
        entity.OpeningHours = openingHours;
        entity.ClosingHours = closingHours;
        entity.Type = request.Type;
        entity.Status = request.Status;

        await _locationRepository.UpdateLocationAsync(entity);
        return await GetAdminLocationDetailAsync(entity.Id);
    }

    public async Task<AdminLocationDto> HideAdminLocationAsync(Guid locationId)
    {
        var entity = await _locationRepository.GetLocationEntityForAdminAsync(locationId);
        if (entity == null)
        {
            throw new AppException("Location not found", ErrorCodes.NotFound, StatusCodes.Status404NotFound);
        }

        entity.Status = LocationStatuses.Inactive;
        await _locationRepository.UpdateLocationAsync(entity);
        return await GetAdminLocationDetailAsync(entity.Id);
    }

    public async Task<PagedResult<AdminReviewDto>> GetAdminReviewsAsync(string? search, int? status, int? minRating, int page, int pageSize)
    {
        return await _locationRepository.GetAdminReviewsAsync(search, status, minRating, page, pageSize);
    }

    public async Task<AdminReviewDto> GetAdminReviewDetailAsync(Guid reviewId)
    {
        var review = await _locationRepository.GetAdminReviewDetailAsync(reviewId);
        if (review == null)
        {
            throw new AppException("Review not found", ErrorCodes.NotFound, StatusCodes.Status404NotFound);
        }

        return review;
    }

    public async Task<AdminReviewDto> UpdateAdminReviewStatusAsync(Guid reviewId, AdminUpdateReviewStatusRequest request)
    {
        if (request.Status is not (ReviewStatuses.Inactive or ReviewStatuses.Active))
        {
            throw new AppException("Review status is invalid", ErrorCodes.BadRequest, StatusCodes.Status400BadRequest);
        }

        var review = await _locationRepository.GetReviewEntityForAdminAsync(reviewId);
        if (review == null)
        {
            throw new AppException("Review not found", ErrorCodes.NotFound, StatusCodes.Status404NotFound);
        }

        review.Status = request.Status;
        await _locationRepository.UpdateReviewAsync(review);
        return await GetAdminReviewDetailAsync(reviewId);
    }

    public async Task<AdminReviewDto> HideAdminReviewAsync(Guid reviewId)
    {
        var review = await _locationRepository.GetReviewEntityForAdminAsync(reviewId);
        if (review == null)
        {
            throw new AppException("Review not found", ErrorCodes.NotFound, StatusCodes.Status404NotFound);
        }

        review.Status = ReviewStatuses.Inactive;
        await _locationRepository.UpdateReviewAsync(review);
        return await GetAdminReviewDetailAsync(reviewId);
    }

    private static TimeSpan? ParseOperatingHours(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (TimeSpan.TryParseExact(value.Trim(), @"hh\:mm\:ss", CultureInfo.InvariantCulture, out var parsedValue))
        {
            return parsedValue;
        }

        throw new AppException($"{fieldName} must be in hh:mm:ss format", ErrorCodes.BadRequest, StatusCodes.Status400BadRequest);
    }
}
