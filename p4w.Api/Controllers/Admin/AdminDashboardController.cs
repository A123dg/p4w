using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using p4w.Core.Constants;
using p4w.Core.Dtos.Dashboard;
using p4w.Core.Interfaces.Services.Dashboard;
using p4w.Core.Paginations;

namespace p4w.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public class AdminDashboardController : ControllerBase
{
    private readonly IAdminDashboardService _adminDashboardService;

    public AdminDashboardController(IAdminDashboardService adminDashboardService)
    {
        _adminDashboardService = adminDashboardService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<AdminDashboardDto>>> GetDashboard()
    {
        var dashboard = await _adminDashboardService.GetDashboardAsync();
        return Ok(new ApiResponse<AdminDashboardDto>
        {
            Code = 200,
            Success = true,
            Message = MessageConstant.DashboardMessage.ADMIN_DASHBOARD_RETRIEVED_SUCCESS,
            Data = dashboard
        });
    }
}
