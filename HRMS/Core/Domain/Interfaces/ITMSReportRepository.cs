using HRMS.Core.Application.DTOs;

namespace HRMS.Core.Domain.Interfaces;

public interface ITMSReportRepository
{
    Task<TMSOverallSummaryDto> GetOverallSummaryAsync(TMSReportFilter filter);
    Task<IEnumerable<TMSMonthlySummaryDto>> GetMonthlySummaryAsync(TMSReportFilter filter);
    Task<IEnumerable<TMSTrainerPerformanceDto>> GetTrainerPerformanceAsync(TMSReportFilter filter);
    Task<IEnumerable<TMSCourseWiseReportDto>> GetCourseWiseReportAsync(TMSReportFilter filter);
}
