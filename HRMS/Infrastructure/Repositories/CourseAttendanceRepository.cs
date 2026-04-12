using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using Npgsql;
using HRMS.Core.Application.DTOs;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;

namespace HRMS.Infrastructure.Repositories;

public class CourseAttendanceRepository : Repository<CourseAttendance>, ICourseAttendanceRepository
{
    public CourseAttendanceRepository(HRMSDbContext context) : base(context)
    {
    }

    public async Task<MarkAttendanceResponse> MarkAttendanceAsync(
        int userId,
        int coursePlanId,
        int staffId,
        int tenantId,
        TimeSpan? checkInTime = null,
        int? createdBy = null)
    {
        try
        {
            var today = DateTime.Now.Date;

            // Check if attendance already recorded today
            var existing = await _dbSet
                .FirstOrDefaultAsync(a =>
                    a.CoursePlanId == coursePlanId &&
                    a.StaffId      == staffId       &&
                    a.TenantId     == tenantId       &&
                    a.AttendanceDate.Date == today   &&
                    a.IsActive);

            if (existing != null)
            {
                // Update check-in time if provided
                if (checkInTime.HasValue)
                {
                    existing.CheckInTime  = checkInTime;
                    existing.UpdatedDate  = DateTime.Now;
                    existing.UpdatedBy    = createdBy;
                    await _context.SaveChangesAsync();
                }

                return new MarkAttendanceResponse
                {
                    AttendanceId = existing.AttendanceId,
                    Message      = "Attendance already recorded for today.",
                    Success      = true
                };
            }

            // Insert new attendance record
            var record = new CourseAttendance
            {
                UserId         = userId,
                CoursePlanId   = coursePlanId,
                StaffId        = staffId,
                AttendanceDate = DateTime.Now,
                CheckInTime    = checkInTime ?? DateTime.Now.TimeOfDay,
                Status         = "Present",
                TenantId       = tenantId,
                IsActive       = true,
                CreatedDate    = DateTime.Now,
                CreatedBy      = createdBy
            };

            await _dbSet.AddAsync(record);
            await _context.SaveChangesAsync();

            return new MarkAttendanceResponse
            {
                AttendanceId = record.AttendanceId,
                Message      = "Attendance marked successfully.",
                Success      = true
            };
        }
        catch (Exception)
        {
            return new MarkAttendanceResponse
            {
                AttendanceId = -1,
                Message      = "Failed to mark attendance.",
                Success      = false
            };
        }
    }

    public async Task<IEnumerable<CourseAttendanceDto>> GetCourseAttendanceAsync(
        int tenantId,
        int? coursePlanId = null,
        int? userId       = null,
        DateTime? fromDate = null,
        DateTime? toDate   = null)
    {
        var query = _dbSet
            .Where(a => a.TenantId == tenantId && a.IsActive)
            .Include(a => a.User)
            .Include(a => a.Staff)
            .Include(a => a.CoursePlan)
            .AsQueryable();

        if (coursePlanId.HasValue)
            query = query.Where(a => a.CoursePlanId == coursePlanId.Value);

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);

        if (fromDate.HasValue)
            query = query.Where(a => a.AttendanceDate.Date >= fromDate.Value.Date);

        if (toDate.HasValue)
            query = query.Where(a => a.AttendanceDate.Date <= toDate.Value.Date);

        return await query
            .OrderByDescending(a => a.AttendanceDate)
            .Select(a => new CourseAttendanceDto
            {
                AttendanceId  = a.AttendanceId,
                UserId        = a.UserId,
                CoursePlanId  = a.CoursePlanId,
                StaffId       = a.StaffId,
                AttendanceDate = a.AttendanceDate,
                CheckInTime   = a.CheckInTime,
                CheckOutTime  = a.CheckOutTime,
                Status        = a.Status,
                Remarks       = a.Remarks,
                TenantId      = a.TenantId,
                UserName      = a.User != null ? a.User.FullName : string.Empty,
                StaffName     = a.Staff != null ? a.Staff.Name : string.Empty,
                EmployeeCode  = a.Staff != null ? a.Staff.EmployeeCode : null
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<AttendanceSummaryDto>> GetAttendanceByCoursePlanAsync(
        int coursePlanId,
        int tenantId)
    {
        return await _dbSet
            .Where(a => a.CoursePlanId == coursePlanId && a.TenantId == tenantId && a.IsActive)
            .Include(a => a.User)
            .Include(a => a.Staff)
            .OrderByDescending(a => a.AttendanceDate)
            .Select(a => new AttendanceSummaryDto
            {
                AttendanceId  = a.AttendanceId,
                AttendanceDate = a.AttendanceDate,
                CheckInTime   = a.CheckInTime,
                Status        = a.Status,
                UserName      = a.User != null ? a.User.FullName : string.Empty,
                StaffName     = a.Staff != null ? a.Staff.Name : string.Empty,
                EmployeeCode  = a.Staff != null ? a.Staff.EmployeeCode : null
            })
            .AsNoTracking()
            .ToListAsync();
    }
}
