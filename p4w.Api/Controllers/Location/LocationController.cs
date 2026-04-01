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

    [HttpGet("{locationId:guid}")]
    public async Task<ActionResult<ApiResponse<LocationDetailDto>>> GetLocationDetail(Guid locationId)
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
