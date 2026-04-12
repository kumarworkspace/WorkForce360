namespace HRMS.Core.Application.DTOs;

/// <summary>
/// Request to create/update date-wise attendance
/// </summary>
public class CreateAttendanceDateWiseRequest
{
    public int CoursePlanId { get; set; }
    public int StaffId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public bool IsPresent { get; set; } = true;
    public string? Remarks { get; set; }
}

/// <summary>
/// Request to update date-wise attendance by AttendanceId
/// </summary>
public class UpdateAttendanceDateWiseByIdRequest
{
    public int AttendanceId { get; set; }
    public bool IsPresent { get; set; }
    public string? Remarks { get; set; }
}

/// <summary>
/// Request for bulk attendance marking
/// </summary>
public class BulkAttendanceDateWiseRequest
{
    public int CoursePlanId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public List<AttendanceItemRequest> AttendanceItems { get; set; } = new();
}

/// <summary>
/// Individual attendance item for bulk operations
/// </summary>
public class AttendanceItemRequest
{
    public int StaffId { get; set; }
    public bool IsPresent { get; set; } = true;
    public string? Remarks { get; set; }
}

/// <summary>
/// Response from attendance CRUD operations
/// </summary>
public class AttendanceOperationResponse
{
    public int? AttendanceId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool Success { get; set; }
}

/// <summary>
/// DTO for attendance summary per staff
/// </summary>
public class AttendanceSummaryByStaffDto
{
    public int StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public string? EmployeeCode { get; set; }
    public string? Department { get; set; }
    public string? Division { get; set; }
    public string? Position { get; set; }
    public string? StaffPhoto { get; set; }
    public int TotalCourseDays { get; set; }
    public int DaysPresent { get; set; }
    public int DaysAbsent { get; set; }
    public int DaysMarked { get; set; }
    public int DaysNotMarked { get; set; }
    public decimal AttendancePercentage { get; set; }
}

/// <summary>
/// DTO for daily attendance summary
/// </summary>
public class DailyAttendanceSummaryDto
{
    public DateTime AttendanceDate { get; set; }
    public int TotalParticipants { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
}
