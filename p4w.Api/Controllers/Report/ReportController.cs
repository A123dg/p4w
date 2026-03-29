using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using p4w.Core.Constants;
using p4w.Core.Dtos.Report;
using p4w.Core.Interfaces.Services.Report;
using p4w.Core.Paginations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
            Message = "Report created successfully",
            Data = report
        });
    }

    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ReportDto>>>> GetReports([FromQuery] string? targetType, [FromQuery] int? status, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var reports = await _reportService.GetReportsAsync(targetType, status, search, page, pageSize);
        return Ok(new ApiResponse<List<ReportDto>>
        {
            Code = 200,
            Success = true,
            Message = "Reports retrieved successfully",
            Data = reports.Items,
            MetaData = reports.MetaData
        });
    }

    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [HttpGet("{reportId:guid}")]
    public async Task<ActionResult<ApiResponse<ReportDto>>> GetReportDetail(Guid reportId)
    {
        var report = await _reportService.GetReportDetailAsync(reportId);
        return Ok(new ApiResponse<ReportDto>
        {
            Code = 200,
            Success = true,
            Message = "Report detail retrieved successfully",
            Data = report
        });
    }

    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [HttpPut("{reportId:guid}/status")]
    public async Task<ActionResult<ApiResponse<ReportDto>>> UpdateReportStatus(Guid reportId, [FromBody] UpdateReportStatusRequest request)
    {
        var report = await _reportService.UpdateReportStatusAsync(reportId, request);
        return Ok(new ApiResponse<ReportDto>
        {
            Code = 200,
            Success = true,
            Message = "Report status updated successfully",
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
