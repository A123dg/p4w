using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using p4w.Core.Constants;
using p4w.Core.Dtos.Comment;
using p4w.Core.Dtos.Review;
using p4w.Core.Interfaces.Services.Location;
using p4w.Core.Dtos.User;
using p4w.Core.Interfaces.Services;
using p4w.Core.Interfaces.Services.Auth;
using p4w.Core.Paginations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace p4w.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILocationService _locationService;

        public UserController(IUserService userService, ILocationService locationService)
        {
            _userService = userService;
            _locationService = locationService;
        }

        /// <summary>
        /// Get profile of the current user.
        /// </summary>
        /// <remarks>
        /// Requires authentication and returns profile mapped from the current access token userId.
        /// </remarks>
        [HttpGet("profile")]
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetProfile()
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return new ApiResponse<UserProfileDto>
                {
                    Code = 401,
                    Success = false,
                    Message = MessageConstant.CommonMessage.UNAUTHORIZED,
                    Data = null
                };

            var profile = await _userService.GetUserProfileAsync(Guid.Parse(userId));
            return Ok(new ApiResponse<UserProfileDto>
            {
                Code = 200,
                Success = true,
                Message = MessageConstant.UserMessage.USER_PROFILE_RETRIEVED_SUCCESS,
                Data = profile
            });
        }

        /// <summary>
        /// Get the latest location visited by the current user.
        /// </summary>
        /// <remarks>
        /// Requires authentication and may return null data when no recent location exists.
        /// </remarks>
        [HttpGet("recent-location")]
        public async Task<ActionResult<ApiResponse<RecentLocationDto>>> GetRecentLocation()
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return new ApiResponse<RecentLocationDto>
                {
                    Code = 401,
                    Success = false,
                    Message = MessageConstant.CommonMessage.UNAUTHORIZED,
                    Data = null
                };

            var recentLocation = await _userService.GetRecentLocationAsync(Guid.Parse(userId));
            return Ok(new ApiResponse<RecentLocationDto>
            {
                Code = 200,
                Success = true,
                Message = recentLocation == null
                    ? MessageConstant.UserMessage.USER_RECENT_LOCATION_EMPTY
                    : MessageConstant.UserMessage.USER_RECENT_LOCATION_RETRIEVED_SUCCESS,
                Data = recentLocation
            });
        }

        /// <summary>
        /// Create a review as the current user.
        /// </summary>
        /// <remarks>
        /// Requires authentication. Review is created under the requester account.
        /// </remarks>
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
        /// Create a comment as the current user.
        /// </summary>
        /// <remarks>
        /// Requires authentication. Comment is created under the requester account.
        /// </remarks>
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

        /// <summary>
        /// Get user list for admin management.
        /// </summary>
        /// <remarks>
        /// Admin only. Supports filtering by search text, roleId, status, and pagination.
        /// </remarks>
        [Authorize(Policy = AuthPolicies.AdminOnly)]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<UserResponseDto>>>> GetUsers([FromQuery] string? search, [FromQuery] Guid? roleId, [FromQuery] int? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var users = await _userService.GetUsersAsync(search, roleId, status, page, pageSize);
            return Ok(new ApiResponse<List<UserResponseDto>>
            {
                Code = 200,
                Success = true,
                Message = MessageConstant.UserMessage.USERS_RETRIEVED_SUCCESS,
                Data = users.Items,
                MetaData = users.MetaData
            });
        }

        /// <summary>
        /// Get user detail by id for admin management.
        /// </summary>
        /// <remarks>
        /// Admin only endpoint returning full user information by userId.
        /// </remarks>
        [Authorize(Policy = AuthPolicies.AdminOnly)]
        [HttpGet("{userId:guid}")]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetUserById(Guid userId)
        {
            var user = await _userService.GetAdminUserByIdAsync(userId);
            return Ok(new ApiResponse<UserResponseDto>
            {
                Code = 200,
                Success = true,
                Message = MessageConstant.UserMessage.USER_RETRIEVED_SUCCESS,
                Data = user
            });
        }

        /// <summary>
        /// Create a new user from admin panel.
        /// </summary>
        /// <remarks>
        /// Admin only endpoint to create users with role and status settings.
        /// </remarks>
        [Authorize(Policy = AuthPolicies.AdminOnly)]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> CreateUser([FromBody] AdminUpsertUserRequest request)
        {
            var user = await _userService.CreateAdminUserAsync(request);
            return Ok(new ApiResponse<UserResponseDto>
            {
                Code = 200,
                Success = true,
                Message = MessageConstant.UserMessage.USER_CREATED_SUCCESS,
                Data = user
            });
        }

        /// <summary>
        /// Update an existing user from admin panel.
        /// </summary>
        /// <remarks>
        /// Admin only endpoint for updating profile, role, and status data by userId.
        /// </remarks>
        [Authorize(Policy = AuthPolicies.AdminOnly)]
        [HttpPut("{userId:guid}")]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> UpdateUser(Guid userId, [FromBody] AdminUpsertUserRequest request)
        {
            var user = await _userService.UpdateAdminUserAsync(userId, request);
            return Ok(new ApiResponse<UserResponseDto>
            {
                Code = 200,
                Success = true,
                Message = MessageConstant.UserMessage.USER_UPDATED_SUCCESS,
                Data = user
            });
        }

        /// <summary>
        /// Lock a user account.
        /// </summary>
        /// <remarks>
        /// Admin only endpoint that prevents a user from accessing the system.
        /// </remarks>
        [Authorize(Policy = AuthPolicies.AdminOnly)]
        [HttpPut("{userId:guid}/lock")]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> LockUser(Guid userId)
        {
            var user = await _userService.LockUserAsync(userId);
            return Ok(new ApiResponse<UserResponseDto>
            {
                Code = 200,
                Success = true,
                Message = MessageConstant.UserMessage.USER_LOCKED_SUCCESS,
                Data = user
            });
        }

        /// <summary>
        /// Unlock a user account.
        /// </summary>
        /// <remarks>
        /// Admin only endpoint that restores access for a locked user.
        /// </remarks>
        [Authorize (Policy = AuthPolicies.AdminOnly)]
        [HttpPut("{userId:guid}/unlock")]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> UnlockUser(Guid userId)
        {
            var user = await _userService.UnlockUserAsync(userId);
            return Ok(new ApiResponse<UserResponseDto>
            {
                Code = 200,
                Success = true,
                Message = MessageConstant.UserMessage.USER_UNLOCKED_SUCCESS,
                Data = user
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
}
