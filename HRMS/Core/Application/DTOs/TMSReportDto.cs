namespace HRMS.Core.Application.DTOs;

// ─── Filter ──────────────────────────────────────────────────────────────────

public class TMSReportFilter
{
    public int TenantId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? TrainerId { get; set; }
    public int? CourseId { get; set; }
}

// ─── Overall Summary ──────────────────────────────────────────────────────────

public class TMSOverallSummaryDto
{
    public int TotalCourses { get; set; }
    public int TotalSessions { get; set; }
    public int TotalParticipantsEnrolled { get; set; }
    public int TotalPresent { get; set; }
    public decimal AvgAttendancePercentage { get; set; }
    public int CoursesCompleted { get; set; }
    public int CoursesOngoing { get; set; }
    public int CoursesUpcoming { get; set; }
}

// ─── Monthly / Yearly Summary ─────────────────────────────────────────────────

public class TMSMonthlySummaryDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public int TotalParticipants { get; set; }
    public int TotalPresent { get; set; }
    public decimal AvgAttendancePercentage { get; set; }
}

// ─── Trainer Performance ──────────────────────────────────────────────────────

public class TMSTrainerPerformanceDto
{
    public int StaffId { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public string? EmployeeCode { get; set; }
    public string? Department { get; set; }
    public int TotalCoursesConducted { get; set; }
    public int TotalParticipantsTrained { get; set; }
    public decimal AvgAttendancePercentage { get; set; }
}

// ─── Course-wise Report ───────────────────────────────────────────────────────

public class TMSCourseWiseReportDto
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string? CourseCode { get; set; }
    public string? Category { get; set; }
    public int TotalSessions { get; set; }
    public int TotalParticipantsEnrolled { get; set; }
    public int TotalPresent { get; set; }
    public decimal AvgAttendancePercentage { get; set; }
}
