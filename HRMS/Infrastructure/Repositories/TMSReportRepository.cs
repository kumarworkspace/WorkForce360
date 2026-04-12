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

    public async Task<TMSOverallSummaryDto> GetOverallSummaryAsync(TMSReportFilter filter)
    {
        await _context.Database.OpenConnectionAsync();
        try
        {
            using var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = "SELECT * FROM usp_GetTMSOverallSummary($1,$2,$3,$4,$5)";

            cmd.Parameters.Add(new NpgsqlParameter { Value = filter.TenantId,                                       NpgsqlDbType = NpgsqlDbType.Integer });
            cmd.Parameters.Add(new NpgsqlParameter { Value = filter.FromDate.HasValue ? (object)filter.FromDate.Value.Date : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Date });
            cmd.Parameters.Add(new NpgsqlParameter { Value = filter.ToDate.HasValue   ? (object)filter.ToDate.Value.Date   : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Date });
            cmd.Parameters.Add(new NpgsqlParameter { Value = filter.TrainerId.HasValue ? (object)filter.TrainerId.Value     : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Integer });
            cmd.Parameters.Add(new NpgsqlParameter { Value = filter.CourseId.HasValue  ? (object)filter.CourseId.Value      : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Integer });

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new TMSOverallSummaryDto
                {
                    TotalCourses               = reader.GetInt32(reader.GetOrdinal("TotalCourses")),
                    TotalSessions              = reader.GetInt32(reader.GetOrdinal("TotalSessions")),
                    TotalParticipantsEnrolled  = reader.GetInt32(reader.GetOrdinal("TotalParticipantsEnrolled")),
                    TotalPresent               = reader.GetInt32(reader.GetOrdinal("TotalPresent")),
                    AvgAttendancePercentage    = reader.GetDecimal(reader.GetOrdinal("AvgAttendancePercentage")),
                    CoursesCompleted           = reader.GetInt32(reader.GetOrdinal("CoursesCompleted")),
                    CoursesOngoing             = reader.GetInt32(reader.GetOrdinal("CoursesOngoing")),
                    CoursesUpcoming            = reader.GetInt32(reader.GetOrdinal("CoursesUpcoming"))
                };
            }

            return new TMSOverallSummaryDto();
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }

    public async Task<IEnumerable<TMSMonthlySummaryDto>> GetMonthlySummaryAsync(TMSReportFilter filter)
    {
        await _context.Database.OpenConnectionAsync();
        try
        {
            using var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = "SELECT * FROM usp_GetTMSMonthlySummary($1,$2,$3,$4,$5)";

            cmd.Parameters.Add(new NpgsqlParameter { Value = filter.TenantId,                                       NpgsqlDbType = NpgsqlDbType.Integer });
            cmd.Parameters.Add(new NpgsqlParameter { Value = filter.FromDate.HasValue ? (object)filter.FromDate.Value.Date : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Date });
            cmd.Parameters.Add(new NpgsqlParameter { Value = filter.ToDate.HasValue   ? (object)filter.ToDate.Value.Date   : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Date });
            cmd.Parameters.Add(new NpgsqlParameter { Value = filter.TrainerId.HasValue ? (object)filter.TrainerId.Value     : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Integer });
            cmd.Parameters.Add(new NpgsqlParameter { Value = filter.CourseId.HasValue  ? (object)filter.CourseId.Value      : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Integer });

            var result = new List<TMSMonthlySummaryDto>();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new TMSMonthlySummaryDto
                {
                    Year                    = reader.GetInt32(reader.GetOrdinal("Year")),
                    Month                   = reader.GetInt32(reader.GetOrdinal("Month")),
                    MonthName               = reader.GetString(reader.GetOrdinal("MonthName")),
                    TotalSessions           = reader.GetInt32(reader.GetOrdinal("TotalSessions")),
                    TotalParticipants       = reader.GetInt32(reader.GetOrdinal("TotalParticipants")),
                    TotalPresent            = reader.GetInt32(reader.GetOrdinal("TotalPresent")),
                    AvgAttendancePercentage = reader.GetDecimal(reader.GetOrdinal("AvgAttendancePercentage"))
                });
            }

            return result;
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }

    public async Task<IEnumerable<TMSTrainerPerformanceDto>> GetTrainerPerformanceAsync(TMSReportFilter filter)
    {
        await _context.Database.OpenConnectionAsync();
        try
        {
            using var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = "SELECT * FROM usp_GetTMSTrainerPerformance($1,$2,$3,$4)";

            cmd.Parameters.Add(new NpgsqlParameter { Value = filter.TenantId,                                       NpgsqlDbType = NpgsqlDbType.Integer });
            cmd.Parameters.Add(new NpgsqlParameter { Value = filter.FromDate.HasValue ? (object)filter.FromDate.Value.Date : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Date });
            cmd.Parameters.Add(new NpgsqlParameter { Value = filter.ToDate.HasValue   ? (object)filter.ToDate.Value.Date   : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Date });
            cmd.Parameters.Add(new NpgsqlParameter { Value = filter.TrainerId.HasValue ? (object)filter.TrainerId.Value     : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Integer });

            var result = new List<TMSTrainerPerformanceDto>();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new TMSTrainerPerformanceDto
                {
                    StaffId                  = reader.GetInt32(reader.GetOrdinal("StaffId")),
                    TrainerName              = reader.GetString(reader.GetOrdinal("TrainerName")),
                    EmployeeCode             = reader.IsDBNull(reader.GetOrdinal("EmployeeCode")) ? null : reader.GetString(reader.GetOrdinal("EmployeeCode")),
                    Department               = reader.IsDBNull(reader.GetOrdinal("Department"))   ? null : reader.GetString(reader.GetOrdinal("Department")),
                    TotalCoursesConducted    = reader.GetInt32(reader.GetOrdinal("TotalCoursesConducted")),
                    TotalParticipantsTrained = reader.GetInt32(reader.GetOrdinal("TotalParticipantsTrained")),
                    AvgAttendancePercentage  = reader.GetDecimal(reader.GetOrdinal("AvgAttendancePercentage"))
                });
            }

            return result;
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }

    public async Task<IEnumerable<TMSCourseWiseReportDto>> GetCourseWiseReportAsync(TMSReportFilter filter)
    {
        await _context.Database.OpenConnectionAsync();
        try
        {
            using var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = "SELECT * FROM usp_GetTMSCourseWiseReport($1,$2,$3,$4,$5)";

            cmd.Parameters.Add(new NpgsqlParameter { Value = filter.TenantId,                                       NpgsqlDbType = NpgsqlDbType.Integer });
            cmd.Parameters.Add(new NpgsqlParameter { Value = filter.FromDate.HasValue ? (object)filter.FromDate.Value.Date : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Date });
            cmd.Parameters.Add(new NpgsqlParameter { Value = filter.ToDate.HasValue   ? (object)filter.ToDate.Value.Date   : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Date });
            cmd.Parameters.Add(new NpgsqlParameter { Value = filter.TrainerId.HasValue ? (object)filter.TrainerId.Value     : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Integer });
            cmd.Parameters.Add(new NpgsqlParameter { Value = filter.CourseId.HasValue  ? (object)filter.CourseId.Value      : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Integer });

            var result = new List<TMSCourseWiseReportDto>();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new TMSCourseWiseReportDto
                {
                    CourseId                  = reader.GetInt32(reader.GetOrdinal("CourseId")),
                    CourseTitle               = reader.GetString(reader.GetOrdinal("CourseTitle")),
                    CourseCode                = reader.IsDBNull(reader.GetOrdinal("CourseCode"))  ? null : reader.GetString(reader.GetOrdinal("CourseCode")),
                    Category                  = reader.IsDBNull(reader.GetOrdinal("Category"))    ? null : reader.GetString(reader.GetOrdinal("Category")),
                    TotalSessions             = reader.GetInt32(reader.GetOrdinal("TotalSessions")),
                    TotalParticipantsEnrolled = reader.GetInt32(reader.GetOrdinal("TotalParticipantsEnrolled")),
                    TotalPresent              = reader.GetInt32(reader.GetOrdinal("TotalPresent")),
                    AvgAttendancePercentage   = reader.GetDecimal(reader.GetOrdinal("AvgAttendancePercentage"))
                });
            }

            return result;
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }
}
