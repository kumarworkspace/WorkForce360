using HRMS.Core.Application.DTOs;

namespace HRMS.Core.Application.Interfaces;

public interface ITMSReportService
{
    Task<GeneralReportResultDto> GetGeneralReportAsync(GeneralReportFilter filter);
    Task<TrainerKPIResultDto> GetTrainerKPIReportAsync(TrainerKPIFilter filter);
    Task<StatisticsResultDto> GetStatisticsReportAsync(StatisticsFilter filter);
}
