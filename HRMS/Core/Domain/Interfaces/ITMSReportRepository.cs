using HRMS.Core.Application.DTOs;

namespace HRMS.Core.Domain.Interfaces;

public interface ITMSReportRepository
{
    Task<GeneralReportResultDto> GetGeneralReportAsync(GeneralReportFilter filter);
    Task<TrainerKPIResultDto> GetTrainerKPIReportAsync(TrainerKPIFilter filter);
    Task<StatisticsResultDto> GetStatisticsReportAsync(StatisticsFilter filter);
}
