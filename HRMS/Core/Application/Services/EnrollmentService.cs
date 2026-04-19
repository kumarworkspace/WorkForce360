using HRMS.Core.Application.DTOs;
using HRMS.Core.Application.Interfaces;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Core.Application.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(IUnitOfWork uow, ILogger<EnrollmentService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<IEnumerable<EnrollmentDto>> GetByStaffAsync(int staffId, int tenantId)
    {
        var enrollments = await _uow.Enrollment.GetByStaffIdAsync(staffId, tenantId);
        return await MapToDtoListAsync(enrollments);
    }

    public async Task<IEnumerable<EnrollmentDto>> GetByCourseAsync(int courseId, int tenantId)
    {
        var enrollments = await _uow.Enrollment.GetByCourseIdAsync(courseId, tenantId);
        return await MapToDtoListAsync(enrollments);
    }

    public async Task<EnrollmentDto?> GetByIdAsync(int enrollmentId, int tenantId)
    {
        var e = await _uow.Enrollment.GetByIdAsync(enrollmentId, tenantId);
        if (e == null) return null;
        var progress = await _uow.ProgressTracking.GetOverallProgressAsync(enrollmentId);
        return MapToDto(e, progress);
    }

    public async Task<(bool Success, string Message, int EnrollmentId)> EnrollAsync(EnrollRequest req)
    {
        try
        {
            var existing = await _uow.Enrollment.GetByStaffAndCourseAsync(req.StaffId, req.LmsCourseId, req.TenantId);
            if (existing != null && existing.IsActive)
                return (false, "Staff is already enrolled in this course.", 0);

            if (existing != null && !existing.IsActive)
            {
                existing.IsActive    = true;
                existing.Status      = "Enrolled";
                existing.EnrolledDate = DateTime.UtcNow;
                existing.CompletedDate = null;
                existing.UpdatedBy   = req.CreatedBy;
                existing.UpdatedDate = DateTime.UtcNow;
                await _uow.Enrollment.UpdateAsync(existing);
                await _uow.SaveChangesAsync();
                return (true, "Re-enrolled successfully.", existing.EnrollmentId);
            }

            var enrollment = new Enrollment
            {
                StaffId      = req.StaffId,
                LmsCourseId  = req.LmsCourseId,
                TenantId     = req.TenantId,
                EnrolledDate = DateTime.UtcNow,
                Status       = "Enrolled",
                IsActive     = true,
                CreatedBy    = req.CreatedBy,
                CreatedDate  = DateTime.UtcNow
            };
            await _uow.Enrollment.AddAsync(enrollment);
            await _uow.SaveChangesAsync();
            return (true, "Enrolled successfully.", enrollment.EnrollmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enrolling staff {StaffId} in course {CourseId}", req.StaffId, req.LmsCourseId);
            return (false, "Failed to enroll.", 0);
        }
    }

    public async Task<(bool Success, string Message)> WithdrawAsync(int enrollmentId, int tenantId, string updatedBy)
    {
        try
        {
            var enrollment = await _uow.Enrollment.GetByIdAsync(enrollmentId, tenantId);
            if (enrollment == null) return (false, "Enrollment not found.");

            enrollment.Status      = "Withdrawn";
            enrollment.IsActive    = false;
            enrollment.UpdatedBy   = updatedBy;
            enrollment.UpdatedDate = DateTime.UtcNow;
            await _uow.Enrollment.UpdateAsync(enrollment);
            await _uow.SaveChangesAsync();
            return (true, "Withdrawn successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error withdrawing enrollment {EnrollmentId}", enrollmentId);
            return (false, "Failed to withdraw.");
        }
    }

    public async Task<(bool Success, string Message)> UpdateProgressAsync(UpdateProgressRequest req)
    {
        try
        {
            var enrollment = await _uow.Enrollment.GetByIdAsync(req.EnrollmentId, req.TenantId);
            if (enrollment == null) return (false, "Enrollment not found.");

            var record = await _uow.ProgressTracking.GetByEnrollmentAndModuleAsync(req.EnrollmentId, req.LmsModuleId);
            if (record == null)
            {
                record = new ProgressTracking
                {
                    EnrollmentId    = req.EnrollmentId,
                    LmsModuleId     = req.LmsModuleId,
                    TenantId        = req.TenantId,
                    ProgressPct     = req.ProgressPct,
                    IsCompleted     = req.ProgressPct >= 100,
                    LastAccessedDate = DateTime.UtcNow,
                    IsActive        = true,
                    CreatedBy       = req.UpdatedBy,
                    CreatedDate     = DateTime.UtcNow
                };
                await _uow.ProgressTracking.AddAsync(record);
            }
            else
            {
                record.ProgressPct      = req.ProgressPct;
                record.IsCompleted      = req.ProgressPct >= 100;
                record.LastAccessedDate = DateTime.UtcNow;
                record.UpdatedBy        = req.UpdatedBy;
                record.UpdatedDate      = DateTime.UtcNow;
                await _uow.ProgressTracking.UpdateAsync(record);
            }

            // Update enrollment status
            var overall = await _uow.ProgressTracking.GetOverallProgressAsync(req.EnrollmentId);
            if (overall >= 100)
            {
                enrollment.Status        = "Completed";
                enrollment.CompletedDate = DateTime.UtcNow;
            }
            else if (overall > 0)
            {
                enrollment.Status = "InProgress";
            }
            enrollment.UpdatedBy   = req.UpdatedBy;
            enrollment.UpdatedDate = DateTime.UtcNow;
            await _uow.Enrollment.UpdateAsync(enrollment);

            await _uow.SaveChangesAsync();
            return (true, "Progress updated.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating progress for enrollment {EnrollmentId}", req.EnrollmentId);
            return (false, "Failed to update progress.");
        }
    }

    public async Task<IEnumerable<ProgressTrackingDto>> GetProgressAsync(int enrollmentId, int tenantId)
    {
        var records = await _uow.ProgressTracking.GetByEnrollmentIdAsync(enrollmentId);
        return records.Select(p => new ProgressTrackingDto
        {
            ProgressTrackingId = p.ProgressTrackingId,
            EnrollmentId       = p.EnrollmentId,
            LmsModuleId        = p.LmsModuleId,
            ModuleTitle        = p.Module?.Title ?? string.Empty,
            ProgressPct        = p.ProgressPct,
            LastAccessedDate   = p.LastAccessedDate,
            IsCompleted        = p.IsCompleted
        });
    }

    private async Task<IEnumerable<EnrollmentDto>> MapToDtoListAsync(IEnumerable<Enrollment> enrollments)
    {
        var list = new List<EnrollmentDto>();
        foreach (var e in enrollments)
        {
            var progress = await _uow.ProgressTracking.GetOverallProgressAsync(e.EnrollmentId);
            list.Add(MapToDto(e, progress));
        }
        return list;
    }

    private static EnrollmentDto MapToDto(Enrollment e, decimal overallProgress) => new()
    {
        EnrollmentId    = e.EnrollmentId,
        StaffId         = e.StaffId,
        StaffName       = e.Staff?.Name ?? string.Empty,
        LmsCourseId     = e.LmsCourseId,
        CourseTitle     = e.Course?.Title ?? string.Empty,
        CourseTypeName  = e.Course?.CourseType?.ValueName,
        DurationMinutes = e.Course?.DurationMinutes ?? 0,
        EnrolledDate    = e.EnrolledDate,
        Status          = e.Status,
        CompletedDate   = e.CompletedDate,
        OverallProgress = overallProgress,
        IsActive        = e.IsActive,
        TenantId        = e.TenantId
    };
}
