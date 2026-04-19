namespace HRMS.Core.Application.DTOs;

// ── LMS Course ──────────────────────────────────────────────────────────────

public class LmsCourseDto
{
    public int LmsCourseId { get; set; }
    public int TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? CourseTypeId { get; set; }
    public string? CourseTypeName { get; set; }
    public int? DifficultyId { get; set; }
    public string? DifficultyName { get; set; }
    public string? ContentUrl { get; set; }
    public int DurationMinutes { get; set; }
    public string? Tags { get; set; }
    public string? ThumbnailPath { get; set; }
    public string? Objectives { get; set; }
    public bool IsActive { get; set; }
    public int ModuleCount { get; set; }
    public int EnrollmentCount { get; set; }
}

public class CreateLmsCourseRequest
{
    public int TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? CourseTypeId { get; set; }
    public int? DifficultyId { get; set; }
    public string? ContentUrl { get; set; }
    public int DurationMinutes { get; set; }
    public string? Tags { get; set; }
    public string? ThumbnailPath { get; set; }
    public string? Objectives { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class UpdateLmsCourseRequest : CreateLmsCourseRequest
{
    public int LmsCourseId { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
}

// ── LMS Module ───────────────────────────────────────────────────────────────

public class LmsModuleDto
{
    public int LmsModuleId { get; set; }
    public int LmsCourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ContentUrl { get; set; }
    public string? ContentType { get; set; }
    public int DurationMinutes { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

public class CreateLmsModuleRequest
{
    public int LmsCourseId { get; set; }
    public int TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ContentUrl { get; set; }
    public string? ContentType { get; set; }
    public int DurationMinutes { get; set; }
    public int SortOrder { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

// ── Learning Path ─────────────────────────────────────────────────────────────

public class LearningPathDto
{
    public int LearningPathId { get; set; }
    public int TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? JobTitleId { get; set; }
    public string? JobTitleName { get; set; }
    public bool IsActive { get; set; }
    public List<LearningPathCourseDto> Courses { get; set; } = new();
}

public class LearningPathCourseDto
{
    public int LearningPathCourseId { get; set; }
    public int LmsCourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public int DurationMinutes { get; set; }
}

public class CreateLearningPathRequest
{
    public int TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? JobTitleId { get; set; }
    public List<int> CourseIds { get; set; } = new();
    public string CreatedBy { get; set; } = string.Empty;
}

// ── Enrollment ────────────────────────────────────────────────────────────────

public class EnrollmentDto
{
    public int EnrollmentId { get; set; }
    public int StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public int LmsCourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string? CourseTypeName { get; set; }
    public int DurationMinutes { get; set; }
    public DateTime EnrolledDate { get; set; }
    public string Status { get; set; } = "Enrolled";
    public DateTime? CompletedDate { get; set; }
    public decimal OverallProgress { get; set; }
    public bool IsActive { get; set; }
    public int TenantId { get; set; }
}

public class EnrollRequest
{
    public int StaffId { get; set; }
    public int LmsCourseId { get; set; }
    public int TenantId { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

// ── Progress Tracking ─────────────────────────────────────────────────────────

public class ProgressTrackingDto
{
    public int ProgressTrackingId { get; set; }
    public int EnrollmentId { get; set; }
    public int LmsModuleId { get; set; }
    public string ModuleTitle { get; set; } = string.Empty;
    public decimal ProgressPct { get; set; }
    public DateTime? LastAccessedDate { get; set; }
    public bool IsCompleted { get; set; }
}

public class UpdateProgressRequest
{
    public int EnrollmentId { get; set; }
    public int LmsModuleId { get; set; }
    public int TenantId { get; set; }
    public decimal ProgressPct { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
}
