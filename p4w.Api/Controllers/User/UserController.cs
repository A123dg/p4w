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
                    Message = "Unauthorized",
                    Data = null
                };

            var profile = await _userService.GetUserProfileAsync(Guid.Parse(userId));
            return Ok(new ApiResponse<UserProfileDto>
            {
                Code = 200,
                Success = true,
                Message = "User profile retrieved successfully",
                Data = profile
            });
        }

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
                    Message = "Unauthorized",
                    Data = null
                };

            var recentLocation = await _userService.GetRecentLocationAsync(Guid.Parse(userId));
            return Ok(new ApiResponse<RecentLocationDto>
            {
                Code = 200,
                Success = true,
                Message = recentLocation == null
                    ? "User has no recent comment or review location"
                    : "Recent location retrieved successfully",
                Data = recentLocation
            });
        }

        [HttpPost("reviews")]
        public async Task<ActionResult<ApiResponse<ReviewDto>>> CreateReview([FromBody] CreateReviewRequest request)
        {
            var userId = GetCurrentUserId();
            var review = await _locationService.CreateReviewAsync(userId, request);

            return Ok(new ApiResponse<ReviewDto>
            {
                Code = 200,
                Success = true,
                Message = "Review created successfully",
                Data = review
            });
        }

        [HttpPost("comments")]
        public async Task<ActionResult<ApiResponse<CommentDto>>> CreateComment([FromBody] CreateCommentRequest request)
        {
            var userId = GetCurrentUserId();
            var comment = await _locationService.CreateCommentAsync(userId, request);

            return Ok(new ApiResponse<CommentDto>
            {
                Code = 200,
                Success = true,
                Message = "Comment created successfully",
                Data = comment
            });
        }

        [Authorize(Policy = AuthPolicies.AdminOnly)]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<UserResponseDto>>>> GetUsers([FromQuery] string? search, [FromQuery] Guid? roleId, [FromQuery] int? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var users = await _userService.GetUsersAsync(search, roleId, status, page, pageSize);
            return Ok(new ApiResponse<List<UserResponseDto>>
            {
                Code = 200,
                Success = true,
                Message = "Users retrieved successfully",
                Data = users.Items,
                MetaData = users.MetaData
            });
        }

        [Authorize(Policy = AuthPolicies.AdminOnly)]
        [HttpGet("{userId:guid}")]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetUserById(Guid userId)
        {
            var user = await _userService.GetAdminUserByIdAsync(userId);
            return Ok(new ApiResponse<UserResponseDto>
            {
                Code = 200,
                Success = true,
                Message = "User retrieved successfully",
                Data = user
            });
        }

        [Authorize(Policy = AuthPolicies.AdminOnly)]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> CreateUser([FromBody] AdminUpsertUserRequest request)
        {
            var user = await _userService.CreateAdminUserAsync(request);
            return Ok(new ApiResponse<UserResponseDto>
            {
                Code = 200,
                Success = true,
                Message = "User created successfully",
                Data = user
            });
        }

        [Authorize(Policy = AuthPolicies.AdminOnly)]
        [HttpPut("{userId:guid}")]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> UpdateUser(Guid userId, [FromBody] AdminUpsertUserRequest request)
        {
            var user = await _userService.UpdateAdminUserAsync(userId, request);
            return Ok(new ApiResponse<UserResponseDto>
            {
                Code = 200,
                Success = true,
                Message = "User updated successfully",
                Data = user
            });
        }

        [Authorize(Policy = AuthPolicies.AdminOnly)]
        [HttpPut("{userId:guid}/lock")]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> LockUser(Guid userId)
        {
            var user = await _userService.LockUserAsync(userId);
            return Ok(new ApiResponse<UserResponseDto>
            {
                Code = 200,
                Success = true,
                Message = "User locked successfully",
                Data = user
            });
        }
        [Authorize (Policy = AuthPolicies.AdminOnly)]
        [HttpPut("{userId:guid}/unlock")]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> UnlockUser(Guid userId)
        {
            var user = await _userService.UnlockUserAsync(userId);
            return Ok(new ApiResponse<UserResponseDto>
            {
                Code = 200,
                Success = true,
                Message = "User unlocked successfully",
                Data = user
            });
        }
       

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new Exception("Unauthorized");
            }

            return Guid.Parse(userId);
        }
    }
}
