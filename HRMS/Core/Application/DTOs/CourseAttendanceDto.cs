namespace HRMS.Core.Application.DTOs;

public class MarkAttendanceRequest
{
    public int UserId { get; set; }
    public int CoursePlanId { get; set; }
    public int StaffId { get; set; }
    public TimeSpan? CheckInTime { get; set; }
}

public class MarkAttendanceResponse
{
    public int AttendanceId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool Success { get; set; }
}

public class CourseAttendanceDto
{
    public int AttendanceId { get; set; }
    public int UserId { get; set; }
    public int CoursePlanId { get; set; }
    public int StaffId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public int TenantId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public int? UpdatedBy { get; set; }

    // User details
    public string UserName { get; set; } = string.Empty;
    public string? UserEmail { get; set; }

    // Staff details
    public string StaffName { get; set; } = string.Empty;
    public string? EmployeeCode { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }

    // Course Plan details
    public DateTime CourseStartDate { get; set; }
    public DateTime CourseEndDate { get; set; }
    public string? Venue { get; set; }

    // Course details
    public string CourseTitle { get; set; } = string.Empty;
    public string? CourseCode { get; set; }
    public string? CourseNumber { get; set; }

    // Trainer details
    public string? TrainerName { get; set; }
}

public class AttendanceSummaryDto
{
    public int AttendanceId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StaffName { get; set; } = string.Empty;
    public string? EmployeeCode { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public string? Email { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserEmail { get; set; }
    public string? Remarks { get; set; }
}

public class GetAttendanceRequest
{
    public int TenantId { get; set; }
    public int? CoursePlanId { get; set; }
    public int? UserId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
