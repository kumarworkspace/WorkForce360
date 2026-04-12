namespace HRMS.Core.Application.DTOs;

public class AddCourseParticipantRequest
{
    public int CoursePlanId { get; set; }
    public List<int> StaffIds { get; set; } = new();
}

public class CourseParticipantDto
{
    public int CourseParticipantId { get; set; }
    public int CoursePlanId { get; set; }
    public int StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public string? EmployeeCode { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CourseDetailWithParticipantsDto
{
    public int CoursePlanId { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string? CourseCode { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal CourseDuration { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public string? Venue { get; set; }
    public int TenantId { get; set; }
    public List<CourseParticipantDto> Participants { get; set; } = new();
}

public class AttendanceDateWiseDto
{
    public int AttendanceId { get; set; }
    public int CoursePlanId { get; set; }
    public int StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public string? EmployeeCode { get; set; }
    public string? Department { get; set; }
    public string? Division { get; set; }
    public string? Position { get; set; }
    public string? StaffPhoto { get; set; }
    public DateTime AttendanceDate { get; set; }
    public bool IsPresent { get; set; }
    public string? Remarks { get; set; }
    public int TenantId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public int? UpdatedBy { get; set; }

    // Course details
    public DateTime CourseStartDate { get; set; }
    public DateTime CourseEndDate { get; set; }
    public string? CourseTitle { get; set; }
    public string? CourseNumber { get; set; }
    public string? TrainerName { get; set; }
}

public class UpdateAttendanceRequest
{
    public int CoursePlanId { get; set; }
    public int StaffId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public bool IsPresent { get; set; }
    public string? Remarks { get; set; }
}

public class CourseResultDto
{
    public int ResultId { get; set; }
    public int CoursePlanId { get; set; }
    public int StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public string? EmployeeCode { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public int TotalDays { get; set; }
    public int PresentDays { get; set; }
    public decimal AttendancePercentage { get; set; }
    public decimal? Marks { get; set; }
    public string? ResultStatus { get; set; } // Pass / Fail
    public string? CertificatePath { get; set; }
    public string? CertificateSerialNumber { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

public class UpdateResultRequest
{
    public int CoursePlanId { get; set; }
    public int StaffId { get; set; }
    public decimal? Marks { get; set; }
    public string ResultStatus { get; set; } = string.Empty; // Pass / Fail
}

public class AttendanceGridDto
{
    public int StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public string? EmployeeCode { get; set; }
    // Key = "yyyy-MM-dd" string — avoids all DateTimeKind/timezone comparison issues
    public Dictionary<string, bool> AttendanceByDate { get; set; } = new();
    public int TotalDays { get; set; }
    public int PresentDays { get; set; }
    public decimal AttendancePercentage { get; set; }
}

public class MyCourseSummaryDto
{
    public int CoursePlanId { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string? CourseCode { get; set; }
    public string? CourseType { get; set; }
    public string? CourseCategory { get; set; }
    public decimal CourseDuration { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Venue { get; set; } = string.Empty;
    public string TrainerName { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public bool IsActive { get; set; }
    public string? ResultStatus { get; set; }
    public decimal AttendancePercentage { get; set; }
    public decimal? Marks { get; set; }
    public string? CertificatePath { get; set; }
    public string? CertificateSerialNumber { get; set; }

    public string Status
    {
        get
        {
            var today = DateTime.Now.Date;
            if (today < StartDate.Date) return "Upcoming";
            if (today > EndDate.Date) return "Completed";
            return "Ongoing";
        }
    }
}

public class StaffCertificateDto
{
    public int ResultId { get; set; }
    public int CoursePlanId { get; set; }
    public int StaffId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string? CourseCode { get; set; }
    public DateTime CourseStartDate { get; set; }
    public DateTime CourseEndDate { get; set; }
    public string? CertificatePath { get; set; }
    public string? CertificateSerialNumber { get; set; }
    public string? ResultStatus { get; set; }
    public DateTime? IssuedDate { get; set; }
}
