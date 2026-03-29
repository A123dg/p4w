using p4w.Core.Dtos.Report;
using p4w.Core.Paginations;

namespace p4w.Core.Interfaces.Services.Report;

public interface IReportService
{
    Task<ReportDto> CreateReportAsync(Guid userId, CreateReportRequest request);
    Task<PagedResult<ReportDto>> GetReportsAsync(string? targetType, int? status, string? search, int page, int pageSize);
    Task<ReportDto> GetReportDetailAsync(Guid reportId);
    Task<ReportDto> UpdateReportStatusAsync(Guid reportId, UpdateReportStatusRequest request);
}
