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

    public async Task<GeneralReportResultDto> GetGeneralReportAsync(GeneralReportFilter filter)
    {
        try { return await _repo.GetGeneralReportAsync(filter); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching General Report for Tenant {TenantId}", filter.TenantId);
            return new GeneralReportResultDto();
        }
    }

    public async Task<TrainerKPIResultDto> GetTrainerKPIReportAsync(TrainerKPIFilter filter)
    {
        try { return await _repo.GetTrainerKPIReportAsync(filter); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Trainer KPI Report for Tenant {TenantId}", filter.TenantId);
            return new TrainerKPIResultDto();
        }
    }

    public async Task<StatisticsResultDto> GetStatisticsReportAsync(StatisticsFilter filter)
    {
        try { return await _repo.GetStatisticsReportAsync(filter); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Statistics Report for Tenant {TenantId}", filter.TenantId);
            return new StatisticsResultDto();
        }
    }
}
