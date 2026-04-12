using HRMS.Core.Application.DTOs;

namespace HRMS.Core.Application.Interfaces;

public interface ITMSReportService
{
    Task<TMSOverallSummaryDto> GetOverallSummaryAsync(TMSReportFilter filter);
    Task<IEnumerable<TMSMonthlySummaryDto>> GetMonthlySummaryAsync(TMSReportFilter filter);
    Task<IEnumerable<TMSTrainerPerformanceDto>> GetTrainerPerformanceAsync(TMSReportFilter filter);
    Task<IEnumerable<TMSCourseWiseReportDto>> GetCourseWiseReportAsync(TMSReportFilter filter);
}
