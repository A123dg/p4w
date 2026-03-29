using Microsoft.EntityFrameworkCore;
using p4w.Core.Constants.Statuses;
using p4w.Core.Dtos.Report;
using p4w.Core.Interfaces.Repositories.Report;
using p4w.Core.Paginations;
using p4w.Data.Persistence;

namespace p4w.Data.Repositories.Report;

public class ReportRepository : IReportRepository
{
    private readonly AppDbContext _context;

    public ReportRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Core.Models.Report report)
    {
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();
    }

    public async Task<Core.Models.Report?> GetByIdAsync(Guid reportId)
    {
        return await _context.Reports
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == reportId);
    }

    public async Task<PagedResult<ReportDto>> GetReportsAsync(string? targetType, int? status, string? search, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;

        IQueryable<Core.Models.Report> query = _context.Reports
            .Include(x => x.User)
            .Where(x => x.Status != ReportStatuses.Inactive)
            .OrderByDescending(x => x.CreatedAt);

        if (!string.IsNullOrWhiteSpace(targetType))
        {
            query = query.Where(x => x.TargetType == targetType.Trim().ToLower());
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            query = query.Where(x => x.User.UserName.Contains(normalizedSearch) || x.Reason.Contains(normalizedSearch) || x.TargetId.Contains(normalizedSearch));
        }

        var total = await query.CountAsync();
        var reports = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ReportDto>
        {
            Items = reports.Select(MapToDto).ToList(),
            MetaData = new MetaData
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                TotalPage = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize)
            }
        };
    }

    public async Task<ReportDto?> GetReportDetailAsync(Guid reportId)
    {
        var report = await _context.Reports
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == reportId && x.Status != ReportStatuses.Inactive);

        return report == null ? null : MapToDto(report);
    }

    public async Task UpdateAsync(Core.Models.Report report)
    {
        _context.Reports.Update(report);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> TargetExistsAsync(string targetType, string targetId)
    {
        if (!Guid.TryParse(targetId, out var guidId))
        {
            return false;
        }

        return targetType.ToLower() switch
        {
            "user" => await _context.Users.AnyAsync(x => x.Id == guidId),
            "location" => await _context.Locations.AnyAsync(x => x.Id == guidId),
            "review" => await _context.Reviews.AnyAsync(x => x.Id == guidId),
            "comment" => await _context.Comments.AnyAsync(x => x.Id == guidId),
            _ => false
        };
    }

    private static ReportDto MapToDto(Core.Models.Report report)
    {
        return new ReportDto
        {
            Id = report.Id,
            UserId = report.UserId,
            ReportedBy = report.User.UserName,
            Reason = report.Reason,
            ReportedItemType = report.TargetType,
            ReportedItemId = report.TargetId,
            ReportedItem = $"{report.TargetType} - {report.TargetId}",
            Status = report.Status,
            StatusName = report.Status switch
            {
                ReportStatuses.Pending => "pending",
                ReportStatuses.Approved => "approved",
                ReportStatuses.Rejected => "rejected",
                _ => "unknown"
            },
            CreatedAt = report.CreatedAt
        };
    }
}
