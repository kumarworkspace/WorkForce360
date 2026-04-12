using HRMS.Core.Application.DTOs;
using HRMS.Core.Application.Interfaces;
using HRMS.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Core.Application.Services;

public class TMSReportService : ITMSReportService
{
    private readonly ITMSReportRepository _repo;
    private readonly ILogger<TMSReportService> _logger;

    public TMSReportService(ITMSReportRepository repo, ILogger<TMSReportService> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<TMSOverallSummaryDto> GetOverallSummaryAsync(TMSReportFilter filter)
    {
        try
        {
            return await _repo.GetOverallSummaryAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching TMS overall summary for Tenant {TenantId}", filter.TenantId);
            return new TMSOverallSummaryDto();
        }
    }

    public async Task<IEnumerable<TMSMonthlySummaryDto>> GetMonthlySummaryAsync(TMSReportFilter filter)
    {
        try
        {
            return await _repo.GetMonthlySummaryAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching TMS monthly summary for Tenant {TenantId}", filter.TenantId);
            return Enumerable.Empty<TMSMonthlySummaryDto>();
        }
    }

    public async Task<IEnumerable<TMSTrainerPerformanceDto>> GetTrainerPerformanceAsync(TMSReportFilter filter)
    {
        try
        {
            return await _repo.GetTrainerPerformanceAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching TMS trainer performance for Tenant {TenantId}", filter.TenantId);
            return Enumerable.Empty<TMSTrainerPerformanceDto>();
        }
    }

    public async Task<IEnumerable<TMSCourseWiseReportDto>> GetCourseWiseReportAsync(TMSReportFilter filter)
    {
        try
        {
            return await _repo.GetCourseWiseReportAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching TMS course-wise report for Tenant {TenantId}", filter.TenantId);
            return Enumerable.Empty<TMSCourseWiseReportDto>();
        }
    }
}
