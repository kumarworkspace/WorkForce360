using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class ProgressTrackingRepository : Repository<ProgressTracking>, IProgressTrackingRepository
{
    public ProgressTrackingRepository(HRMSDbContext context) : base(context) { }

    public async Task<IEnumerable<ProgressTracking>> GetByEnrollmentIdAsync(int enrollmentId)
    {
        return await _dbSet
            .Include(p => p.Module)
            .Where(p => p.EnrollmentId == enrollmentId)
            .ToListAsync();
    }

    public async Task<ProgressTracking?> GetByEnrollmentAndModuleAsync(int enrollmentId, int moduleId)
    {
        return await _dbSet.FirstOrDefaultAsync(p =>
            p.EnrollmentId == enrollmentId && p.LmsModuleId == moduleId);
    }

    public async Task<decimal> GetOverallProgressAsync(int enrollmentId)
    {
        var records = await _dbSet.Where(p => p.EnrollmentId == enrollmentId).ToListAsync();
        if (!records.Any()) return 0;
        return records.Average(p => p.ProgressPct);
    }
}
