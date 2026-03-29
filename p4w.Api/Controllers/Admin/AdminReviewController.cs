using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using p4w.Core.Constants;
using p4w.Core.Dtos.Review;
using p4w.Core.Interfaces.Services.Location;
using p4w.Core.Paginations;

namespace p4w.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/reviews")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public class AdminReviewController : ControllerBase
{
    private readonly ILocationService _locationService;

    public AdminReviewController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AdminReviewDto>>>> GetReviews([FromQuery] string? search, [FromQuery] int? status, [FromQuery] int? minRating, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var reviews = await _locationService.GetAdminReviewsAsync(search, status, minRating, page, pageSize);
        return Ok(new ApiResponse<List<AdminReviewDto>>
        {
            Code = 200,
            Success = true,
            Message = "Admin reviews retrieved successfully",
            Data = reviews.Items,
            MetaData = reviews.MetaData
        });
    }

    [HttpGet("{reviewId:guid}")]
    public async Task<ActionResult<ApiResponse<AdminReviewDto>>> GetReviewDetail(Guid reviewId)
    {
        var review = await _locationService.GetAdminReviewDetailAsync(reviewId);
        return Ok(new ApiResponse<AdminReviewDto>
        {
            Code = 200,
            Success = true,
            Message = "Admin review detail retrieved successfully",
            Data = review
        });
    }

    [HttpPut("{reviewId:guid}/status")]
    public async Task<ActionResult<ApiResponse<AdminReviewDto>>> UpdateReviewStatus(Guid reviewId, [FromBody] AdminUpdateReviewStatusRequest request)
    {
        var review = await _locationService.UpdateAdminReviewStatusAsync(reviewId, request);
        return Ok(new ApiResponse<AdminReviewDto>
        {
            Code = 200,
            Success = true,
            Message = "Admin review status updated successfully",
            Data = review
        });
    }

    [HttpDelete("{reviewId:guid}")]
    public async Task<ActionResult<ApiResponse<AdminReviewDto>>> HideReview(Guid reviewId)
    {
        var review = await _locationService.HideAdminReviewAsync(reviewId);
        return Ok(new ApiResponse<AdminReviewDto>
        {
            Code = 200,
            Success = true,
            Message = "Admin review hidden successfully",
            Data = review
        });
    }
}
