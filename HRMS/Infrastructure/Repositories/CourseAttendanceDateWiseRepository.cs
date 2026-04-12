using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using HRMS.Core.Application.DTOs;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;

namespace HRMS.Infrastructure.Repositories;

public class CourseAttendanceDateWiseRepository : Repository<CourseAttendanceDateWise>, ICourseAttendanceDateWiseRepository
{
    public CourseAttendanceDateWiseRepository(HRMSDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Retrieves attendance records with staff details using EF Core.
    /// Using EF Core (instead of ADO.NET) ensures DateTime values share the same
    /// DateTimeKind so dictionary key comparisons in the attendance grid always match.
    /// </summary>
    public async Task<IEnumerable<AttendanceDateWiseDto>> GetAttendanceByCoursePlanAsync(int coursePlanId, int tenantId)
    {
        var records = await _dbSet
            .AsNoTracking()
            .Include(ca => ca.Staff)
            .Where(ca => ca.CoursePlanId == coursePlanId
                      && ca.TenantId     == tenantId
                      && ca.IsActive)
            .OrderBy(ca => ca.AttendanceDate)
            .ThenBy(ca => ca.Staff!.Name)
            .ToListAsync();

        return records.Select(ca => new AttendanceDateWiseDto
        {
            AttendanceId   = ca.AttendanceId,
            CoursePlanId   = ca.CoursePlanId,
            StaffId        = ca.StaffId,
            StaffName      = ca.Staff?.Name      ?? string.Empty,
            EmployeeCode   = ca.Staff?.EmployeeCode,
            Department     = ca.Staff?.Department,
            Position       = ca.Staff?.Position,
            AttendanceDate = ca.AttendanceDate,   // same EF Core DateTimeKind as course dates
            IsPresent      = ca.IsPresent,
            Remarks        = ca.Remarks,
            TenantId       = ca.TenantId,
            IsActive       = ca.IsActive,
            CreatedDate    = ca.CreatedDate,
            CreatedBy      = ca.CreatedBy,
            UpdatedDate    = ca.UpdatedDate,
            UpdatedBy      = ca.UpdatedBy
        });
    }

    public async Task<CourseAttendanceDateWise?> GetByCoursePlanStaffDateAsync(
        int coursePlanId,
        int staffId,
        DateTime attendanceDate,
        int tenantId)
    {
        var normalizedDate = new DateTime(attendanceDate.Year, attendanceDate.Month, attendanceDate.Day);
        return await _dbSet
            .FirstOrDefaultAsync(ca => ca.CoursePlanId == coursePlanId
                && ca.StaffId == staffId
                && ca.AttendanceDate == normalizedDate
                && ca.TenantId == tenantId
                && ca.IsActive);
    }

    public async Task<IEnumerable<CourseAttendanceDateWise>> GetByCoursePlanIdAsync(int coursePlanId, int tenantId)
    {
        return await _dbSet
            .Where(ca => ca.CoursePlanId == coursePlanId && ca.TenantId == tenantId && ca.IsActive)
            .ToListAsync();
    }

    #region CRUD Stored Procedure Methods

    public async Task<IEnumerable<AttendanceDateWiseDto>> GetAttendanceDateWiseSpAsync(int coursePlanId, int tenantId, DateTime? attendanceDate = null, int? staffId = null)
    {
        var parameters = new[]
        {
            new NpgsqlParameter("@CoursePlanId", coursePlanId),
            new NpgsqlParameter("@TenantId", tenantId),
            new NpgsqlParameter("@AttendanceDate", (object?)attendanceDate ?? DBNull.Value),
            new NpgsqlParameter("@StaffId", (object?)staffId ?? DBNull.Value)
        };

        var result = await _context.Set<AttendanceDateWiseDto>()
            .FromSqlRaw("SELECT * FROM usp_GetCourseAttendanceDateWise(@CoursePlanId, @TenantId, @AttendanceDate, @StaffId)", parameters)
            .AsNoTracking()
            .ToListAsync();

        return result;
    }

    public async Task<AttendanceOperationResponse> CreateAttendanceDateWiseSpAsync(CreateAttendanceDateWiseRequest request, int tenantId, int? createdBy = null)
    {
        // Upsert: update existing record if present, otherwise insert
        var existing = await _dbSet.FirstOrDefaultAsync(ca =>
            ca.CoursePlanId == request.CoursePlanId &&
            ca.StaffId == request.StaffId &&
            ca.AttendanceDate.Date == request.AttendanceDate.Date &&
            ca.TenantId == tenantId &&
            ca.IsActive);

        if (existing != null)
        {
            existing.IsPresent   = request.IsPresent;
            existing.Remarks     = request.Remarks;
            existing.UpdatedDate = DateTime.Now;
            existing.UpdatedBy   = createdBy;
            await _context.SaveChangesAsync();

            return new AttendanceOperationResponse
            {
                AttendanceId = existing.AttendanceId,
                Success      = true,
                Message      = "Attendance updated successfully"
            };
        }

        var record = new CourseAttendanceDateWise
        {
            CoursePlanId   = request.CoursePlanId,
            StaffId        = request.StaffId,
            AttendanceDate = request.AttendanceDate,
            IsPresent      = request.IsPresent,
            Remarks        = request.Remarks,
            TenantId       = tenantId,
            IsActive       = true,
            CreatedDate    = DateTime.Now,
            CreatedBy      = createdBy
        };

        await _dbSet.AddAsync(record);
        await _context.SaveChangesAsync();

        return new AttendanceOperationResponse
        {
            AttendanceId = record.AttendanceId,
            Success      = true,
            Message      = "Attendance created successfully"
        };
    }

    public async Task<AttendanceOperationResponse> UpdateAttendanceDateWiseSpAsync(UpdateAttendanceDateWiseByIdRequest request, int tenantId, int? updatedBy = null)
    {
        var existing = await _dbSet.FirstOrDefaultAsync(ca =>
            ca.AttendanceId == request.AttendanceId &&
            ca.TenantId == tenantId &&
            ca.IsActive);

        if (existing == null)
            return new AttendanceOperationResponse { Success = false, Message = "Attendance record not found" };

        existing.IsPresent   = request.IsPresent;
        existing.Remarks     = request.Remarks;
        existing.UpdatedDate = DateTime.Now;
        existing.UpdatedBy   = updatedBy;

        await _context.SaveChangesAsync();

        return new AttendanceOperationResponse
        {
            AttendanceId = existing.AttendanceId,
            Success      = true,
            Message      = "Attendance updated successfully"
        };
    }

    public async Task<AttendanceOperationResponse> DeleteAttendanceDateWiseSpAsync(int attendanceId, int tenantId, int? updatedBy = null)
    {
        var existing = await _dbSet.FirstOrDefaultAsync(ca =>
            ca.AttendanceId == attendanceId &&
            ca.TenantId == tenantId &&
            ca.IsActive);

        if (existing == null)
            return new AttendanceOperationResponse { Success = false, Message = "Attendance record not found" };

        existing.IsActive    = false;
        existing.UpdatedDate = DateTime.Now;
        existing.UpdatedBy   = updatedBy;

        await _context.SaveChangesAsync();

        return new AttendanceOperationResponse
        {
            AttendanceId = attendanceId,
            Success      = true,
            Message      = "Attendance deleted successfully"
        };
    }

    /// <summary>
    /// Bulk upsert attendance records using Entity Framework.
    /// For each item: updates existing record (if found) or inserts a new one.
    /// All changes are committed in a single SaveChangesAsync call for efficiency.
    /// </summary>
    public async Task<AttendanceOperationResponse> BulkMarkAttendanceDateWiseSpAsync(
        BulkAttendanceDateWiseRequest request, int tenantId, int? createdBy = null)
    {
        try
        {
            var req = request.AttendanceDate;
            var attendanceDate = new DateTime(req.Year, req.Month, req.Day);

            // Load all existing records for this course/date/tenant in one query
            var existingRecords = await _dbSet
                .Where(ca => ca.CoursePlanId  == request.CoursePlanId
                          && ca.AttendanceDate == attendanceDate
                          && ca.TenantId       == tenantId
                          && ca.IsActive)
                .ToListAsync();

            var existingByStaff = existingRecords.ToDictionary(ca => ca.StaffId);

            foreach (var item in request.AttendanceItems)
            {
                if (existingByStaff.TryGetValue(item.StaffId, out var existing))
                {
                    // Update existing record
                    existing.IsPresent   = item.IsPresent;
                    existing.Remarks     = item.Remarks;
                    existing.UpdatedDate = DateTime.Now;
                    existing.UpdatedBy   = createdBy;
                }
                else
                {
                    // Insert new record — use the same normalized date
                    await _dbSet.AddAsync(new CourseAttendanceDateWise
                    {
                        CoursePlanId   = request.CoursePlanId,
                        StaffId        = item.StaffId,
                        AttendanceDate = attendanceDate,   // already normalized above
                        IsPresent      = item.IsPresent,
                        Remarks        = item.Remarks,
                        TenantId       = tenantId,
                        IsActive       = true,
                        CreatedDate    = DateTime.Now,
                        CreatedBy      = createdBy
                    });
                }
            }

            await _context.SaveChangesAsync();

            return new AttendanceOperationResponse
            {
                Success = true,
                Message = "Bulk attendance saved successfully"
            };
        }
        catch (Exception ex)
        {
            return new AttendanceOperationResponse
            {
                Success = false,
                Message = $"Failed to save attendance: {ex.Message}"
            };
        }
    }

    public async Task<(IEnumerable<AttendanceSummaryByStaffDto> StaffSummary, IEnumerable<DailyAttendanceSummaryDto> DailySummary)> GetAttendanceSummaryByCoursePlanSpAsync(int coursePlanId, int tenantId)
    {
        // PostgreSQL functions cannot return multiple result sets; call two separate functions
        var staffSummaryParams = new[]
        {
            new NpgsqlParameter("@CoursePlanId", coursePlanId),
            new NpgsqlParameter("@TenantId", tenantId)
        };

        var staffSummary = new List<AttendanceSummaryByStaffDto>();
        var dailySummary = new List<DailyAttendanceSummaryDto>();

        await _context.Database.OpenConnectionAsync();
        try
        {
            // First call: staff attendance summary
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "SELECT * FROM usp_GetAttendanceSummaryByCoursePlan_Staff(@CoursePlanId, @TenantId)";
                command.CommandType = System.Data.CommandType.Text;
                command.Parameters.AddRange(staffSummaryParams);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    staffSummary.Add(new AttendanceSummaryByStaffDto
                    {
                        StaffId = reader.GetInt32(reader.GetOrdinal("StaffId")),
                        StaffName = reader.IsDBNull(reader.GetOrdinal("StaffName")) ? string.Empty : reader.GetString(reader.GetOrdinal("StaffName")),
                        EmployeeCode = reader.IsDBNull(reader.GetOrdinal("EmployeeCode")) ? null : reader.GetString(reader.GetOrdinal("EmployeeCode")),
                        Department = reader.IsDBNull(reader.GetOrdinal("Department")) ? null : reader.GetString(reader.GetOrdinal("Department")),
                        Division = reader.IsDBNull(reader.GetOrdinal("Division")) ? null : reader.GetString(reader.GetOrdinal("Division")),
                        Position = reader.IsDBNull(reader.GetOrdinal("Position")) ? null : reader.GetString(reader.GetOrdinal("Position")),
                        StaffPhoto = reader.IsDBNull(reader.GetOrdinal("StaffPhoto")) ? null : reader.GetString(reader.GetOrdinal("StaffPhoto")),
                        TotalCourseDays = reader.GetInt32(reader.GetOrdinal("TotalCourseDays")),
                        DaysPresent = reader.GetInt32(reader.GetOrdinal("DaysPresent")),
                        DaysAbsent = reader.GetInt32(reader.GetOrdinal("DaysAbsent")),
                        DaysMarked = reader.GetInt32(reader.GetOrdinal("DaysMarked")),
                        DaysNotMarked = reader.GetInt32(reader.GetOrdinal("DaysNotMarked")),
                        AttendancePercentage = reader.GetDecimal(reader.GetOrdinal("AttendancePercentage"))
                    });
                }
            }

            // Second call: daily attendance summary
            var dailyParams = new[]
            {
                new NpgsqlParameter("@CoursePlanId", coursePlanId),
                new NpgsqlParameter("@TenantId", tenantId)
            };
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "SELECT * FROM usp_GetAttendanceSummaryByCoursePlan_Daily(@CoursePlanId, @TenantId)";
                command.CommandType = System.Data.CommandType.Text;
                command.Parameters.AddRange(dailyParams);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    dailySummary.Add(new DailyAttendanceSummaryDto
                    {
                        AttendanceDate = reader.GetDateTime(reader.GetOrdinal("AttendanceDate")),
                        TotalParticipants = reader.GetInt32(reader.GetOrdinal("TotalParticipants")),
                        PresentCount = reader.GetInt32(reader.GetOrdinal("PresentCount")),
                        AbsentCount = reader.GetInt32(reader.GetOrdinal("AbsentCount"))
                    });
                }
            }

            return (staffSummary, dailySummary);
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }

    #endregion
}
