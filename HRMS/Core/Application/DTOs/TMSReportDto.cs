namespace HRMS.Core.Application.DTOs;

// ─── Filters ─────────────────────────────────────────────────────────────────

public class GeneralReportFilter
{
    public int TenantId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? CourseId { get; set; }
    public int? TrainerId { get; set; }
    public string? Department { get; set; }
    public string? Company { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class TrainerKPIFilter
{
    public int TenantId { get; set; }
    public int? Year { get; set; }
    public int? Month { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? TrainerId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class StatisticsFilter
{
    public int TenantId { get; set; }
    public string? Department { get; set; }
    public string? Company { get; set; }
    public int? Year { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

// ─── General Report ──────────────────────────────────────────────────────────

public class GeneralReportSummaryDto
{
    public long TotalClasses { get; set; }
    public long TotalStaffAttended { get; set; }
    public decimal TotalHours { get; set; }
}

public class GeneralReportRowDto
{
    public long RowNo { get; set; }
    public int CoursePlanId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CourseCode { get; set; }
    public string? CourseType { get; set; }
    public decimal TotalHours { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public string? Venue { get; set; }
    public long TotalStaffAttended { get; set; }

    public string TimeRange => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm} Hrs";
}

public class GeneralReportResultDto
{
    public GeneralReportSummaryDto Summary { get; set; } = new();
    public long TotalCount { get; set; }
    public List<GeneralReportRowDto> Rows { get; set; } = new();
}

// ─── Trainer KPI Report ──────────────────────────────────────────────────────

public class TrainerKPISummaryDto
{
    public long TotalClasses { get; set; }
    public decimal TotalHours { get; set; }
}

public class TrainerKPIRowDto
{
    public long RowNo { get; set; }
    public int StaffId { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public string? EmployeeCode { get; set; }
    public string? Department { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public long NumClasses { get; set; }
    public decimal TotalHours { get; set; }
}

public class TrainerKPIResultDto
{
    public TrainerKPISummaryDto Summary { get; set; } = new();
    public long TotalCount { get; set; }
    public List<TrainerKPIRowDto> Rows { get; set; } = new();
}

// ─── Statistics Report ───────────────────────────────────────────────────────

public class StatisticsSummaryDto
{
    public long TotalSessions { get; set; }
    public long TotalEnrolled { get; set; }
    public long TotalPresent { get; set; }
    public decimal AttendancePct { get; set; }
}

public class StatisticsRowDto
{
    public long RowNo { get; set; }
    public string Department { get; set; } = string.Empty;
    public string CourseType { get; set; } = string.Empty;
    public long TotalSessions { get; set; }
    public long TotalEnrolled { get; set; }
    public long TotalPresent { get; set; }
    public decimal AttendancePct { get; set; }
}

public class StatisticsResultDto
{
    public StatisticsSummaryDto Summary { get; set; } = new();
    public long TotalCount { get; set; }
    public List<StatisticsRowDto> Rows { get; set; } = new();
}
