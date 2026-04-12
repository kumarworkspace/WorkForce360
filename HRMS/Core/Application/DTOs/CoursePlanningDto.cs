namespace HRMS.Core.Application.DTOs;

public class CreateCoursePlanningRequest
{
    public int CourseId { get; set; }
    public DateTime StartDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Venue { get; set; } = string.Empty;
    public int TrainerId { get; set; }
    public string? Remarks { get; set; }
    public List<string>? UploadFilePaths { get; set; }
}

public class UpdateCoursePlanningRequest : CreateCoursePlanningRequest
{
    public int Id { get; set; }
}

public class CoursePlanningListDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string CourseNumber { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Venue { get; set; } = string.Empty;
    public int TrainerId { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public string? TrainerEmail { get; set; }
    public string? CourseType { get; set; }
    public string? CourseCategory { get; set; }
    public decimal CourseDuration { get; set; }
    public bool IsActive { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CoursePlanningDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string CourseNumber { get; set; } = string.Empty;
    public string? TrainingModule { get; set; }
    public DateTime StartDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Venue { get; set; } = string.Empty;
    public int TrainerId { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public string? TrainerEmail { get; set; }
    public string? TrainerPhone { get; set; }
    public string? Remarks { get; set; }
    public List<string> UploadFilePaths { get; set; } = new();
    public string? QRCodePath { get; set; }
    public string? CourseType { get; set; }
    public string? CourseCategory { get; set; }
    public decimal CourseDuration { get; set; }
    public int TenantId { get; set; }
    public bool IsActive { get; set; }
    public bool IsCompleted { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

public class ConflictValidationRequest
{
    public int? Id { get; set; }
    public int TrainerId { get; set; }
    public DateTime StartDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan EndTime { get; set; }
    public int TenantId { get; set; }
}

public class ConflictValidationResult
{
    public bool HasConflict { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<CoursePlanningListDto> ConflictingSchedules { get; set; } = new();
}
