using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using p4w.Core.Constants;
using p4w.Core.Dtos.Comment;
using p4w.Core.Dtos.Location;
using p4w.Core.Dtos.Review;
using p4w.Core.Interfaces.Services.Location;
using p4w.Core.Paginations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace p4w.Api.Controllers.Location;

[ApiController]
[Route("api/[controller]")]
public class LocationController : ControllerBase
{
    private readonly ILocationService _locationService;

    public LocationController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    /// <summary>
    /// Get public location list with optional filters.
    /// </summary>
    /// <remarks>
    /// Supports search by keyword, filter by type, and pagination via page/pageSize.
    /// </remarks>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<LocationCardDto>>>> GetLocations([FromQuery] string? search, [FromQuery] int? type, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var locations = await _locationService.GetLocationsAsync(search, type, page, pageSize);
        return Ok(new ApiResponse<List<LocationCardDto>>
        {
            Code = 200,
            Success = true,
            Message = MessageConstant.LocationMessage.LOCATIONS_RETRIEVED_SUCCESS,
            Data = locations.Items,
            MetaData = locations.MetaData
        });
    }

    /// <summary>
    /// Get detail of a specific location.
    /// </summary>
    /// <remarks>
    /// Returns full location information by locationId.
    /// </remarks>
    [HttpGet("{locationId:guid}")]
    public async Task<ActionResult<ApiResponse<LocationDetailDto>>> GetLocationDetail(Guid locationId)
    {
        try
        {
            var location = await _locationService.GetLocationDetailAsync(locationId);
            return Ok(new ApiResponse<LocationDetailDto>
            {
                Code = 200,
                Success = true,
                Message = MessageConstant.LocationMessage.LOCATION_DETAIL_RETRIEVED_SUCCESS,
                Data = location
            });
        }
        catch (p4w.Core.Exceptions.AppException ex) when (ex.ErrorCode == p4w.Core.Exceptions.ErrorCodes.NotFound)
        {
            // If not found for public (likely inactive), allow owner to view regardless of status
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                throw;
            }

            Guid userId;
            try
            {
                userId = GetCurrentUserId();
            }
            catch
            {
                throw;
            }

            // Verify ownership
            var adminLocation = await _locationService.GetAdminLocationDetailAsync(locationId);
            if (adminLocation.OwnerId != userId)
            {
                throw;
            }

            var ownerDetail = await _locationService.GetLocationDetailForOwnerAsync(locationId);
            if (ownerDetail == null)
            {
                throw new p4w.Core.Exceptions.AppException(MessageConstant.LocationMessage.LOCATION_NOT_FOUND, p4w.Core.Exceptions.ErrorCodes.NotFound, StatusCodes.Status404NotFound);
            }

            return Ok(new ApiResponse<LocationDetailDto>
            {
                Code = 200,
                Success = true,
                Message = MessageConstant.LocationMessage.LOCATION_DETAIL_RETRIEVED_SUCCESS,
                Data = ownerDetail
            });
        }
    }

    /// <summary>
    /// Get reviews of a location.
    /// </summary>
    /// <remarks>
    /// Returns paged reviews for the selected location.
    /// </remarks>
    [HttpGet("{locationId:guid}/reviews")]
    public async Task<ActionResult<ApiResponse<List<ReviewDto>>>> GetLocationReviews(Guid locationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var reviews = await _locationService.GetLocationReviewsAsync(locationId, page, pageSize);
        return Ok(new ApiResponse<List<ReviewDto>>
        {
            Code = 200,
            Success = true,
            Message = MessageConstant.LocationMessage.LOCATION_REVIEWS_RETRIEVED_SUCCESS,
            Data = reviews.Items,
            MetaData = reviews.MetaData
        });
    }

    /// <summary>
    /// If the current user is the owner of the specified location, return all locations owned by that user including statuses.
    /// </summary>
    [Authorize]
    [HttpGet("{locationId:guid}/owner-locations")]
    public async Task<ActionResult<ApiResponse<List<AdminLocationDto>>>> GetOwnerLocationsIfOwner(Guid locationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetCurrentUserId();
        var locationDetail = await _locationService.GetAdminLocationDetailAsync(locationId);
        if (locationDetail.OwnerId != userId)
        {
            return Forbid();
        }

        var locations = await _locationService.GetOwnerLocationsAsync(userId, page, pageSize);
        return Ok(new ApiResponse<List<AdminLocationDto>>
        {
            Code = 200,
            Success = true,
            Message = MessageConstant.LocationMessage.LOCATIONS_RETRIEVED_SUCCESS,
            Data = locations.Items,
            MetaData = locations.MetaData
        });
    }

    /// <summary>
    /// If the current user is the owner of the specified location, return the admin detail of that location (includes status).
    /// </summary>
    [Authorize]
    [HttpGet("{locationId:guid}/owner-detail")]
    public async Task<ActionResult<ApiResponse<AdminLocationDto>>> GetOwnerLocationDetail(Guid locationId)
    {
        var userId = GetCurrentUserId();
        var location = await _locationService.GetAdminLocationDetailAsync(locationId);
        if (location.OwnerId != userId)
        {
            return Forbid();
        }

        return Ok(new ApiResponse<AdminLocationDto>
        {
            Code = 200,
            Success = true,
            Message = MessageConstant.LocationMessage.LOCATION_DETAIL_RETRIEVED_SUCCESS,
            Data = location
        });
    }

    /// <summary>
    /// Create a new location request.
    /// </summary>
    /// <remarks>
    /// Requires authentication. Newly created location is marked as pending approval.
    /// </remarks>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<AdminLocationDto>>> CreateLocation([FromBody] CreateLocationRequest request)
    {
        var userId = GetCurrentUserId();
        var location = await _locationService.CreateLocationAsync(userId, request);

        return Ok(new ApiResponse<AdminLocationDto>
        {
            Code = 200,
            Success = true,
            Message = MessageConstant.LocationMessage.LOCATION_CREATED_PENDING_APPROVAL,
            Data = location
        });
    }

    /// <summary>
    /// Submit an update request for an existing location.
    /// </summary>
    /// <remarks>
    /// Requires authentication. Update is submitted for approval before being published.
    /// </remarks>
    [Authorize]
    [HttpPut("{locationId:guid}")]
    public async Task<ActionResult<ApiResponse<AdminLocationDto>>> UpdateLocation(Guid locationId, [FromBody] UpdateLocationRequest request)
    {
        var userId = GetCurrentUserId();
        var location = await _locationService.RequestLocationUpdateAsync(userId, locationId, request);

        return Ok(new ApiResponse<AdminLocationDto>
        {
            Code = 200,
            Success = true,
            Message = MessageConstant.LocationMessage.LOCATION_UPDATED_PENDING_APPROVAL,
            Data = location
        });
    }

    /// <summary>
    /// Create a review for a location.
    /// </summary>
    /// <remarks>
    /// Requires authentication and associates the review with the current user.
    /// </remarks>
    [Authorize]
    [HttpPost("reviews")]
    public async Task<ActionResult<ApiResponse<ReviewDto>>> CreateReview([FromBody] CreateReviewRequest request)
    {
        var userId = GetCurrentUserId();
        var review = await _locationService.CreateReviewAsync(userId, request);

        return Ok(new ApiResponse<ReviewDto>
        {
            Code = 200,
            Success = true,
            Message = MessageConstant.ReviewMessage.REVIEW_CREATED_SUCCESS,
            Data = review
        });
    }

    /// <summary>
    /// Get comments of a review.
    /// </summary>
    /// <remarks>
    /// Returns paged comments for the selected reviewId.
    /// </remarks>
    [HttpGet("reviews/{reviewId:guid}/comments")]
    public async Task<ActionResult<ApiResponse<List<CommentDto>>>> GetReviewComments(Guid reviewId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var comments = await _locationService.GetReviewCommentsAsync(reviewId, page, pageSize);
        return Ok(new ApiResponse<List<CommentDto>>
        {
            Code = 200,
            Success = true,
            Message = MessageConstant.ReviewMessage.REVIEW_COMMENTS_RETRIEVED_SUCCESS,
            Data = comments.Items,
            MetaData = comments.MetaData
        });
    }

    /// <summary>
    /// Create a comment for a review.
    /// </summary>
    /// <remarks>
    /// Requires authentication and links the comment to the current user.
    /// </remarks>
    [Authorize]
    [HttpPost("comments")]
    public async Task<ActionResult<ApiResponse<CommentDto>>> CreateComment([FromBody] CreateCommentRequest request)
    {
        var userId = GetCurrentUserId();
        var comment = await _locationService.CreateCommentAsync(userId, request);

        return Ok(new ApiResponse<CommentDto>
        {
            Code = 200,
            Success = true,
            Message = MessageConstant.CommentMessage.COMMENT_CREATED_SUCCESS,
            Data = comment
        });
    }

    private Guid GetCurrentUserId()
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException();
        }

        return Guid.Parse(userId);
    }
}
