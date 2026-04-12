using Microsoft.EntityFrameworkCore;
using Npgsql;
using HRMS.Core.Application.DTOs;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;

namespace HRMS.Infrastructure.Repositories;

public class CourseResultRepository : Repository<CourseResult>, ICourseResultRepository
{
    public CourseResultRepository(HRMSDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CourseResultDto>> GetResultSummaryByCoursePlanAsync(int coursePlanId, int tenantId)
    {
        var result = await _dbSet
            .Where(cr => cr.CoursePlanId == coursePlanId && cr.TenantId == tenantId && cr.IsActive)
            .Include(cr => cr.Staff)
            .OrderBy(cr => cr.Staff!.Name)
            .Select(cr => new CourseResultDto
            {
                ResultId = cr.ResultId,
                CoursePlanId = cr.CoursePlanId,
                StaffId = cr.StaffId,
                StaffName = cr.Staff != null ? cr.Staff.Name ?? string.Empty : string.Empty,
                EmployeeCode = cr.Staff != null ? cr.Staff.EmployeeCode : null,
                Department = cr.Staff != null ? cr.Staff.Department : null,
                Position = cr.Staff != null ? cr.Staff.Position : null,
                TotalDays = cr.TotalDays,
                PresentDays = cr.PresentDays,
                AttendancePercentage = cr.AttendancePercentage,
                Marks = cr.Marks,
                ResultStatus = cr.ResultStatus,
                CertificatePath = cr.CertificatePath,
                CertificateSerialNumber = cr.CertificateSerialNumber,
                UpdatedDate = cr.UpdatedDate
            })
            .AsNoTracking()
            .ToListAsync();

        return result;
    }

    public async Task<CourseResult?> GetByCoursePlanStaffAsync(int coursePlanId, int staffId, int tenantId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(cr => cr.CoursePlanId == coursePlanId
                && cr.StaffId == staffId
                && cr.TenantId == tenantId
                && cr.IsActive);
    }

    public async Task<IEnumerable<StaffCertificateDto>> GetCertificatesByStaffIdAsync(int staffId, int tenantId)
    {
        var parameters = new[]
        {
            new NpgsqlParameter("@StaffId", staffId),
            new NpgsqlParameter("@TenantId", tenantId)
        };

        var result = await _context.Set<StaffCertificateDto>()
            .FromSqlRaw(@"
                SELECT
                    cr.""ResultId"",
                    cr.""CoursePlanId"",
                    cr.""StaffId"",
                    COALESCE(c.""Title"", '') AS ""CourseName"",
                    c.""Code"" AS ""CourseCode"",
                    cp.""StartDate"" AS ""CourseStartDate"",
                    cp.""EndDate"" AS ""CourseEndDate"",
                    cr.""CertificatePath"",
                    cr.""CertificateSerialNumber"",
                    cr.""ResultStatus"",
                    cr.""UpdatedDate"" AS ""IssuedDate""
                FROM ""CourseResult"" cr
                INNER JOIN ""CoursePlanning"" cp ON cr.""CoursePlanId"" = cp.""Id""
                INNER JOIN ""CourseRegistration"" c ON cp.""CourseId"" = c.""CourseId""
                WHERE cr.""StaffId"" = @StaffId
                    AND cr.""TenantId"" = @TenantId
                    AND cr.""IsActive"" = TRUE
                    AND cr.""ResultStatus"" = 'Pass'
                    AND cr.""CertificatePath"" IS NOT NULL
                ORDER BY cp.""EndDate"" DESC", parameters)
            .AsNoTracking()
            .ToListAsync();

        return result;
    }
}
