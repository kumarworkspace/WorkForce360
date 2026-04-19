using Npgsql;
using NpgsqlTypes;
using HRMS.Core.Application.DTOs;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class TMSReportRepository : ITMSReportRepository
{
    private readonly HRMSDbContext _context;

    public TMSReportRepository(HRMSDbContext context)
    {
        _context = context;
    }

    public async Task<GeneralReportResultDto> GetGeneralReportAsync(GeneralReportFilter filter)
    {
        var result = new GeneralReportResultDto();

        await _context.Database.OpenConnectionAsync();
        try
        {
            using var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = "SELECT * FROM usp_GetTMSGeneralReport($1,$2,$3,$4,$5,$6,$7,$8,$9)";

            cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Integer, Value = filter.TenantId });
            cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Date,    Value = filter.FromDate.HasValue ? (object)filter.FromDate.Value.Date : DBNull.Value });
            cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Date,    Value = filter.ToDate.HasValue   ? (object)filter.ToDate.Value.Date   : DBNull.Value });
            cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Integer, Value = filter.CourseId.HasValue  ? (object)filter.CourseId.Value  : DBNull.Value });
            cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Integer, Value = filter.TrainerId.HasValue ? (object)filter.TrainerId.Value : DBNull.Value });
            cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Varchar, Value = string.IsNullOrWhiteSpace(filter.Department) ? DBNull.Value : filter.Department });
            cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Varchar, Value = string.IsNullOrWhiteSpace(filter.Company)    ? DBNull.Value : filter.Company });
            cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Integer, Value = filter.PageNumber });
            cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Integer, Value = filter.PageSize });

            using var reader = await cmd.ExecuteReaderAsync();
            bool summarySet = false;

            while (await reader.ReadAsync())
            {
                if (!summarySet)
                {
                    result.TotalCount = reader.GetInt64(reader.GetOrdinal("TotalCount"));
                    result.Summary = new GeneralReportSummaryDto
                    {
                        TotalClasses       = reader.GetInt64(reader.GetOrdinal("TotalClasses")),
                        TotalStaffAttended = reader.GetInt64(reader.GetOrdinal("GrandStaffAttended")),
                        TotalHours         = reader.IsDBNull(reader.GetOrdinal("GrandTotalHours")) ? 0 : reader.GetDecimal(reader.GetOrdinal("GrandTotalHours"))
                    };
                    summarySet = true;
                }

                var startTimeRaw = reader.GetValue(reader.GetOrdinal("StartTime"));
                var endTimeRaw   = reader.GetValue(reader.GetOrdinal("EndTime"));

                result.Rows.Add(new GeneralReportRowDto
                {
                    RowNo              = reader.GetInt64(reader.GetOrdinal("RowNo")),
                    CoursePlanId       = reader.GetInt32(reader.GetOrdinal("CoursePlanId")),
                    StartDate          = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                    EndDate            = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                    StartTime          = startTimeRaw is TimeSpan st ? st : TimeSpan.Zero,
                    EndTime            = endTimeRaw   is TimeSpan et ? et : TimeSpan.Zero,
                    Title              = reader.GetString(reader.GetOrdinal("Title")),
                    CourseCode         = reader.IsDBNull(reader.GetOrdinal("CourseCode"))  ? null : reader.GetString(reader.GetOrdinal("CourseCode")),
                    CourseType         = reader.IsDBNull(reader.GetOrdinal("CourseType"))  ? null : reader.GetString(reader.GetOrdinal("CourseType")),
                    TotalHours         = reader.IsDBNull(reader.GetOrdinal("TotalHours"))  ? 0    : reader.GetDecimal(reader.GetOrdinal("TotalHours")),
                    TrainerName        = reader.GetString(reader.GetOrdinal("TrainerName")),
                    Venue              = reader.IsDBNull(reader.GetOrdinal("Venue"))       ? null : reader.GetString(reader.GetOrdinal("Venue")),
                    TotalStaffAttended = reader.GetInt64(reader.GetOrdinal("TotalStaffAttended"))
                });
            }
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }

        return result;
    }

    public async Task<TrainerKPIResultDto> GetTrainerKPIReportAsync(TrainerKPIFilter filter)
    {
        var result = new TrainerKPIResultDto();

        await _context.Database.OpenConnectionAsync();
        try
        {
            using var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = "SELECT * FROM usp_GetTMSTrainerKPIReport($1,$2,$3,$4,$5,$6,$7,$8)";

            cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Integer, Value = filter.TenantId });
            cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Integer, Value = filter.Year.HasValue  ? (object)filter.Year.Value  : DBNull.Value });
            cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Integer, Value = filter.Month.HasValue ? (object)filter.Month.Value : DBNull.Value });
            cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Date,    Value = filter.FromDate.HasValue  ? (object)filter.FromDate.Value.Date  : DBNull.Value });
            cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Date,    Value = filter.ToDate.HasValue    ? (object)filter.ToDate.Value.Date    : DBNull.Value });
            cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Integer, Value = filter.TrainerId.HasValue ? (object)filter.TrainerId.Value : DBNull.Value });
            cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Integer, Value = filter.PageNumber });
            cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Integer, Value = filter.PageSize });

            using var reader = await cmd.ExecuteReaderAsync();
            bool summarySet = false;

            while (await reader.ReadAsync())
            {
                if (!summarySet)
                {
                    result.TotalCount = reader.GetInt64(reader.GetOrdinal("TotalCount"));
                    result.Summary = new TrainerKPISummaryDto
                    {
                        TotalClasses = reader.GetInt64(reader.GetOrdinal("GrandClasses")),
                        TotalHours   = reader.IsDBNull(reader.GetOrdinal("GrandHours")) ? 0 : reader.GetDecimal(reader.GetOrdinal("GrandHours"))
                    };
                    summarySet = true;
                }

                result.Rows.Add(new TrainerKPIRowDto
                {
                    RowNo        = reader.GetInt64(reader.GetOrdinal("RowNo")),
                    StaffId      = reader.GetInt32(reader.GetOrdinal("StaffId")),
                    TrainerName  = reader.GetString(reader.GetOrdinal("TrainerName")),
                    EmployeeCode = reader.IsDBNull(reader.GetOrdinal("EmployeeCode")) ? null : reader.GetString(reader.GetOrdinal("EmployeeCode")),
                    Department   = reader.IsDBNull(reader.GetOrdinal("Department"))   ? null : reader.GetString(reader.GetOrdinal("Department")),
                    Year         = reader.GetInt32(reader.GetOrdinal("Year")),
                    Month        = reader.GetInt32(reader.GetOrdinal("Month")),
                    MonthName    = reader.GetString(reader.GetOrdinal("MonthName")),
                    NumClasses   = reader.GetInt64(reader.GetOrdinal("NumClasses")),
                    TotalHours   = reader.IsDBNull(reader.GetOrdinal("TotalHours")) ? 0 : reader.GetDecimal(reader.GetOrdinal("TotalHours"))
                });
            }
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }

        return result;
    }

    public async Task<StatisticsResultDto> GetStatisticsReportAsync(StatisticsFilter filter)
    {
        var result = new StatisticsResultDto();

        await _context.Database.OpenConnectionAsync();
        try
        {
            using var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = "SELECT * FROM usp_GetTMSStatisticsReport($1,$2,$3,$4,$5,$6)";

            cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Integer, Value = filter.TenantId });
            cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Varchar, Value = string.IsNullOrWhiteSpace(filter.Department) ? DBNull.Value : filter.Department });
            cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Varchar, Value = string.IsNullOrWhiteSpace(filter.Company)    ? DBNull.Value : filter.Company });
            cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Integer, Value = filter.Year.HasValue ? (object)filter.Year.Value : DBNull.Value });
            cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Integer, Value = filter.PageNumber });
            cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Integer, Value = filter.PageSize });

            using var reader = await cmd.ExecuteReaderAsync();
            bool summarySet = false;

            while (await reader.ReadAsync())
            {
                if (!summarySet)
                {
                    result.TotalCount = reader.GetInt64(reader.GetOrdinal("TotalCount"));
                    result.Summary = new StatisticsSummaryDto
                    {
                        TotalSessions  = reader.GetInt64(reader.GetOrdinal("GrandSessions")),
                        TotalEnrolled  = reader.GetInt64(reader.GetOrdinal("GrandEnrolled")),
                        TotalPresent   = reader.GetInt64(reader.GetOrdinal("GrandPresent")),
                        AttendancePct  = reader.IsDBNull(reader.GetOrdinal("GrandAttendancePct")) ? 0 : reader.GetDecimal(reader.GetOrdinal("GrandAttendancePct"))
                    };
                    summarySet = true;
                }

                result.Rows.Add(new StatisticsRowDto
                {
                    RowNo          = reader.GetInt64(reader.GetOrdinal("RowNo")),
                    Department     = reader.GetString(reader.GetOrdinal("Department")),
                    CourseType     = reader.GetString(reader.GetOrdinal("CourseType")),
                    TotalSessions  = reader.GetInt64(reader.GetOrdinal("TotalSessions")),
                    TotalEnrolled  = reader.GetInt64(reader.GetOrdinal("TotalEnrolled")),
                    TotalPresent   = reader.GetInt64(reader.GetOrdinal("TotalPresent")),
                    AttendancePct  = reader.IsDBNull(reader.GetOrdinal("AttendancePct")) ? 0 : reader.GetDecimal(reader.GetOrdinal("AttendancePct"))
                });
            }
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }

        return result;
    }
}
