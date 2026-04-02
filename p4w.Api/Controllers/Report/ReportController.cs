using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using p4w.Core.Constants;
using p4w.Core.Dtos.Report;
using p4w.Core.Interfaces.Services.Report;
using p4w.Core.Paginations;

namespace p4w.Api.Controllers.Report;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Create a report for inappropriate content.
    /// </summary>
    /// <remarks>
    /// Requires authentication. User can report a target object with reason and detail.
    /// </remarks>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ReportDto>>> CreateReport([FromBody] CreateReportRequest request)
    {
        var userId = GetCurrentUserId();
        var report = await _reportService.CreateReportAsync(userId, request);
        return Ok(new ApiResponse<ReportDto>
        {
            Code = 200,
            Success = true,
            Message = MessageConstant.ReportMessage.REPORT_CREATED_SUCCESS,
            Data = report
        });
    }

    /// <summary>
    /// Get report list for admin moderation.
    /// </summary>
    /// <remarks>
    /// Admin only endpoint with filters for targetType, status, search, and pagination.
    /// </remarks>
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ReportDto>>>> GetReports([FromQuery] string? targetType, [FromQuery] int? status, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var reports = await _reportService.GetReportsAsync(targetType, status, search, page, pageSize);
        return Ok(new ApiResponse<List<ReportDto>>
        {
            Code = 200,
            Success = true,
            Message = MessageConstant.ReportMessage.REPORTS_RETRIEVED_SUCCESS,
            Data = reports.Items,
            MetaData = reports.MetaData
        });
    }

    /// <summary>
    /// Get detail of a report by id.
    /// </summary>
    /// <remarks>
    /// Admin only endpoint returning report payload and moderation context.
    /// </remarks>
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [HttpGet("{reportId:guid}")]
    public async Task<ActionResult<ApiResponse<ReportDto>>> GetReportDetail(Guid reportId)
    {
        var report = await _reportService.GetReportDetailAsync(reportId);
        return Ok(new ApiResponse<ReportDto>
        {
            Code = 200,
            Success = true,
            Message = MessageConstant.ReportMessage.REPORT_DETAIL_RETRIEVED_SUCCESS,
            Data = report
        });
    }

    /// <summary>
    /// Update moderation status of a report.
    /// </summary>
    /// <remarks>
    /// Admin only endpoint to mark report as pending, resolved, rejected, or other configured status.
    /// </remarks>
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [HttpPut("{reportId:guid}/status")]
    public async Task<ActionResult<ApiResponse<ReportDto>>> UpdateReportStatus(Guid reportId, [FromBody] UpdateReportStatusRequest request)
    {
        var report = await _reportService.UpdateReportStatusAsync(reportId, request);
        return Ok(new ApiResponse<ReportDto>
        {
            Code = 200,
            Success = true,
            Message = MessageConstant.ReportMessage.REPORT_STATUS_UPDATED_SUCCESS,
            Data = report
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
