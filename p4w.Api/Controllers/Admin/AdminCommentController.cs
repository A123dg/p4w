using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using p4w.Core.Constants;
using p4w.Core.Dtos.Comment;
using p4w.Core.Interfaces.Services.Location;
using p4w.Core.Paginations;

namespace p4w.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/comments")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public class AdminCommentController : ControllerBase
{
    private readonly ILocationService _locationService;

    public AdminCommentController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    /// <summary>
    /// Get comment list for admin moderation.
    /// </summary>
    /// <remarks>
    /// Admin only endpoint with filters for search, status, and pagination.
    /// </remarks>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AdminCommentDto>>>> GetComments([FromQuery] string? search, [FromQuery] int? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var comments = await _locationService.GetAdminCommentsAsync(search, status, page, pageSize);
        return Ok(new ApiResponse<List<AdminCommentDto>>
        {
            Code = 200,
            Success = true,
            Message = MessageConstant.CommentMessage.ADMIN_COMMENTS_RETRIEVED_SUCCESS,
            Data = comments.Items,
            MetaData = comments.MetaData
        });
    }

    /// <summary>
    /// Get comment detail for admin moderation.
    /// </summary>
    /// <remarks>
    /// Admin only endpoint returning full comment information by commentId.
    /// </remarks>
    [HttpGet("{commentId:guid}")]
    public async Task<ActionResult<ApiResponse<AdminCommentDto>>> GetCommentDetail(Guid commentId)
    {
        var comment = await _locationService.GetAdminCommentDetailAsync(commentId);
        return Ok(new ApiResponse<AdminCommentDto>
        {
            Code = 200,
            Success = true,
            Message = MessageConstant.CommentMessage.ADMIN_COMMENT_DETAIL_RETRIEVED_SUCCESS,
            Data = comment
        });
    }

    /// <summary>
    /// Hide a comment from public listing.
    /// </summary>
    /// <remarks>
    /// Admin only endpoint to soft-hide comment by commentId.
    /// </remarks>
    [HttpDelete("{commentId:guid}")]
    public async Task<ActionResult<ApiResponse<AdminCommentDto>>> HideComment(Guid commentId)
    {
        var comment = await _locationService.HideAdminCommentAsync(commentId);
        return Ok(new ApiResponse<AdminCommentDto>
        {
            Code = 200,
            Success = true,
            Message = MessageConstant.CommentMessage.ADMIN_COMMENT_HIDDEN_SUCCESS,
            Data = comment
        });
    }
}
