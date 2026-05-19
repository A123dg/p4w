using Microsoft.AspNetCore.Http;
using p4w.Core.Constants;
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
            throw new AppException(MessageConstant.LocationMessage.LOCATION_NOT_FOUND, ErrorCodes.NotFound, StatusCodes.Status404NotFound);
        }

        return location;
    }

    public async Task<LocationDetailDto?> GetLocationDetailForOwnerAsync(Guid locationId)
    {
        return await _locationRepository.GetLocationDetailForAdminAsync(locationId);
    }

    public async Task<PagedResult<ReviewDto>> GetLocationReviewsAsync(Guid locationId, int page, int pageSize)
    {
        var location = await _locationRepository.GetLocationEntityAsync(locationId);
        if (location == null)
        {
            throw new AppException(MessageConstant.LocationMessage.LOCATION_NOT_FOUND, ErrorCodes.NotFound, StatusCodes.Status404NotFound);
        }

        return await _locationRepository.GetLocationReviewsAsync(locationId, page, pageSize);
    }

    public async Task<PagedResult<CommentDto>> GetReviewCommentsAsync(Guid reviewId, int page, int pageSize)
    {
        var review = await _locationRepository.GetReviewEntityAsync(reviewId);
        if (review == null)
        {
            throw new AppException(MessageConstant.ReviewMessage.REVIEW_NOT_FOUND, ErrorCodes.NotFound, StatusCodes.Status404NotFound);
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
            CreatedAt = DateTime.UtcNow,
            Status = LocationStatuses.Pending
        };

        await _locationRepository.AddLocationAsync(location);
        await _locationRepository.AddLocationMediaAsync(userId, location.Id, request.MediaLinkUrls);
        return await GetAdminLocationDetailAsync(location.Id);
    }

    public async Task<AdminLocationDto> RequestLocationUpdateAsync(Guid userId, Guid locationId, UpdateLocationRequest request)
    {
        var entity = await _locationRepository.GetLocationEntityForAdminAsync(locationId);
        if (entity == null)
        {
            throw new AppException(MessageConstant.LocationMessage.LOCATION_NOT_FOUND, ErrorCodes.NotFound, StatusCodes.Status404NotFound);
        }

        if (entity.OwnerId != userId)
        {
            throw new AppException(MessageConstant.LocationMessage.LOCATION_UPDATE_ACCESS_DENIED, ErrorCodes.Forbidden, StatusCodes.Status403Forbidden);
        }

        if (request.Status.HasValue)
        {
            if (request.Status is not (LocationStatuses.Inactive or LocationStatuses.Pending or LocationStatuses.Approved or LocationStatuses.Rejected or LocationStatuses.Active))
            {
                throw new AppException(MessageConstant.LocationMessage.LOCATION_STATUS_INVALID, ErrorCodes.BadRequest, StatusCodes.Status400BadRequest);
            }
        }

        if (request.Status == LocationStatuses.Inactive)
        {
            UpdateLocationStatusWithHistory(entity, LocationStatuses.Inactive);
            ClearPendingUpdate(entity);
            await _locationRepository.UpdateLocationAsync(entity);
            await _locationRepository.ClearLocationMediaAsync(entity.Id, "location-pending");
            return await GetAdminLocationDetailAsync(entity.Id);
        }

        if (entity.Status == LocationStatuses.Locked || entity.Status == LocationStatuses.Rejected)
        {
            throw new AppException(MessageConstant.LocationMessage.INACTIVE_LOCATION_CANNOT_BE_UPDATED, ErrorCodes.BadRequest, StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(request.LocationName))
        {
            throw new AppException(MessageConstant.LocationMessage.LOCATION_NAME_REQUIRED, ErrorCodes.BadRequest, StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(request.Address))
        {
            throw new AppException(MessageConstant.LocationMessage.ADDRESS_REQUIRED, ErrorCodes.BadRequest, StatusCodes.Status400BadRequest);
        }

        if (!request.Type.HasValue)
        {
            throw new AppException(MessageConstant.LocationMessage.TYPE_REQUIRED, ErrorCodes.BadRequest, StatusCodes.Status400BadRequest);
        }

        var openingHours = ParseOperatingHours(request.OpeningHours, nameof(request.OpeningHours));
        var closingHours = ParseOperatingHours(request.ClosingHours, nameof(request.ClosingHours));

        if (entity.Status == LocationStatuses.Pending)
        {
            entity.LocationName = request.LocationName.Trim();
            entity.Description = request.Description?.Trim();
            entity.Address = request.Address.Trim();
            entity.AddressLink = string.IsNullOrWhiteSpace(request.AddressLink) ? null : request.AddressLink.Trim();
            entity.OpeningHours = openingHours;
            entity.ClosingHours = closingHours;
            entity.Type = request.Type.Value;

            await _locationRepository.UpdateLocationAsync(entity);
            await _locationRepository.ReplaceLocationMediaAsync(userId, entity.Id, request.MediaLinkUrls, "location");
            return await GetAdminLocationDetailAsync(entity.Id);
        }

        entity.HasPendingUpdate = true;
        entity.PendingLocationName = request.LocationName.Trim();
        entity.PendingDescription = request.Description?.Trim();
        entity.PendingAddress = request.Address.Trim();
        entity.PendingAddressLink = string.IsNullOrWhiteSpace(request.AddressLink) ? null : request.AddressLink.Trim();
        entity.PendingOpeningHours = openingHours;
        entity.PendingClosingHours = closingHours;
        entity.PendingType = request.Type.Value;
        entity.PendingUpdatedAt = DateTime.UtcNow;

        await _locationRepository.UpdateLocationAsync(entity);
        await _locationRepository.ReplaceLocationMediaAsync(userId, entity.Id, request.MediaLinkUrls, "location-pending");
        return await GetAdminLocationDetailAsync(entity.Id);
    }

    public async Task<ReviewDto> CreateReviewAsync(Guid userId, CreateReviewRequest request)
    {
        if (request.Rating < 1 || request.Rating > 5)
        {
            throw new AppException(MessageConstant.ReviewMessage.RATING_INVALID, ErrorCodes.BadRequest, StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new AppException(MessageConstant.ReviewMessage.REVIEW_CONTENT_REQUIRED, ErrorCodes.BadRequest, StatusCodes.Status400BadRequest);
        }

        var normalizedReviewMediaUrls = request.MediaLinkUrls?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct()
            .ToList() ?? [];

        if (normalizedReviewMediaUrls.Count > 3)
        {
            throw new AppException(MessageConstant.ReviewMessage.REVIEW_MAX_IMAGES, ErrorCodes.BadRequest, StatusCodes.Status400BadRequest);
        }

        var location = await _locationRepository.GetLocationEntityAsync(request.LocationId);
        if (location == null)
        {
            throw new AppException(MessageConstant.LocationMessage.LOCATION_NOT_FOUND, ErrorCodes.NotFound, StatusCodes.Status404NotFound);
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
        await _locationRepository.AddReviewMediaAsync(userId, review.Id, normalizedReviewMediaUrls);

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
            CommentCount = createdReview.Comments.Count,
            MediaLinkUrls = normalizedReviewMediaUrls
        };
    }

    public async Task<CommentDto> CreateCommentAsync(Guid userId, CreateCommentRequest request)
    {
        var review = await _locationRepository.GetReviewEntityAsync(request.ReviewId);
        if (review == null)
        {
            throw new AppException(MessageConstant.ReviewMessage.REVIEW_NOT_FOUND, ErrorCodes.NotFound, StatusCodes.Status404NotFound);
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new AppException(MessageConstant.CommentMessage.COMMENT_CONTENT_REQUIRED, ErrorCodes.BadRequest, StatusCodes.Status400BadRequest);
        }

        if (!string.IsNullOrWhiteSpace(request.MediaLinkUrl) && request.MediaLinkUrl.Trim().Length == 0)
        {
            request.MediaLinkUrl = null;
        }

        if (request.ParentId.HasValue)
        {
            var parentComment = await _locationRepository.GetCommentEntityAsync(request.ParentId.Value);
            if (parentComment == null || parentComment.ReviewId != request.ReviewId)
            {
                throw new AppException(MessageConstant.CommentMessage.PARENT_COMMENT_INVALID, ErrorCodes.BadRequest, StatusCodes.Status400BadRequest);
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
        await _locationRepository.AddCommentMediaAsync(userId, comment.Id, request.MediaLinkUrl);

        var createdComment = await _locationRepository.GetCommentDetailAsync(comment.Id);
        return createdComment!;
    }

    public async Task<PagedResult<AdminLocationDto>> GetAdminLocationsAsync(string? search, int? type, int? status, int page, int pageSize)
    {
        return await _locationRepository.GetAdminLocationsAsync(search, type, status, page, pageSize);
    }

    public async Task<PagedResult<AdminLocationDto>> GetOwnerLocationsAsync(Guid ownerId, int page, int pageSize)
    {
        return await _locationRepository.GetAdminLocationsByOwnerAsync(ownerId, page, pageSize);
    }

    public async Task<AdminLocationDto> GetAdminLocationDetailAsync(Guid locationId)
    {
        var location = await _locationRepository.GetAdminLocationDetailAsync(locationId);
        if (location == null)
        {
            throw new AppException(MessageConstant.LocationMessage.LOCATION_NOT_FOUND, ErrorCodes.NotFound, StatusCodes.Status404NotFound);
        }

        return location;
    }

    public async Task<AdminLocationDto> CreateAdminLocationAsync(AdminUpsertLocationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.LocationName))
        {
            throw new AppException(MessageConstant.LocationMessage.LOCATION_NAME_REQUIRED, ErrorCodes.BadRequest, StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(request.Address))
        {
            throw new AppException(MessageConstant.LocationMessage.ADDRESS_REQUIRED, ErrorCodes.BadRequest, StatusCodes.Status400BadRequest);
        }

        if (!request.Type.HasValue)
        {
            throw new AppException(MessageConstant.LocationMessage.TYPE_REQUIRED, ErrorCodes.BadRequest, StatusCodes.Status400BadRequest);
        }

        if (!request.Status.HasValue)
        {
            throw new AppException(MessageConstant.LocationMessage.STATUS_REQUIRED, ErrorCodes.BadRequest, StatusCodes.Status400BadRequest);
        }

        if (request.Status is not (LocationStatuses.Inactive or LocationStatuses.Pending or LocationStatuses.Approved or LocationStatuses.Rejected or LocationStatuses.Active))
        {
            throw new AppException(MessageConstant.LocationMessage.LOCATION_STATUS_INVALID, ErrorCodes.BadRequest, StatusCodes.Status400BadRequest);
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
            Type = request.Type.Value,
            CreatedAt = DateTime.UtcNow,
            Status = request.Status.Value
        };

        await _locationRepository.AddLocationAsync(location);
        return (await _locationRepository.GetAdminLocationsAsync(location.LocationName, null, null, 1, 1)).Items.First(x => x.Id == location.Id);
    }

    public async Task<AdminLocationDto> UpdateAdminLocationAsync(Guid locationId, AdminUpsertLocationRequest request)
    {
        if (request.Status.HasValue
            && request.Status is not (LocationStatuses.Inactive or LocationStatuses.Pending or LocationStatuses.Approved or LocationStatuses.Rejected or LocationStatuses.Active))
        {
            throw new AppException(MessageConstant.LocationMessage.LOCATION_STATUS_INVALID, ErrorCodes.BadRequest, StatusCodes.Status400BadRequest);
        }

        var openingHours = ParseOperatingHours(request.OpeningHours, nameof(request.OpeningHours));
        var closingHours = ParseOperatingHours(request.ClosingHours, nameof(request.ClosingHours));

        var entity = await _locationRepository.GetLocationEntityForAdminAsync(locationId);
        if (entity == null)
        {
            throw new AppException(MessageConstant.LocationMessage.LOCATION_NOT_FOUND, ErrorCodes.NotFound, StatusCodes.Status404NotFound);
        }

        if (entity.HasPendingUpdate)
        {
            if (request.Status == LocationStatuses.Rejected)
            {
                ClearPendingUpdate(entity);
                await _locationRepository.UpdateLocationAsync(entity);
                await _locationRepository.ClearLocationMediaAsync(entity.Id, "location-pending");
                return await GetAdminLocationDetailAsync(entity.Id);
            }

            if (request.Status is LocationStatuses.Active or LocationStatuses.Approved)
            {
                var adminChangedCoreFields = !MatchesCurrentLocation(entity, request);
                if (adminChangedCoreFields)
                {
                    ApplyAdminUpdate(entity, request, openingHours, closingHours);
                }
                else
                {
                    entity.LocationName = entity.PendingLocationName ?? entity.LocationName;
                    entity.Description = entity.PendingDescription;
                    entity.Address = entity.PendingAddress ?? entity.Address;
                    entity.AddressLink = entity.PendingAddressLink;
                    entity.OpeningHours = entity.PendingOpeningHours;
                    entity.ClosingHours = entity.PendingClosingHours;
                    entity.Type = entity.PendingType ?? entity.Type;
                }

                if (request.OwnerId.HasValue)
                {
                    entity.OwnerId = request.OwnerId;
                }

                UpdateLocationStatusWithHistory(entity, request.Status.Value);

                ClearPendingUpdate(entity);
                await _locationRepository.UpdateLocationAsync(entity);
                await _locationRepository.ApplyPendingLocationMediaAsync(entity.Id);
                return await GetAdminLocationDetailAsync(entity.Id);
            }
        }

        ApplyAdminUpdate(entity, request, openingHours, closingHours);

        if (request.OwnerId.HasValue)
        {
            entity.OwnerId = request.OwnerId;
        }

        if (request.Status.HasValue)
        {
            UpdateLocationStatusWithHistory(entity, request.Status.Value);
        }

        if (HasCoreLocationChanges(request))
        {
            ClearPendingUpdate(entity);
        }

        await _locationRepository.UpdateLocationAsync(entity);
        if (HasCoreLocationChanges(request))
        {
            await _locationRepository.ClearLocationMediaAsync(entity.Id, "location-pending");
        }
        return await GetAdminLocationDetailAsync(entity.Id);
    }

    public async Task<AdminLocationDto> HideAdminLocationAsync(Guid locationId)
    {
        var entity = await _locationRepository.GetLocationEntityForAdminAsync(locationId);
        if (entity == null)
        {
            throw new AppException(MessageConstant.LocationMessage.LOCATION_NOT_FOUND, ErrorCodes.NotFound, StatusCodes.Status404NotFound);
        }

        UpdateLocationStatusWithHistory(entity, LocationStatuses.Inactive);
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
            throw new AppException(MessageConstant.ReviewMessage.REVIEW_NOT_FOUND, ErrorCodes.NotFound, StatusCodes.Status404NotFound);
        }

        return review;
    }

    public async Task<AdminReviewDto> UpdateAdminReviewStatusAsync(Guid reviewId, AdminUpdateReviewStatusRequest request)
    {
        if (request.Status is not (ReviewStatuses.Inactive or ReviewStatuses.Active))
        {
            throw new AppException(MessageConstant.ReviewMessage.REVIEW_STATUS_INVALID, ErrorCodes.BadRequest, StatusCodes.Status400BadRequest);
        }

        var review = await _locationRepository.GetReviewEntityForAdminAsync(reviewId);
        if (review == null)
        {
            throw new AppException(MessageConstant.ReviewMessage.REVIEW_NOT_FOUND, ErrorCodes.NotFound, StatusCodes.Status404NotFound);
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
            throw new AppException(MessageConstant.ReviewMessage.REVIEW_NOT_FOUND, ErrorCodes.NotFound, StatusCodes.Status404NotFound);
        }

        review.Status = ReviewStatuses.Inactive;
        await _locationRepository.UpdateReviewAsync(review);
        return await GetAdminReviewDetailAsync(reviewId);
    }

    public async Task<PagedResult<AdminCommentDto>> GetAdminCommentsAsync(string? search, int? status, int page, int pageSize)
    {
        return await _locationRepository.GetAdminCommentsAsync(search, status, page, pageSize);
    }

    public async Task<AdminCommentDto> GetAdminCommentDetailAsync(Guid commentId)
    {
        var comment = await _locationRepository.GetAdminCommentDetailAsync(commentId);
        if (comment == null)
        {
            throw new AppException(MessageConstant.CommentMessage.COMMENT_NOT_FOUND, ErrorCodes.NotFound, StatusCodes.Status404NotFound);
        }

        return comment;
    }

    public async Task<AdminCommentDto> HideAdminCommentAsync(Guid commentId)
    {
        var comment = await _locationRepository.GetCommentEntityAsync(commentId);
        if (comment == null)
        {
            throw new AppException(MessageConstant.CommentMessage.COMMENT_NOT_FOUND, ErrorCodes.NotFound, StatusCodes.Status404NotFound);
        }

        comment.Status = CommentStatuses.Inactive;
        await _locationRepository.UpdateCommentAsync(comment);
        return await GetAdminCommentDetailAsync(commentId);
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

        throw new AppException(MessageConstant.LocationMessage.TIME_FORMAT_INVALID, ErrorCodes.BadRequest, StatusCodes.Status400BadRequest);
    }

    private static bool MatchesCurrentLocation(Core.Models.Location entity, AdminUpsertLocationRequest request)
    {
        if (!HasCoreLocationChanges(request))
        {
            return true;
        }

        var normalizedRequestAddressLink = string.IsNullOrWhiteSpace(request.AddressLink) ? null : request.AddressLink.Trim();
        return entity.LocationName == request.LocationName?.Trim()
            && entity.Description == request.Description?.Trim()
            && entity.Address == request.Address?.Trim()
            && entity.AddressLink == normalizedRequestAddressLink
            && entity.Type == request.Type;
    }

    private static bool HasCoreLocationChanges(AdminUpsertLocationRequest request)
    {
        return request.LocationName != null
            || request.Description != null
            || request.Address != null
            || request.AddressLink != null
            || request.OpeningHours != null
            || request.ClosingHours != null
            || request.Type.HasValue;
    }

    private static void ApplyAdminUpdate(Core.Models.Location entity, AdminUpsertLocationRequest request, TimeSpan? openingHours, TimeSpan? closingHours)
    {
        if (request.LocationName != null)
        {
            entity.LocationName = request.LocationName.Trim();
        }

        if (request.Description != null)
        {
            entity.Description = request.Description.Trim();
        }

        if (request.Address != null)
        {
            entity.Address = request.Address.Trim();
        }

        if (request.AddressLink != null)
        {
            entity.AddressLink = string.IsNullOrWhiteSpace(request.AddressLink) ? null : request.AddressLink.Trim();
        }

        if (request.OpeningHours != null)
        {
            entity.OpeningHours = openingHours;
        }

        if (request.ClosingHours != null)
        {
            entity.ClosingHours = closingHours;
        }

        if (request.Type.HasValue)
        {
            entity.Type = request.Type.Value;
        }
    }

    private static void UpdateLocationStatusWithHistory(Core.Models.Location entity, int newStatus)
    {
        if (entity.Status == newStatus)
        {
            return;
        }

        entity.PreviousStatus = entity.Status;
        entity.Status = newStatus;
    }

    private static void ClearPendingUpdate(Core.Models.Location entity)
    {
        entity.HasPendingUpdate = false;
        entity.PendingLocationName = null;
        entity.PendingDescription = null;
        entity.PendingAddress = null;
        entity.PendingAddressLink = null;
        entity.PendingOpeningHours = null;
        entity.PendingClosingHours = null;
        entity.PendingType = null;
        entity.PendingUpdatedAt = null;
    }
}

