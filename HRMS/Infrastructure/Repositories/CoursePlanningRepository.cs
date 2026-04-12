using Microsoft.EntityFrameworkCore;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;

namespace HRMS.Infrastructure.Repositories;

public class CoursePlanningRepository : Repository<CoursePlanning>, ICoursePlanningRepository
{
    public CoursePlanningRepository(HRMSDbContext context) : base(context)
    {
    }

    public async Task<CoursePlanning?> GetByIdWithDetailsAsync(int id, int tenantId)
    {
        return await _dbSet
            .Where(cp => cp.Id == id && cp.TenantId == tenantId)
            .Include(cp => cp.Course)
                .ThenInclude(c => c!.CourseType)
            .Include(cp => cp.Course)
                .ThenInclude(c => c!.CourseCategory)
            .Include(cp => cp.Trainer)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<CoursePlanning>> GetByTenantIdAsync(int tenantId)
    {
        return await _dbSet
            .Where(cp => cp.TenantId == tenantId && cp.IsActive)
            .Include(cp => cp.Course)
            .Include(cp => cp.Trainer)
            .OrderByDescending(cp => cp.StartDate)
            .ThenByDescending(cp => cp.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<CoursePlanning>> GetFilteredCoursePlansAsync(
        int tenantId,
        int? trainerId = null,
        int? courseId = null,
        bool? isActive = null)
    {
        // Use regular EF Core query instead of stored procedure to avoid composition issues
        var query = _dbSet
            .Where(cp => cp.TenantId == tenantId)
            .Include(cp => cp.Course)
                .ThenInclude(c => c!.CourseType)
            .Include(cp => cp.Course)
                .ThenInclude(c => c!.CourseCategory)
            .Include(cp => cp.Trainer)
            .AsQueryable();

        // Apply optional filters
        if (trainerId.HasValue)
        {
            query = query.Where(cp => cp.TrainerId == trainerId.Value);
        }

        if (courseId.HasValue)
        {
            query = query.Where(cp => cp.CourseId == courseId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(cp => cp.IsActive == isActive.Value);
        }

        var result = await query
            .OrderByDescending(cp => cp.StartDate)
            .ThenByDescending(cp => cp.StartTime)
            .ToListAsync();

        return result;
    }

    public async Task<int> ValidateConflictAsync(
        int tenantId,
        int trainerId,
        DateTime startDate,
        TimeSpan startTime,
        DateTime endDate,
        TimeSpan endTime,
        int? excludeId = null)
    {
        // Build DateTime objects for comparison
        var newStartDateTime = startDate.Add(startTime);
        var newEndDateTime = endDate.Add(endTime);

        // Query for conflicting schedules
        var query = _dbSet
            .Where(cp => cp.TenantId == tenantId)
            .Where(cp => cp.TrainerId == trainerId)
            .Where(cp => cp.IsActive)
            .AsQueryable();

        // Exclude current record if updating
        if (excludeId.HasValue)
        {
            query = query.Where(cp => cp.Id != excludeId.Value);
        }

        // Check for overlapping schedules
        // A conflict exists if any of these conditions are true:
        // 1. New schedule starts during an existing schedule
        // 2. New schedule ends during an existing schedule
        // 3. New schedule completely encompasses an existing schedule
        // 4. Existing schedule completely encompasses the new schedule
        var conflicts = await query
            .Where(cp =>
                // Case 1: New schedule starts during existing schedule
                (startDate >= cp.StartDate && startDate <= cp.EndDate &&
                    (startDate != cp.StartDate || startTime < cp.EndTime) &&
                    (startDate != cp.EndDate || startTime < cp.EndTime))
                ||
                // Case 2: New schedule ends during existing schedule
                (endDate >= cp.StartDate && endDate <= cp.EndDate &&
                    (endDate != cp.StartDate || endTime > cp.StartTime) &&
                    (endDate != cp.EndDate || endTime > cp.StartTime))
                ||
                // Case 3: New schedule encompasses existing schedule
                (startDate <= cp.StartDate && endDate >= cp.EndDate)
                ||
                // Case 4: Existing schedule encompasses new schedule
                (cp.StartDate <= startDate && cp.EndDate >= endDate)
            )
            .CountAsync();

        return conflicts;
    }
}
