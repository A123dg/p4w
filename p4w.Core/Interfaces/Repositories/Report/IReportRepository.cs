using p4w.Core.Dtos.Report;
using p4w.Core.Models;
using p4w.Core.Paginations;

namespace p4w.Core.Interfaces.Repositories.Report;

public interface IReportRepository
{
    Task AddAsync(Core.Models.Report report);
    Task<Core.Models.Report?> GetByIdAsync(Guid reportId);
    Task<PagedResult<ReportDto>> GetReportsAsync(string? targetType, int? status, string? search, int page, int pageSize);
    Task<ReportDto?> GetReportDetailAsync(Guid reportId);
    Task UpdateAsync(Core.Models.Report report);
    Task<bool> TargetExistsAsync(string targetType, string targetId);
}
