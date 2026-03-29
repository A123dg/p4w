using Microsoft.AspNetCore.Http;
using p4w.Core.Constants.Statuses;
using p4w.Core.Dtos.Report;
using p4w.Core.Exceptions;
using p4w.Core.Interfaces.Repositories.Report;
using p4w.Core.Interfaces.Services.Report;
using p4w.Core.Paginations;

namespace p4w.Service.Services.Report;

public class ReportService : IReportService
{
    private static readonly string[] AllowedTargetTypes = ["user", "location", "review", "comment"];
    private readonly IReportRepository _reportRepository;

    public ReportService(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<ReportDto> CreateReportAsync(Guid userId, CreateReportRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new AppException("Reason is required", ErrorCodes.BadRequest, StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(request.TargetType) || !AllowedTargetTypes.Contains(request.TargetType.Trim().ToLower()))
        {
            throw new AppException("Target type is invalid", ErrorCodes.BadRequest, StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(request.TargetId))
        {
            throw new AppException("Target id is required", ErrorCodes.BadRequest, StatusCodes.Status400BadRequest);
        }

        var normalizedTargetType = request.TargetType.Trim().ToLower();
        var targetExists = await _reportRepository.TargetExistsAsync(normalizedTargetType, request.TargetId.Trim());
        if (!targetExists)
        {
            throw new AppException("Reported target not found", ErrorCodes.NotFound, StatusCodes.Status404NotFound);
        }

        var report = new Core.Models.Report
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Reason = request.Reason.Trim(),
            TargetType = normalizedTargetType,
            TargetId = request.TargetId.Trim(),
            Status = ReportStatuses.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _reportRepository.AddAsync(report);
        return await GetReportDetailAsync(report.Id);
    }

    public async Task<PagedResult<ReportDto>> GetReportsAsync(string? targetType, int? status, string? search, int page, int pageSize)
    {
        return await _reportRepository.GetReportsAsync(targetType, status, search, page, pageSize);
    }

    public async Task<ReportDto> GetReportDetailAsync(Guid reportId)
    {
        var report = await _reportRepository.GetReportDetailAsync(reportId);
        if (report == null)
        {
            throw new AppException("Report not found", ErrorCodes.NotFound, StatusCodes.Status404NotFound);
        }

        return report;
    }

    public async Task<ReportDto> UpdateReportStatusAsync(Guid reportId, UpdateReportStatusRequest request)
    {
        if (request.Status is not (ReportStatuses.Pending or ReportStatuses.Approved or ReportStatuses.Rejected))
        {
            throw new AppException("Status is invalid", ErrorCodes.BadRequest, StatusCodes.Status400BadRequest);
        }

        var report = await _reportRepository.GetByIdAsync(reportId);
        if (report == null)
        {
            throw new AppException("Report not found", ErrorCodes.NotFound, StatusCodes.Status404NotFound);
        }

        report.Status = request.Status;
        await _reportRepository.UpdateAsync(report);

        return await GetReportDetailAsync(report.Id);
    }
}
